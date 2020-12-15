using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Taxonomy
{
    public class ForceOutlineToTaxonomy : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ForceOutlineToTaxonomy class.
        /// </summary>
        public ForceOutlineToTaxonomy()
          : base("Outline To Taxonomy", "FromOutLine",
              "Extraxt a taxonomy form list of outline lines",
              Properties.Resources.Cat, Properties.Resources.Sub)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Code", "S", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
       

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {

            pManager.RegisterParam(new ParamTaxonomy(), GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var typeid = 0;
            var ss = new List<string>();
            // DA.GetData(1, ref typeid);
            DA.GetDataList(0, ss); 
            TreeStructure tree; 
            ss.RemoveAll(i => Regex.IsMatch(i, "^\\s+$")); 
            tree = TreeStructure.FromOutlineHrdCore(ss);
            DA.SetData(0, tree);

        }


        


     
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Properties.Resources.t_lay2t;
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("31d0a00e-4783-4a85-9268-d3d5584566bd");
    }
}