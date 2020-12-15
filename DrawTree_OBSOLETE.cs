using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Taxonomy
{
    public class DrawTree_OBSOLETE : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DrawTree class.
        /// </summary>
        public DrawTree_OBSOLETE()
          : base("CustomDrawTree", "DrawTree",
              "DrawTree",
              Properties.Resources.Cat, Properties.Resources.Sub)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new ParamTaxonomy(), "Tree", "T", "Tree Structure item", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "C", "Curves to represent tree on", GH_ParamAccess.list);

        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            
            pManager.Register_CurveParam("Curves","C","Connection Curves",GH_ParamAccess.tree);
            pManager.RegisterParam(new ParamTaxoNode(), "AllNodes", "N", "", GH_ParamAccess.tree);
            pManager.RegisterParam(new ParamTaxoNode(),  "Heads", "H", "", GH_ParamAccess.list);  
            pManager.RegisterParam(new ParamTaxoNode(), "Middles", "M", "", GH_ParamAccess.list);
            pManager.RegisterParam(new ParamTaxoNode(), "Pseudos", "P", "", GH_ParamAccess.list);


        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            treeGoo treeg=new treeGoo();
            DA.GetData(0, ref treeg);
            List<Curve> curves=new List<Curve>();
            DA.GetDataList(1,  curves);
         
          


            var layers = treeg.Value.GetLayers(curves);



            
            var pp = layers.SelectMany(la => la.Select(i => i.PV.Point)).ToList();
          
            var Pointset = new DataTree<GeoNode>();
            var TigSet=new DataTree<Curve>();
            


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

   


        string jeco(IEnumerable<double> oo)
            => string.Join("|", oo.Select(i=>(i*100).ToString("00.0")));

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.DrawTree2;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2aa14aa2-9c7c-48c4-b78b-6cdf929c71fe");
    }

   
}