using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace mu
{

    public struct InterNode : IComparable
    {

        public int Index; 
        public int Level; 



        public InterNode(int a, int b)
        {
            Level = a;
            Index = b;
        }

        public InterNode(InterNode h)
        {
            Level = h.Level; 
            Index = h.Index;
        }

        public InterNode(HyperindexGoo hyperindexGoo)
        {
            Level = hyperindexGoo.Value.Level;
            Index = hyperindexGoo.Value.Index;
        }

        public InterNode(GH_Path path)
        {
            var t = path.Indices; 
            Level =t[t.Length-2];
            Index =t.Last();  
        }

        public InterNode(GH_Path path, int i)
        {
            Index = i;
            Level = path.Indices.Last();     
        }



        public GH_Path Path  =>  new GH_Path(Level);
     

     

        public int CompareTo(object obj)
        {
            if (obj is InterNode b)
                return Level != b.Level ? this.Level.CompareTo(b.Level) : this.Index.CompareTo(b.Index);
            else return 1;
        }
            

        public override string ToString() => $"{Level}:{Index}"; 
     public int OveralIndex(List<int> k) => k.Take(Level).Sum() + Index;
        public GH_Path ToPath() => new GH_Path(Level, Index);
        public GH_Path ToPath(GH_Path prepend) => new GH_Path() { Indices = prepend.Indices.Concat(new[] { Level, Index }).ToArray() };

        public static InterNode FromString(string s)
        { 
         var m=   Regex.Match(s, "(?<level>\\d+)[:;,\\(\\)|\\^\\*](?<index>\\d+)\\)?$");
            return new InterNode(int.Parse(m.Groups["level"].Value), int.Parse(m.Groups["index"].Value));
        }
    }


    public sealed class HyperindexGoo : GH_Goo<InterNode>
    {
        public HyperindexGoo() => Value = new InterNode(-1, -1);
        public HyperindexGoo(InterNode v) => Value = v;
        public HyperindexGoo(GH_Path p, int i) => Value = new InterNode(p, i);
        public override bool IsValid => Value.Index > -1;
        public override string TypeName => "HyperIndex";
        public override string TypeDescription => "Address to specific item in a data tree";
        public override IGH_Goo Duplicate() => new HyperindexGoo(Value);
        public override string ToString() =>Value.ToString();
        public override bool CastFrom(object source)
        {
            if (source is GH_StructurePath m)
            {
                var p = m.Value;
                if (p.Length < 2) return false;
                Value = new InterNode(p.CullElement(), p.Indices.Last());
                return true;
            }


            try
            {

                var gs = source.ToString();
                Value = InterNode.FromString(gs);
                return true;
            }
            catch (Exception)
            {

                return base.CastFrom(source);
            }

        }

        public override bool CastTo<TQ>(ref TQ target)
        {
            if (typeof(TQ).IsAssignableFrom(typeof(GH_StructurePath)))
            {
                target = (TQ)(object)new GH_StructurePath(Value.Path);
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(GH_Integer)))
            {
                target = (TQ) (object) new GH_Integer(Value.Index);
                return true;
            }

          


            return base.CastTo(ref target);




        }
    }

    public class HyperItemParam : GH_Param<HyperindexGoo>
    {
        public HyperItemParam() :
            base(new GH_InstanceDescription("ItemHyperIndex", "h.Index"
                , "Item Hyperindex", "Params", "Primitive"))
        {
        }


        public override Guid ComponentGuid => new Guid("{4B3C49A5-DADA-4276-8567-EBCF967A143C}");
        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        protected override Bitmap Icon => Taxonomy.Properties.Resources.hgX;  
       
    }

}
