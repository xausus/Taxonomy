using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Taxonomy
{
    public class DrawingBaseArc_OBSOLETE : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DrawingBaseArc class.
        /// </summary>
        public DrawingBaseArc_OBSOLETE()
          : base("DrawingBaseArc", "DrawingBaseArc",
              "DrawingBaseArc",
              Properties.Resources.Cat, Properties.Resources.Sub)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new ParamTaxonomy(), "Tree", "T", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Center", "P", "Points", GH_ParamAccess.item,Point3d.Origin);
            pManager.AddNumberParameter("Domain", "D", "Pie Interval", GH_ParamAccess.item,.5);
            pManager.AddNumberParameter("TN", "", "", GH_ParamAccess.item,.8);
            pManager.AddNumberParameter("AN", "", "", GH_ParamAccess.item,5);
            pManager.AddNumberParameter("SN", "", "", GH_ParamAccess.item,5); 
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddArcParameter("Arc", "A", "Arcs", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            TreeStructure t =null;
            double tn, sn, an, pie;
            tn = sn = an = pie = 0;
            Point3d cen=new Point3d();
            DA.GetData(0, ref t);
            DA.GetData("Center", ref cen);  
            DA.GetData("Domain", ref pie);
            DA.GetData("TN", ref tn);
            DA.GetData("AN", ref sn);
            DA.GetData("SN", ref an); 

            var dom = new Interval((pie - 1) * Math.PI / 2, (3 - pie) * Math.PI / 2);
            var arcs= Enumerable.Range(0, t.Depth).Select(i=>
            new Arc( new Circle(cen, sn * Math.Pow( i,tn) + an), dom)).ToList();
            DA.SetDataList(0, arcs);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
       // protected override System.Drawing.Bitmap Icon => IconGenerator.Icon("ARC", Color.Aquamarine, Brushes.Sienna);


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("25636a8e-ed3f-49f2-95b5-06dd2bbc3d0a"); }
        }
    }
}