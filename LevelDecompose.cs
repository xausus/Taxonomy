using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Taxonomy
{
    public class LevelDecompose : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LevelDecompose class.
        /// </summary>
        public LevelDecompose()
          : base("T.L.Decompose", "TLD",
              "Tree Level Decompose",
              Properties.Resources.Cat, Properties.Resources.Sub)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new ParamTaxonomy(), "Tree", "T", "Tree Structure item", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.Register_IntegerParam("Items", "I", "Items clusatered in levels", GH_ParamAccess.tree);
            pManager.Register_IntegerParam("Level-Topology", "LT", "Children Topology seperated in Levels", GH_ParamAccess.tree);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var treeg = new treeGoo();
        
     
            DA.GetData(0, ref treeg);
            var tree = treeg.Value;
            DA.SetDataTree(0, tree.GetLevelsID());
            DA.SetDataTree(1, tree.GetLevelsTopology());



        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.p_hier2;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c6a382bd-1325-4173-a1be-74a8f3528e33");
    }
}