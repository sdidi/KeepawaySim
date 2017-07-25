using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Keepaway
{
    [Serializable]
    public class LinkGene : ICloneable, IComparable
    {
        private static int nextId = 0;
        [XmlAttribute]
        public int Id;
        [XmlAttribute]
        public int Source;
        [XmlAttribute]
        public int Target;
        [XmlAttribute]
        public double Weight;
        [XmlAttribute]
        public bool Fixed;
        internal bool Mutated;

        internal static int NextId
        {
            get
            {
                int num = LinkGene.nextId;
                ++LinkGene.nextId;
                return num;
            }
            set
            {
                LinkGene.nextId = value;
            }
        }

        public LinkGene(int id, int source, int target, double weight, bool isFixed, bool mutated)
        {
            this.Fixed = isFixed;
            this.Id = id;
            this.Source = source;
            this.Target = target;
            this.Weight = weight;
            this.Mutated = mutated;
        }

        public LinkGene(int id, int source, int target, double weight, bool isFixed)
            : this(id, source, target, weight, isFixed, false)
        {
        }

        public LinkGene(int id, int source, int target, double weight)
            : this(id, source, target, weight, false, false)
        {
        }

        public LinkGene()
            : this(0, 0, 0, 0.0, false, false)
        {
        }

        public LinkGene(LinkGene copy)
            : this(copy.Id, copy.Source, copy.Target, copy.Weight, copy.Fixed, copy.Mutated)
        {
        }

        public object Clone()
        {
            return (object)new LinkGene(this);
        }

        public int CompareTo(object obj)
        {
            LinkGene linkGene = obj as LinkGene;
            if (linkGene == null)
                throw new InvalidOperationException("Object being compared to is not a Link Gene!");
            return this.Id.CompareTo(linkGene.Id);
        }

        public int CompareTo(LinkGene obj)
        {
            return this.Id.CompareTo(obj.Id);
        }
    }
}
