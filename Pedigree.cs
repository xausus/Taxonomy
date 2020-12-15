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
    public class Pedigree : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Pedigree class.
        /// </summary>
        public Pedigree()
          : base("Pedigree", "Nickname",
              "Description",
              Properties.Resources.Cat, Properties.Resources.Sub)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new ParamTaxonomy(), "Tree", "T", "Tree Structure item", GH_ParamAccess.item);
            pManager.AddTextParameter("Deliminator", "D", "Deliminator", GH_ParamAccess.item,":");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Pedigree","P","Pedigree in Text Format",GH_ParamAccess.list);
            pManager.Register_StringParam("Pedigree","P","Pedigree in List Format",GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {


            var treeg = new treeGoo();
            string d = "";
            DA.GetData(1, ref d);
            DA.GetData(0, ref treeg);
            var tree = treeg.Value;
            var adds = tree.GetAddressList();
            var names = tree.NodeSet.Select(i => i.Name).ToList();
            var gset = adds.Select(i => i.Indices.Select(u => names[u]).ToList()).ToList();
            var r = new DataTree<string>();
            for (var i = 0; i < gset.Count; i++)
               r.AddRange(gset[i],new GH_Path(i));
            var rs = gset.Select(i => string.Join(d, i));
            DA.SetDataList(0, rs);
            DA.SetDataTree(1, r);


        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.p_Twig3;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c239119e-f30b-4a00-aa85-5f989473993c");
    }
}