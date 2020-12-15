using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

namespace Taxonomy
{
    public class Bono : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Bono class.
        /// </summary>
        public Bono()
            : base("Parent Text", "FROMTEXT",
                "Create a tree-struct from text",
                 Properties.Resources.Cat,Properties.Resources.Sub)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Code", "S", "", GH_ParamAccess.list);
         /*   pManager.AddIntegerParameter("Type", "T", "", GH_ParamAccess.item, 0);
            var pik = (pManager[1] as Param_Integer);
             for (var i = 0; i < types.Length; i++)
              pik.AddNamedValue(types[i],i);
             */
        }


        private string[] types = {"MarkDown","Outline", "JSON", "JSON*", "CSV (Pointer)", "CSV (OutLine)","???"};      
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {  
            pManager.RegisterParam(new ParamTaxonomy(),GH_ParamAccess.item);  
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var typeid=0;
            var ss = new  List<string>(); 
            // DA.GetData(1, ref typeid);
            DA.GetDataList(0,  ss);
            /*   ss = s.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(i => i != "").ToList();
          List<string> lines = new List<string>(
              input.Split(new string[] { "\r\n" },
                  StringSplitOptions.RemoveEmptyEntries));  */

            TreeStructure tree;


            if (TreeStructure.TryGetFromXML(ss[0], out tree))
            {
                DA.SetData(0, tree);
                Message = "XML";
                return;
            }
                var s = string.Join("\n\r", ss);
               ss = s.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(i=>i!="").ToList(); 
          
          
            typeid = 
                match(ss, R_Outline) ? 1 :
                match(ss, R_MarkDown) ? 0 :
                R_JSON.IsMatch(s) ? 2 :
                R_SJSON.IsMatch(s) ? 3 :
                match(ss, R_Pointer) ? 4:
                match(ss, R_CSVOL) ? 5 : 6;  
                Message = types[typeid];
            
            try
            {
                switch (typeid)
                {
                    case 0:tree=TreeStructure.FromMarkDown(ss);  break;
                    case 1:tree =TreeStructure.FromOutline(ss); break;  
                    case 2:tree = TreeStructure.FromJSON(s);  break;
                    case 3:tree = TreeStructure.FromSJSON(s);  break;   
                    case 4:tree =TreeStructure.FromPointerCSV(ss); break;
                    case 5:tree=TreeStructure.FromOutlineCSV(ss);  break; 
                    default:throw new Exception();  
                }



            }
            catch (Exception e)
            {
            
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,e.ToString()+"\nCouldn't recognize the string format\n"+e.Message);
            return;
            }
             
              
         
            DA.SetData(0, tree);
                
        }



        Regex R_Outline = new Regex(@"^(\W)?\1*\w+(\s+\w+)*$");
        Regex R_Pointer = new Regex(@"^\w+( \w+)*,(-1|\d+)$");
        Regex R_CSVOL = new Regex(@"^((\w| )+)?(,((\w| )+)?)+$");
        Regex R_MarkDown = new Regex(@"^(#+|\s*-)\s*.+", RegexOptions.CultureInvariant);
        private Regex R_JSON = new Regex(@"^{""name"":""\w.+?"",""id"":\d+,""level"":\d+,""children"":\[.+\]}$");
        private Regex R_SJSON = new Regex("^{\"(\\w.+)\":\\[(.+)\\]}$");



        bool match(List<string> ss, Regex r) => ss.Take(20).ToList()
            .TrueForAll(i =>  r.IsMatch(i));

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Properties.Resources.t_s2t;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6d4cdc1b-c252-4580-b990-45222a248f82");
    }
}