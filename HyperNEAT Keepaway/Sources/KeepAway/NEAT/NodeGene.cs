using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
namespace Keepaway
{
    [XmlInclude(typeof(NodeType))]
    [Serializable]
    public class NodeGene : ICloneable, IComparable
    {
        private static int nextId;
        [XmlAttribute]
        public int Id;
        [XmlAttribute]
        public NodeType Type;
        [XmlAttribute]
        public string Function;
        [XmlAttribute]
        public int Layer;
        [XmlAttribute]
        public bool Fixed;
        internal bool Mutated;

        internal static int NextId
        {
            get
            {
                int num = NodeGene.nextId;
                ++NodeGene.nextId;
                return num;
            }
            set
            {
                NodeGene.nextId = value;
            }
        }

        static NodeGene()
        {
            NodeGene.NextId = 0;
        }

        public NodeGene()
            : this(false, "Identity", 0, 0, false, NodeType.Hidden)
        {
        }

        public NodeGene(string function, int id, int layer, NodeType nodeType)
            : this(false, function, id, layer, false, nodeType)
        {
        }

        public NodeGene(bool nodeFixed, string function, int id, int layer, bool mutated, NodeType nodeType)
        {
            this.Fixed = nodeFixed;
            this.Function = function;
            this.Id = id;
            this.Layer = layer;
            this.Mutated = mutated;
            this.Type = nodeType;
        }

        public NodeGene(NodeGene copyFrom)
            : this(copyFrom.Fixed, copyFrom.Function, copyFrom.Id, copyFrom.Layer, copyFrom.Mutated, copyFrom.Type)
        {
        }

        public object Clone()
        {
            return (object)new NodeGene(this);
        }

        public int CompareTo(object obj)
        {
            NodeGene nodeGene = obj as NodeGene;
            if (nodeGene == null)
                throw new InvalidOperationException("Object being compared to is not a NodeGene!");
            return this.Id.CompareTo(nodeGene.Id);
        }

        public int CompareTo(LinkGene obj)
        {
            return this.Id.CompareTo(obj.Id);
        }
    }
}
