using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Taxonomy
{
    class treeGoo : GH_Goo<TreeStructure>
    {                              
        public override IGH_Goo Duplicate()  =>  MemberwiseClone() as IGH_Goo; 
        public override string ToString() => Value.ToString();   
        public override bool IsValid => Value != null;
        public override string TypeName => "Tree Struct";
        public override string TypeDescription => ""; 
        public override bool CastFrom(object source)
        {
            if (!(source is TreeStructure k)) return base.CastFrom(source);
            Value = k;
            return true;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (!typeof(Q).IsAssignableFrom(typeof(TreeStructure))) return base.CastTo(ref target);
            target = (Q)(object)Value;
            return true;

        }
    }

    class ParamTaxonomy : GH_Param<treeGoo>
    {
        public override Guid ComponentGuid => new Guid("{94D8AF50-0154-4C57-A657-EE33EADB0EDB}");
        protected override Bitmap Icon => Taxonomy.Properties.Resources.p_Taxonomy;

        public ParamTaxonomy()
            : base(new GH_InstanceDescription("Taxonomy", "T"
                , "A tree-structure of nodes", "Params", "Primitive"))

            {

            }

    }
}
