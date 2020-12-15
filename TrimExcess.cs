using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Taxonomy
{
    public class TrimExcess : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TrimExcess class.
        /// </summary>
        public TrimExcess()
          : base("TrimTree", "Trim",
        "Trim the outermost nodes",
        Properties.Resources.Cat, Properties.Resources.Sub)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new ParamTaxonomy(), "Tree", "T", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "N", "The successive number of trimming outermost", GH_ParamAccess.item,1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new ParamTaxonomy(), "Tree", "T", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            TreeStructure t = null;
            int n = 1;
            DA.GetData(0, ref t);
            DA.GetData(1, ref n);
            for (int i = 0; i < n; i++)
                t = t.Trim();

            DA.SetData(0, t);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        //  protected override System.Drawing.Bitmap Icon => IconGenerator.Icon("TR", Color.BurlyWood, Brushes.Sienna);
        protected override Bitmap Icon => Properties.Resources.t_trim2;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9103af79-f489-4cf0-846f-76ddcd46fa82");
    }
}