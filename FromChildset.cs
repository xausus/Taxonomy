using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Taxonomy
{
    public class FromChildset : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FromChildset class.
        /// </summary>
        public FromChildset()
          : base("FromChildset", "FromChildren",
              "Description",
              Properties.Resources.Cat, Properties.Resources.Sub)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", " A list of tagname refering to the nodes", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Topology", "T", "Children structure topology", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new ParamTaxonomy(), "TreeStructure", "T", "Data-Tree Structure", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetDataTree(1, out GH_Structure<GH_Integer> tpt);
            var names=new List<string>();
            DA.GetDataList(0, names); 
            var tree=new TreeStructure(names,TreeStructure.ToListStruct(tpt));
            DA.SetData(0, tree);   
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.p_Twig;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("21ee0a46-50be-439b-924f-4cbbd9488e05");
    }
}