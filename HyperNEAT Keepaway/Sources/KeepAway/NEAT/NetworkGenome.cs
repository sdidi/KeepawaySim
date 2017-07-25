using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Keepaway
{
    [XmlInclude(typeof(LinkGene))]
    [XmlInclude(typeof(SubstrateNodes))]
    [XmlInclude(typeof(NodeGene))]
    [XmlInclude(typeof(NodeType))]
    [XmlInclude(typeof(Behavior))]
    [XmlInclude(typeof(Diversity))]
    [Serializable]
    public class NetworkGenome : IComparable, ICloneable
    {
        private static int nextId = 0;
        private static XmlSerializer serializer = new XmlSerializer(typeof(NetworkGenome));
        
        [XmlArray]
        public List<NodeGene> Nodes;
        [XmlArray]
        public List<LinkGene> Links;
        //[XmlAttribute]
        public Behavior BehaviorType;
        //[XmlAttribute]
        public Diversity GenomeDiversity;
        [XmlAttribute]
        public int Id;
        [XmlAttribute]
        public double Fitness;
        [XmlAttribute]
        public double Novelty;
        [XmlAttribute]
        public double RealFitness;
        [XmlAttribute]
        public double Genotypic_Diversity;
        [XmlAttribute]
        public double Complexity;
        [XmlAttribute]
        public int Age;
        [XmlAttribute]
        public int Parent1;
        [XmlAttribute]
        public int Parent2;
        [XmlAttribute]
        public int Species;
        public ComputeNovelty NoveltyMeasure;
        internal IEnumerable<NodeGene> MutatableNodes;
        internal IEnumerable<LinkGene> MutatableLinks;
        int _nearestNeighbors;
        public static int NextId
        {
            get
            {
                int num = NetworkGenome.nextId;
                ++NetworkGenome.nextId;
                return num;
            }
            set
            {
                NetworkGenome.nextId = value;
            }
        }

        public int nearestNeighbors
        {
            get
            {
                return _nearestNeighbors;
            }
            set
            {
                _nearestNeighbors = value;
            }
        }
        public NetworkGenome(int id, int age, int parent1, int parent2, int species, double fitness, double novelty, double realFitness, double genotypic_Diversity )
        {
            this.Id = id;
            this.Age = age;
            this.Parent1 = parent1;
            this.Parent2 = parent2;
            this.Species = species;
            this.Fitness = fitness;
            this.RealFitness = realFitness;
            this.Genotypic_Diversity = genotypic_Diversity;
            this.GenomeDiversity = new Diversity();
            this.BehaviorType = new Behavior();
            this.NoveltyMeasure = new ComputeNovelty();
            this.Novelty = novelty;
            this.Nodes = new List<NodeGene>();
            this.Links = new List<LinkGene>();
           // this.Complexity = this.Links.Count;
            this.MutatableNodes = Enumerable.Where<NodeGene>((IEnumerable<NodeGene>)this.Nodes, (Func<NodeGene, bool>)(nodes =>
            {
                if (!nodes.Mutated && !nodes.Fixed)
                    return nodes.Type == NodeType.Hidden;
                return false;
            }));
            this.MutatableLinks = Enumerable.Where<LinkGene>((IEnumerable<LinkGene>)this.Links, (Func<LinkGene, bool>)(links =>
            {
                if (!links.Mutated)
                    return !links.Fixed;
                return false;
            }));
        }

        public NetworkGenome(int id, int age, int parent1, int parent2, int species)
            : this(id, age, parent1, parent2, species, 0.0,0.0,0.0,0.0)
        {
        }

        public NetworkGenome()
            : this(-1, 0, -1, -1, -1, 0.0,0.0,0.0,0.0)
        {
        }

        public NetworkGenome(NetworkGenome copy)
            : this(copy.Id, copy.Age, copy.Parent1, copy.Parent2, copy.Species, copy.Fitness, copy.Novelty, copy.RealFitness, copy.Genotypic_Diversity)
        {
            foreach (NodeGene nodeGene in copy.Nodes)
                this.Nodes.Add(nodeGene.Clone() as NodeGene);
            foreach (LinkGene linkGene in copy.Links)
                this.Links.Add(linkGene.Clone() as LinkGene);
            this.Complexity = this.Links.Count + this.Nodes.Count;
            //this.BehaviorType = copy.BehaviorType;
            //this.GenomeDiversity = copy.GenomeDiversity;
        }

        public static bool SaveToFile(NetworkGenome genome, string file)
        {
            using (StreamWriter streamWriter = new StreamWriter(file))
            {
                NetworkGenome.serializer.Serialize((TextWriter)streamWriter, (object)genome);
                return true;
            }
        }

        public static NetworkGenome LoadFromFile(string file)
        {
            using (StreamReader streamReader = new StreamReader(file))
                return NetworkGenome.serializer.Deserialize((TextReader)streamReader) as NetworkGenome;
        }

        internal void ResetStats()
        {
            this.Age = 0;
            this.Fitness = 0;
            this.Genotypic_Diversity = 0;
            this.Novelty = 0;
            this.Complexity = 0;
            this.nearestNeighbors = 0;
            }

        public int CompareTo(object obj)
        {
            NetworkGenome networkGenome = obj as NetworkGenome;
            if (networkGenome == null)
                throw new InvalidOperationException("Object being compared to is not a Network Genome!");
            return this.Id.CompareTo(networkGenome.Id);
        }

        public int CompareTo(NetworkGenome obj)
        {
            return this.Id.CompareTo(obj.Id);
        }

        public object Clone()
        {
            return (object)new NetworkGenome(this);
        }
    }
}
