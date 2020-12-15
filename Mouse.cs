using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace Taxonomy
{
    public class Mouse : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Mouse class.
        /// </summary>
        public Mouse()
          : base("Mouse", "Nickname",
              "Description",
              Properties.Resources.Cat, Properties.Resources.Sub)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("A", "", "", GH_ParamAccess.item,false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("X","","",GH_ParamAccess.item);  
        pManager.Register_IntegerParam("B","","",GH_ParamAccess.item);

        var w = pManager[1] as Param_Integer;
        w .AddNamedValue("Non",0);
        w .AddNamedValue("Left",1);
        w .AddNamedValue("Middle",2);
        w .AddNamedValue("Right",3);


        }

        private static readonly List<MouseButtons> pmo = new List<MouseButtons>{MouseButtons.None,MouseButtons.Left,MouseButtons.Middle,MouseButtons.Right};

        /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool active = false;
          DA.GetData(0, ref active);
          if(!active)return;
          var q = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;  
          var pp=  q.ClientToScreen(Cursor.Position);
          var line = q.ClientToWorld(pp);
          if(!  Intersection.LinePlane( line,q.ConstructionPlane()
              , out var w))  return;

          var point = line.PointAt(w);
          DA.SetData(0, point);
          var cl=pmo.IndexOf(  Control.MouseButtons);
          DA.SetData(1, cl);
         ExpireSolution(true);




        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => IconGenerator.Icon("MOUSE",Color.Lime, Brushes.DarkGreen);


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("40775022-fe04-4b4b-8f2d-231e9b850835");
    }
}