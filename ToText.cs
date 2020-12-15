using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Taxonomy
{
    public class ToText : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ToText()
          : base("Tree Child String", "TreeToString",
              "Explode tree to string",
               Properties.Resources.Cat,Properties.Resources.Sub)
        {
          
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new ParamTaxonomy(), "TREE", "T", "Dos", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("MarkDown","MDN","",GH_ParamAccess.list);
            pManager.Register_StringParam("Outline","OLN","",GH_ParamAccess.list);
            pManager.Register_StringParam("Pointer CSV","POI","",GH_ParamAccess.list);
            pManager.Register_StringParam("Outline CSV","CSV","",GH_ParamAccess.list); 
            pManager.Register_StringParam("JSON","JSN","",GH_ParamAccess.item); 
            pManager.Register_StringParam("Simple JSON","JSN","",GH_ParamAccess.item); 

            pManager.Register_StringParam("XML","XML","",GH_ParamAccess.item); 
           pManager.Register_StringParam("Simple XML","XML","",GH_ParamAccess.item); 


        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            treeGoo t = null;
            DA.GetData(0, ref t);
            var tree = t.Value;
            try
            {  
            DA.SetDataList("MarkDown", tree.ToMarkDown());
            DA.SetDataList("Outline", tree.ToOutline());
            DA.SetDataList("Pointer CSV", tree.ToPointerCSV());
            DA.SetDataList("Outline CSV", tree.ToOutlineCSV());
            DA.SetData("JSON", tree.ToJSON());
            DA.SetData("Simple JSON", tree.ToSimpleJSON());
            DA.SetData("XML", tree.ToXML());
            DA.SetData("Simple XML", tree.ToSimpleXML());
           }
            catch
            {
            }
        }

        


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.p_TreeToText;


        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("84c72665-9393-49d9-b490-1ba6cf2ab1a5");
    }
}
