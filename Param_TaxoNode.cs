using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Taxonomy
{
    public class ParamTaxoNode : GH_Param<TNodeGoo>
    {
        public ParamTaxoNode() : base(new GH_InstanceDescription("Taxonomy Node","N"
            ,"Geometric Tree Node","Params","Primitive") )
        {
        }

      //  protected override Bitmap Icon => IconGenerator.Icon("N", Color.LightGray, Brushes.Black);
        public override Guid ComponentGuid =>new Guid("{EAE64990-E3FD-4A81-9E21-BAA9D9DFF2E2}");
        protected override Bitmap Icon => Taxonomy.Properties.Resources.NodeText;
    }

    public class TNodeGoo : GH_Goo<GeoNode>
    {
        public override IGH_Goo Duplicate() => MemberwiseClone() as IGH_Goo;

        public override string ToString() => Value.ToString();
        public override bool IsValid { get; }
        public override string TypeName { get; }
        public override string TypeDescription { get; }
        public override bool CastFrom(object source)
        {
            if (source is GeoNode k)
            {
                Value = k;
                return true;
            }
          


            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(ref Q target)
        {   var pv = Value.PV;
            if (typeof(Q).IsAssignableFrom(typeof(GeoNode)))
            {
                target = (Q)(object)Value;
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(GH_Integer)))
            {
                target = (Q)(object)new GH_Integer(Value.LayerID)  ;
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(GH_Plane)))
            {
                

                target = (Q) (object) new GH_Plane(new Plane(pv.Point, pv.Vector,
                    Vector3d.CrossProduct(Vector3d.ZAxis, pv.Vector)));
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(GH_Point)))
            {
                target = (Q)(object)new GH_Point(pv.Point);
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(GH_Vector)))
            {
                target = (Q)(object)new GH_Vector(pv.Vector);
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(GH_String)))
            {
                target = (Q)(object)new GH_String(Value.Node.Name);
                return true;
            }

            return base.CastTo(ref target);



        }
    }
}
