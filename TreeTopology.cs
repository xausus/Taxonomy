using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Point = Rhino.Geometry.Point;

namespace Taxonomy
{
    public class TreeTopology : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TreeTopology class.
        /// </summary>
        public TreeTopology()
          : base("TreeDecompose", "TreeTopo",
              "Decompose a datatree",
               Properties.Resources.Cat,Properties.Resources.Sub)
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
               
                 pManager.Register_StringParam("Names","N","Nodes' Name",GH_ParamAccess.list);
                 pManager.Register_IntegerParam("Levels","L","Nodes' Level",GH_ParamAccess.list);
                 pManager.Register_IntegerParam("Parent","P","Nodes' Parent",GH_ParamAccess.list);
                 pManager.Register_PathParam("Address","A","Address of node in GhPath format" ,GH_ParamAccess.list);
                 pManager.Register_IntegerParam("Topology","T", "Children Topology", GH_ParamAccess.tree);
                 pManager.Register_IntegerParam("Connections","C", "Connection-Node Topology", GH_ParamAccess.tree);


        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            treeGoo treeGoo = new treeGoo();
            DA.GetData(0, ref treeGoo);
            var tree = treeGoo.Value;

            
            DA.SetDataList(0, tree.NodeSet.Select(i => i.Name));
            DA.SetDataList(1, tree.NodeSet.Select(i => i.Level));
            DA.SetDataList(2, tree.NodeSet.Select(i => i.Parent.ID));
            DA.SetDataList(3, tree.GetAddressList());
            DA.SetDataTree(4,tree.ToNN());
            DA.SetDataTree(5,tree.ToCN()); 



        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        //  protected override System.Drawing.Bitmap Icon => IconGenerator.Icon("TP", Color.White, Brushes.Black);  
        protected override Bitmap Icon => Properties.Resources.p_hierarchy;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("97f954f1-a754-4e98-9eaf-28e7c443ed75");
    }
}