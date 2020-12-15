using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Taxonomy
{
    public class DendoGrapher : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GeneratDrawTree class.
        /// </summary>
        public DendoGrapher()
          : base("Dendrograph", "Dendrograph",
              "Draw a circular dendrograph of the input taxonomy",
              Properties.Resources.Cat, Properties.Resources.Sub)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new ParamTaxonomy(), "Tree", "T", "Tree Structure item", GH_ParamAccess.item);
            pManager.AddPointParameter("Center", "P", "Points", GH_ParamAccess.item, Point3d.Origin);
            pManager.AddNumberParameter("Domain", "D", "Pie Interval", GH_ParamAccess.item, .5);
            pManager.AddNumberParameter("TN", "", "", GH_ParamAccess.item, .8);
            pManager.AddNumberParameter("AN", "", "", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("SN", "", "", GH_ParamAccess.item, 5);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {  
            pManager.Register_CurveParam("Curves", "C", "Connection Curves", GH_ParamAccess.tree);
            pManager.RegisterParam(new ParamTaxoNode(), "AllNodes", "N", "", GH_ParamAccess.tree);
            pManager.RegisterParam(new ParamTaxoNode(), "Heads", "H", "", GH_ParamAccess.list);
            pManager.RegisterParam(new ParamTaxoNode(), "Middles", "M", "", GH_ParamAccess.list);
            pManager.RegisterParam(new ParamTaxoNode(), "Pseudos", "P", "", GH_ParamAccess.list); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            treeGoo treeg = new treeGoo();
            DA.GetData(0, ref treeg);

            TreeStructure t = null;
            double tn, sn, an, pie;
            tn = sn = an = pie = 0;
            Point3d cen = new Point3d();
            DA.GetData(0, ref t);
            DA.GetData("Center", ref cen);
            DA.GetData("Domain", ref pie);
            DA.GetData("TN", ref tn);
            DA.GetData("AN", ref sn);
            DA.GetData("SN", ref an);

            var dom = new Interval((pie - 1) * Math.PI / 2, (3 - pie) * Math.PI / 2);
            var arcs = Enumerable.Range(0, t.Depth+1).Select(i =>
                new Arc(new Circle(cen, sn * Math.Pow(i, tn) + an), dom)).ToList();   
            var layers = treeg.Value.GetLayers(arcs.Select(i=>i.ToNurbsCurve()));  
            var Pointset = new DataTree<GeoNode>();
            var TigSet = new DataTree<Curve>();    
            foreach (var gLayer in layers)
            {
                var path = new GH_Path(gLayer.Level);
                Pointset.AddRange(gLayer.ToList(), path);
                TigSet.AddRange(gLayer.Draw(), path);
            }
            TigSet.RemovePath(TigSet.Paths.Last());
            DA.SetDataTree(0, TigSet);
            DA.SetDataTree(1, Pointset);
            var m = layers.ByOuterMosts();
            DA.SetDataList(2, m[GLayerSet.dets.Outermodes]);
            DA.SetDataList(3, m[GLayerSet.dets.InMiddles]);
            DA.SetDataList(4, m[GLayerSet.dets.Invalid]);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.t_Dendogram;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("80f2fafc-04d9-4ff1-b944-7b6319d4b1c7");
    }
}