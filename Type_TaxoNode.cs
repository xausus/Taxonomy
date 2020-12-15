using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Taxonomy.Properties;


namespace Taxonomy
{
    public class GeoNode
    {
        internal List<GeoNode> _childs;


        private GeoNode _offspring;
        public int Id;
        public int LayerID;

        public TaxoNode Node;
        public GeoNode Parent;
        public double pos;
        public PVM PV;

        /// <summary>
        ///     Create New Geonode based on a treenode
        /// </summary>
        /// <param name="treeNode"></param>
        public GeoNode(TaxoNode treeNode)
        {
            Node = treeNode;
            LayerID = treeNode.Level;
            if (treeNode.Parent is null)
            {
                Parent = null;
                return;
            }

            Parent = treeNode.Parent.Geo;
            Parent.Childs.Add(this);
        }


        public GeoNode(int layerId)
        {
            LayerID = layerId;
        }

        private GeoNode(GeoNode parent)
        {
            LayerID = parent.LayerID + 1;
            Parent = parent;
        }

        public List<GeoNode> Childs => _childs ?? (_childs = new List<GeoNode>());

        public bool IsOuterMost => IsValid && Node.IsOutermost;
        public bool IsValid => !(Node is null);

        public GeoNode Offspring
        {
            get
            {
                if (!(_offspring is null)) return _offspring;
                _offspring = new GeoNode(this) {Parent = this};
                _childs = new List<GeoNode> {_offspring};
                return _offspring;
            }
        }

        public static GeoNode Nan => new GeoNode(-1);

        public override string ToString()
        {
            return IsValid ? Node.Name : "Psudo Node";
        }

        public List<Curve> Draw()
        {
            return Childs.Where(i => i.IsValid).Select(i => OrganicDraw(PV, i.PV)).ToList();
        }

        public void SolvePosition()
        {
            pos = (Childs.First().pos + Childs.Last().pos) / 2;
        }

        private Curve OrganicDraw(PVM a, PVM b)
        {
            if (a.Mother.pos == b.Mother.pos)
                return new LineCurve(a.Point, b.Point);

            var bino = a.DistanceTo(b) * .42;
            return Bezier(a.Point, a.Vector * bino, b.Point, -b.Vector * bino);
        }

        private Curve Bezier(Point3d p1, Vector3d v1, Point3d p2, Vector3d v2)
        {
            return new BezierCurve(new[] {p1, p1 + v1, p2 + v2, p2}).ToNurbsCurve();
        }

        public void SetPV(Curve c)
        {
            var t = c.Domain.ParameterAt(pos);
            PV = new PVM
            {
                Point = c.PointAt(t),
                Vector = Vector3d.CrossProduct(c.TangentAt(t), Vector3d.ZAxis),
                Mother = this
            };
        }


        public class PVM
        {
            public GeoNode Mother;
            public Point3d Point;
            public Vector3d Vector;

            public double DistanceTo(PVM b)
            {
                return Point.DistanceTo(b.Point);
            }
        }
    }

    internal class GLayer : List<GeoNode>
    {
        public int Level;


        public GLayer(int level)
        {
            Level = level;
        }

        public List<Point3d> Points => this.Where(i => i.IsValid).Select(i => i.PV.Point).ToList();

        public void SetPVs(Curve c)
        {
            ForEach(i => i.SetPV(c));
        }

        public List<Curve> Draw()
        {
            return this.Where(i => i.IsValid).SelectMany(i => i.Draw()).ToList();
        }

        public void BasicRepose()
        {
            var stp = 1.0 / (Count - 1);
            for (var i = 0; i < Count; i++)
                this[i].pos = i * stp;
        }

        public void Repose()
        {
            ForEach(i => i.SolvePosition());
        }
    }

    public class TaxoNode : IComparable
    {
        private int _backParID = -1;
        private GeoNode _geo;
        public List<TaxoNode> Childs = new List<TaxoNode>();
        public int ID;

        public int Level;
        public string Name;
        public int OBSID;
        public TaxoNode Parent;


        public TaxoNode()
        {
            Childs = new List<TaxoNode>();
        }

        public TaxoNode(int level, string name)
        {
            Level = level;
            Name = name;
            Childs = new List<TaxoNode>();
        }
      
           static readonly Regex hookeRegex = new Regex("^(?<Leveler>\\W+)(?<Name>\\w.+$)");

        public TaxoNode(string line)
        { 
            var m = hookeRegex.Match(line);
                Level = m.Groups["Leveler"].Length;
                Name = m.Groups["Name"].Value.Trim(); 
            Childs = new List<TaxoNode>();
        }

        public GeoNode Geo => _geo ?? (_geo = new GeoNode(this));




        public GH_Path Address { get; set; }
        public bool IsOutermost => !Childs.Any();


        public int CompareTo(object obj)
        {
            if (obj is null) return -1;
            if (!(obj is TaxoNode other)) return -1;
            if (Parent is null) return -1;
            if (other.Parent is null) return 1;
            if (other.Parent == Parent) return Name.CompareTo(other.Name);
            if (Level == other.Level) return Parent.CompareTo(other.Parent);
            if (Level > other.Level)
                return Parent == other ? 1 : Parent.CompareTo(other);
            return this == other.Parent ? -1 : CompareTo(other.Parent);
        }

        public string ToCJson()
        {
            return "" +
                   "";
        }

        public string ToJsonString()
        {
            return 
                "{"+ Resources.name_  +
                   $":\"{Name}\"," +
                   Resources.id_ +
                   $":{ID}," +
                   Resources.level_ +
                   $":{Level}," +
                   Resources.twigs_ +
                   $":[{string.Join(",", Childs.Select(i => i.ToJsonString()))}]" +
                   "}";
        }

        public string ToSimpleJsonString()
        {
            return $"{{\"{Name}\"" +
                   $":[{string.Join(",", Childs.Select(i => i.ToSimpleJsonString()))}]" +
                   "}";
        }


        public XElement ToXMLelement()
        {
            var w = new XElement(Name);
            w.SetAttributeValue("id", ID);
            w.SetAttributeValue("level", Level);
            foreach (var treeNode in Childs)
                w.Add(treeNode.ToXMLelement());
            return w;
        }

        public XElement ToSimpleXMLement() => 
            new XElement(Name,Childs.Select(i=>i.ToSimpleXMLement()));

        public override string ToString() => Name;


        public string ToMarkD() => (Level < 3 ? new string('#', Level + 1) + " " : new string(' ', Level - 3) + "-") + " " + Name;

        public string ToOutL() => new string('.', Level) + Name;

        public void ParentSync() => Childs.ForEach(i => i.Parent = this);

        public TaxoNode Duplicate() => new TaxoNode {Name = Name, Level = Level};

        public TreeCon Add(TaxoNode a)
        {
           this.Childs.Add(a);
           a.Parent = this;
           return new TreeCon{Parent = this,Child = a};
        }
    }

    public struct TreeCon
    {
        public TaxoNode Parent;
        public TaxoNode Child;


    }

    internal class TreeStructure
    {
        private static readonly Regex MarkDownRegex = new Regex(@"^(?<level>#+|\s*-)\s*(?<name>\w.+)");


        private GLayerSet _bankLayer;

        private List<TreeCon> _connections = new List<TreeCon>();
        private List<TaxoNode> _nodes = new List<TaxoNode>();
        public int Depth;


        private static string patternx =
            $"{{" +
            Resources.name_ +
            $":\"(?<N>\\w.+)\"," +
            Resources.id_ +
            $":(?<id>\\d+),\"level\":(?<level>\\d+)," +
            Properties.Resources. twigs_ +
            $":\\[(?<C>.*)\\]}}";
 
           

       // private readonly Regex toNodeRegexOBS = new Regex(
       //      $"{{\"name\":\"(?<N>\\w.+)\",\"id\":(?<id>\\d+),\"level\":(?<level>\\d+),children:\\[(?<C>.*)\\]}}" ,
        //    RegexOptions.ExplicitCapture | RegexOptions.RightToLeft);

        private readonly Regex toNodeRegex = new Regex(
            $"{{\"name\":\"(?<N>\\w.+)\",.+,\"children\":\\[(?<C>.*)\\]}}",
            RegexOptions.ExplicitCapture | RegexOptions.RightToLeft);

        private readonly Regex toSNodeRegex = new Regex( 
            $"{{\"(?<N>\\w.+)\":\\[(?<C>.*)\\]}}",
            RegexOptions.ExplicitCapture | RegexOptions.RightToLeft);

        private TreeStructure()
        {
        }


        public TreeStructure (XElement x,bool hasID)
        {
          
           if(hasID)
              XML2Node(x,0);
              else 
              XML2Node(x, new TaxoNode { Name = x.Name.LocalName, Level = 0 }); 
          Sort();

        }

       

        public static bool TryGetFromXML(string s,out TreeStructure t)
        {
            try
            {
                var xml=XElement.Parse(s);
           
                t = new TreeStructure(xml, xml.Attribute("id")!=null); 
                return true; 
                
            }
            catch (Exception e)
            {
                t=new TreeStructure();
            
            }

            return false;  

        }

        public TreeStructure(string json)
        {
           
            J2Node(json, 0);   
            Sort();
        }
        public TreeStructure(string json, int root)
        {
            
                SJ2Node(json, root);  
             Sort();
        }



        public TreeStructure(List<string> names, List<List<int>> TP)
        {
            _nodes = names.Select(i => new TaxoNode {Name = i}).ToList();
            for (var p = 0; p < TP.Count; p++)
                foreach (var c in TP[p])
                   _connections.Add( _nodes[p].Add(_nodes[c]));


           Sort();
        }
         
        public TreeStructure(IEnumerable<string> names, IEnumerable<int> parents)
        {
            _nodes = names.Select((n, i) => new TaxoNode() {Name = n}).ToList();
              _connections=  _nodes.Zip(parents, (n, p) =>p>=0? _nodes[p].Add(n):new TreeCon()).ToList();
        Sort();
        }


        public List<TreeCon> Connections
        {
            get => _connections.ToList();
            private set => _connections = value;
        }


        public List<TaxoNode> NodeSet
        {
            get => _nodes.ToList();
            private set => _nodes = value;
        }

        private GLayerSet Layers => _bankLayer ?? ConstructLayers();
        public int MinDepth => GetOutermostNodes().Min(i => i.Level);


        public DataTree<int> ToNN()
        {
            var w = _nodes.Select(i => i.Childs.Select(u => u.ID).ToList()).ToList();
            var r = new DataTree<int>();
            for (var i = 0; i < w.Count; i++)
                r.AddRange(w[i], new GH_Path(i));
            return r;
        }

        public DataTree<int> ToCN()
        {
            var w = _connections.Select(i => new List<int> {i.Parent.ID, i.Child.ID}).ToList();
            var r = new DataTree<int>();
            for (var i = 0; i < w.Count; i++)
                r.AddRange(w[i], new GH_Path(i));
            return r;
        }


        public void Add(TaxoNode a)
        {
            a.ID = _nodes.Count;
            _nodes.Add(a);
        }


        public void AddByExistedParent(TaxoNode P, TaxoNode C)
        {
            Add(C);
            _connections.Add(P.Add(C) );
        }

        public void Add(TaxoNode P, TaxoNode C)
        {
            if (!_nodes.Contains(P)) Add(P);
            Add(C);
            _connections.Add(P.Add(C));

        }

        public static List<List<int>> ToListStruct(GH_Structure<GH_Integer> G)
        {
            return G.Branches.Select(i => i.Select(u => u.Value).ToList()).ToList();
        }

        public static List<List<string>> ToListStruct(GH_Structure<GH_String> G)
        {
            return G.Branches.Select(i => i.Select(u => u.Value).ToList()).ToList();
        }

        // private Regex SplitNodeRegex = new Regex("{[^{}]*(((?<Open>{)[^{}]*)+((?<Close-Open>})[^{}]*)+)*(?(Open)(?!))}");
        private TaxoNode J2Node(string s, int level)
        {
            var m = toNodeRegex.Match(s);
            
            var node = new TaxoNode
            { Name = m.Groups["N"].Value}; 
            _nodes.Add(node);
            var childmatch = m.Groups["C"].Value;   
            if (childmatch == "") return node;
            foreach (var pack in JSONchanker(childmatch))
                _connections.Add(node.Add(J2Node(pack, level + 1))); 
            return node;
        }
        private TaxoNode SJ2Node(string s, int level)
        {
            var m = toSNodeRegex.Match(s);

            var node = new TaxoNode
            {
                Name = m.Groups["N"].Value,
                OBSID = _nodes.Count,
                Level = level
            };
            _nodes.Add(node);
            var childmatch = m.Groups["C"].Value;

            if (childmatch == "") return node;
            foreach (var pack in JSONchanker(childmatch))
                _connections.Add( node.Add(SJ2Node(pack, level + 1)));

            return node;
        }
        private readonly XName ID_tag = "id";
        private readonly XName level_tag = "level";
        private TaxoNode XML2Node(XElement x,int level)
        {
  
            var node = new TaxoNode
            {
                Name = x.Name.LocalName,
                OBSID =Convert.ToInt32(x.Attribute(ID_tag).Value) ,
                Level = Convert.ToInt32(x.Attribute(level_tag).Value)
            };
            _nodes.Add(node);
            var childmatch =x.Elements();

  
            foreach (var element in childmatch)
                _connections.Add(node.Add( XML2Node(element, level + 1)));

            return node;
        }

        private TaxoNode XML2Node(XElement x, TaxoNode parent)
        {

            var node = new TaxoNode
            {
                Name = x.Name.LocalName,
                OBSID = _nodes.Count,
                Level = parent.Level+1
            };
            _nodes.Add(node);
            var childmatch = x.Elements(); 
            foreach (var element in childmatch) 
                _connections.Add(node.Add( XML2Node(element, node)));

            return node;
        }


        private List<string> JSONchanker(string a)
        {
            var r = new List<string>();
            var k = 0;
            var e = new StringBuilder();

            foreach (var c in a)
            {
                if (c == '{') k++;
                else if (c == '}') k--;
                e.Append(c);
                if (k > 0) continue;
                if (c == '}') r.Add(e.ToString());
                e.Clear();
            }

            return r;
        }


        public static TreeStructure FromJSON(string s) => new TreeStructure(s);
        public static TreeStructure FromSJSON(string s) => new TreeStructure(s,0);

        public static TreeStructure FromPointerCSV(List<string> ss)
        {
           
            var nodes = new List<TaxoNode>();
            var parents = new List<int>();
           
         
            foreach (var k in ss.Select(s => s.Split(',')))
            {
                 nodes.Add( new TaxoNode() {Name = k[0]});
                parents.Add(Convert.ToInt32(k[1]));
               
            }
          
            var tree = new TreeStructure();
            for (var i = 0; i < nodes.Count; i++)
            {
              if(parents[i]<0) continue;
              tree.Add(nodes[parents[i]],nodes[i]);
            }  
               
            tree.Sort();
           
            return tree;
           
        }

        /// <summary>
        ///     Basis a Tree by a list of nodes having 'Name' and 'ParentID'
        /// </summary>
        /// <param name="nodes">List of Nodes with 'name' and 'parent id' </param>
        /// <returns></returns>
       

        /// <summary>
        ///     Initiate the treestructure with nodes having their parents id
        /// </summary>
       
       


        private void Sort()
        {
            var si = _nodes.OrderBy(i => i.Parent).ToList();
      
            si[0].Level = 0;
            si.RemoveAt(0);
            si.ForEach(nod => nod.Level = nod.Parent.Level + 1);
            _nodes.Sort();
            Depth = NodeSet.Max(i => i.Level);
            for (var i = 0; i < _nodes.Count; i++) _nodes[i].ID = i;
        }

        public static TreeStructure FromOutlineCSV(List<string> csv)
        {
            var TR = new TreeStructure();
            var x = csv.Select(i => i.Split(',').ToList()).ToList();
            var size = x[0].Count;
            var pool = new TaxoNode[size];
            pool[0] = new TaxoNode {Name = x[0][0], Level = 0, Parent = null};
            TR.Add(pool[0]);
            foreach (var line in x)
            {
                var runi = false;
                for (var i = 1; i < size; i++)
                {
                    var a = line[i].Trim();
                    if (a == "")
                        if (runi) break;
                        else continue;

                    runi = true;
                    var nod = new TaxoNode {Name = a, Level = i};
                    pool[i] = nod;
                    TR.AddByExistedParent(pool[i - 1], nod);
                }
            }

            TR.Sort();
            return TR;
        }


        public List<string> ToMarkDown() => _nodes.Select(i => i.ToMarkD()).ToList();

        public string ToJSON() => _nodes[0].ToJsonString();
        public string ToSimpleJSON() => _nodes[0].ToSimpleJsonString();
       

        public object ToXML() => _nodes[0].ToXMLelement().ToString();
        public object ToSimpleXML() => _nodes[0].ToSimpleXMLement().ToString();

       
        public List<string> ToPointerCSV() => _nodes.Select(i => $"{i.Name},{i.Parent?.ID??-1}").ToList();

        public List<string> ToOutlineCSV()
        {
            var t = -1;
            var maxlevel = _nodes.Max(i => i.Level);
            var ss = new List<string>();
            var s = _nodes[0].Name;
            var nn = _nodes.ToList();
            nn.Add(new TaxoNode {Level = -1});
            Sort();


            foreach (var d in _nodes.Skip(1))
            {
                if (d.Level <= t)
                {
                    ss.Add(s + new string(',', maxlevel - t));
                    s = new string(',', d.Level) + d.Name;
                }
                else
                {
                    s += ',' + d.Name;
                }

                t = d.Level;
            }

            ss.Add(s + new string(',', maxlevel - t));
            return ss;
        }


        public List<string> ToOutline()
        {
            return _nodes.Select(i => i.ToOutL()).ToList();
        }


        public static TaxoNode MD2Node(string st)
        {
            var s = MarkDownRegex.Match(st);
            var k = s.Groups["level"].Value;
            var level = k.StartsWith("#") ? k.Length - 1 : k.Length + 2;
            return new TaxoNode {Name = s.Groups["name"].Value, Level = level};
        }


        public static TreeStructure FromNodes(IEnumerable<TaxoNode> nodes_e)
        {
            var nodes = nodes_e.ToList();
            var tree = new TreeStructure();
            var holder = new TaxoNode[nodes.Max(i => i.Level) + 1];
            holder[0] = nodes[0];

            foreach (var i in nodes.Skip(1))
            {
                var level = i.Level;
                holder[level] = i;
                tree.Add(holder[level - 1], i);
            }

            tree.Sort();
            return tree;
        }


        public override string ToString() => $"TreeStructure: Nodes:{_nodes.Count}  Layer:{Depth}";  
        public static TreeStructure FromMarkDown(List<string> ss) => FromNodes(ss.Select(MD2Node));  
        public static TreeStructure FromOutline(List<string> ss) => FromNodes(ss.Select(OUTL2Node));
        public static TreeStructure FromOutlineHrdCore(List<string> ss) => FromNodes(ss.Select(i=>new TaxoNode(i)));
        private static TaxoNode OUTL2Node(string st)
        {
            var OutLineRegexExtractor = new Regex(@"^(?<level>(\W)?\1*)(?<name>\w.+)$");
            var s = OutLineRegexExtractor.Match(st);
            return new TaxoNode {Name = s.Groups["name"].Value.Trim(), Level = s.Groups["level"]?.Length ?? 0};
        }

    

        public List<GH_Path> GetAddressList()
        {
            return _nodes.Select(GetAddress).ToList();
        }

        private GH_Path GetAddress(TaxoNode n)
        {
            return n.Address ??
                   (n.Address = n.Parent is null ? new GH_Path(0) : GetAddress(n.Parent).AppendElement(n.ID));
        }


        public DataTree<TaxoNode> GetLevels()
        {
            var g = _nodes.GroupBy(i => i.Level);
            var r = new DataTree<TaxoNode>();
            foreach (var m in g)

                r.AddRange(m, new GH_Path(m.Key));
            return r;
        }


        public DataTree<int> GetLevelsID()
        {
            var g = _nodes.GroupBy(i => i.Level);
            var r = new DataTree<int>();
            foreach (var m in g)

                r.AddRange(m.Select(i => i.ID), new GH_Path(m.Key));
            return r;
        }


        public DataTree<int> GetLevelsTopology()
        {
            var g = _nodes.GroupBy(i => i.Level);
            var r = new DataTree<int>();
            foreach (var m in g)
            foreach (var n in m)
                r.AddRange(n.Childs.Select(i => i.ID), new GH_Path(m.Key, n.ID));
            return r;
        }

        public Dictionary<int, List<TaxoNode>> GetLevelsDictionary()
        {
            return _nodes.GroupBy(i => i.Level).OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.ToList());
        }


        /// <summary>
        ///     Create And SetPose a List of GLayers in a Dictionary format
        /// </summary>
        /// <returns></returns>
        private GLayerSet ConstructLayers()
        {
            var w = GetLevelsDictionary();
            var nroot = w[0][0];
            var root = new GLayer(0);
            var subroot = new GLayer(1);
            _bankLayer = new GLayerSet {root, subroot};
            foreach (var b in nroot.Childs.Select(n => n.Geo))
            {
                subroot.Add(b);
                root.Add(new GeoNode(0) {Node = nroot, Childs = {b}});
            }

            for (var k = 1; k < w.Count - 1; k++)
            {
                var t = new GLayer(k + 1);
                _bankLayer.Add(t);
                foreach (var geoNode in _bankLayer[k])
                    if (geoNode.IsValid && geoNode.Node.Childs.Any())
                        t.AddRange(geoNode.Node.Childs.Select(i => i.Geo));
                    else
                        t.Add(geoNode.Offspring);
                for (var i = 0; i < t.Count; i++) t[i].Id = i;
            }


            SolveLayerPos();
            return _bankLayer;
        }


        private void SolveLayerPos()
        {
            _bankLayer.Last().BasicRepose();
            for (var i = _bankLayer.Count - 2; i >= 0; i--)
                _bankLayer[i].Repose();
        }


        public GLayerSet GetLayers(IEnumerable<Curve> curves)
        {
            if (Layers.Count != curves.Count())
                throw new Exception($"The number of curves (={curves.Count()}) doesn't equals " +
                                    $"tge number of layers (={Layers.Count}).");

            foreach (var x in Layers.Zip(curves, (a, b) => new {C = b, L = a}))
                x.L.SetPVs(x.C);
            return Layers;
        }

        private List<TaxoNode> GetOutermostNodes()
        {
            return NodeSet.Where(i => i.IsOutermost).ToList();
        }

        private List<TaxoNode> GetBranchingNodes()
        {
            return NodeSet.Where(i => !i.IsOutermost).ToList();
        }


        public TreeStructure Trim()
        {
            var s = GetBranchingNodes();
            var dic = new Dictionary<int, int> {{-1, -1}};

            for (var i = 0; i < s.Count; i++)
                dic.Add(s[i].ID, i);

            return new TreeStructure(s.Select(i => i.Name), s.Select(i => dic[i.Parent.ID]));
        }

       
    }


    internal class GLayerSet : List<GLayer>
    {
        private Dictionary<dets, List<GeoNode>> ByValids()
        {
            return this.Skip(1).SelectMany(gl => gl).GroupBy(i => i.IsValid)
                .ToDictionary(i => i.Key ? dets.Valid : dets.Invalid, i => i.ToList());
        }


        public Dictionary<dets, List<GeoNode>> ByOuterMosts()
        {
            var v = ByValids();
            var o = v[dets.Valid].GroupBy(i => i.IsOuterMost).Select(i => i.ToList()).ToArray();
            return new Dictionary<dets, List<GeoNode>>
            {
                {dets.InMiddles, o[0]},
                {dets.Outermodes, o[1]},
                {dets.Invalid, v[dets.Invalid]}
            };
        }

        internal enum dets
        {
            Valid,
            Invalid,
            InMiddles,
            Outermodes
        }
    }
}