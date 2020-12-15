using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Taxonomy
{
    public class FromPointer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FromPointer class.
        /// </summary>
        public FromPointer()
          : base("FromPointer", "FromPointer",
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
            pManager.AddIntegerParameter("Parent", "P", "The list of parent indexes", GH_ParamAccess.list);
            
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
            var names=new List<string>();
            var parents=new List<int>();
            DA.GetDataList(0, names); 
            DA.GetDataList(1, parents);
            var tree=new TreeStructure(names,parents);
            DA.SetData(0, tree);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.p_textbook;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fc7d0fc8-ac06-49eb-84de-7083bbf07a2b");
    }
}