//using NEAT.Genetics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Keepaway
{
    [XmlInclude(typeof(NetworkGenome))]
    [Serializable]
    public class Population : ICloneable
    {
        [XmlArray]
        public List<NetworkGenome> Genomes;
        [XmlAttribute]
        public int Generation;

        private static XmlSerializer Serial { get; set; }

        [XmlAttribute]
        public int Size
        {
            get
            {
                return this.Genomes.Count;
            }
            set
            {
            }
        }

        [XmlAttribute]
        public double MeanFitness
        {
            get
            {
                if (this.Size <= 0)
                    return 0.0;
                return this.TotalFitness / (double)this.Size;
            }
            set
            {
            }
        }

        [XmlAttribute]
        public double MeanRealFitness
        {
            get
            {
                if (this.Size <= 0)
                    return 0.0;
                return this.TotalRealFitness / (double)this.Size;
            }
            set
            {
            }
        }

        [XmlAttribute]
        public double MeanDiversity
        {
            get
            {
                if (this.Size <= 0)
                    return 0.0;
                return this.TotalDiversity / (double)this.Size;
            }
            set
            {
            }
        }

        [XmlAttribute]
        public double MeanNovelty
        {
            get
            {
                if (this.Size <= 0)
                    return 0.0;
                return this.TotalNovelty / (double)this.Size;
            }
            set
            {
            }
        }

        [XmlAttribute]
        public double MeanComplexity
        {
            get
            {
                if (this.Size <= 0)
                    return 0.0;
                return this.TotalComplexity / (double)this.Size;
            }
            set
            {
            }
        }
        [XmlAttribute]
        public double MaxFitness
        {
            get
            {
                double val1 = 0.0;
                foreach (NetworkGenome NetworkGenomes in this.Genomes)
                    val1 = Math.Max(val1, NetworkGenomes.Fitness);
                return val1;
            }
            set
            {
            }
        }

        [XmlAttribute]
        public double MaxRealFitness
        {
            get
            {
                double val1 = 0.0;
                foreach (NetworkGenome NetworkGenomes in this.Genomes)
                    val1 = Math.Max(val1, NetworkGenomes.RealFitness);
                return val1;
            }
            set
            {
            }
        }
        [XmlAttribute]
        public double MaxComplexity
        {
            get
            {
                double val1 = 0.0;
                foreach (NetworkGenome NetworkGenomes in this.Genomes)
                    val1 = Math.Max(val1, NetworkGenomes.Complexity);
                return val1;
            }
            set
            {
            }
        }
        internal double TotalFitness
        {
            get
            {
                double num = 0.0;
                foreach (NetworkGenome NetworkGenomes in this.Genomes)
                    num += NetworkGenomes.Fitness;
                return num;
            }
        }

        internal double TotalRealFitness
        {
            get
            {
                double num = 0.0;
                foreach (NetworkGenome NetworkGenomes in this.Genomes)
                    num += NetworkGenomes.RealFitness;
                return num;
            }
        }
        internal double TotalNovelty
        {
            get
            {
                double num = 0.0;
                foreach (NetworkGenome NetworkGenomes in this.Genomes)
                    num += NetworkGenomes.Novelty;
                return num;
            }
        }
        internal double TotalComplexity
        {
            get
            {
                double num = 0.0;
                foreach (NetworkGenome NetworkGenomes in this.Genomes)
                    num += NetworkGenomes.Complexity;
                return num;
            }
        }
        internal double TotalDiversity
        {
            get
            {
                double num = 0.0;
                foreach (NetworkGenome NetworkGenomes in this.Genomes)
                    num += NetworkGenomes.Genotypic_Diversity;
                return num;
            }
        }
        internal IEnumerable<int> SpeciesIds { get; private set; }

        static Population()
        {
            Population.Serial = new XmlSerializer(typeof(Population));
        }

        public Population(List<NetworkGenome> genomeList, int generation)
        {
            this.Genomes = new List<NetworkGenome>();
            if (genomeList != null)
            {
                foreach (NetworkGenome NetworkGenomes in genomeList)
                    this.Genomes.Add(NetworkGenomes.Clone() as NetworkGenome);
            }
            this.SpeciesIds = Enumerable.Distinct<int>(Enumerable.Select<NetworkGenome, int>((IEnumerable<NetworkGenome>)this.Genomes, (Func<NetworkGenome, int>)(genomes => genomes.Species)));
            this.Generation = generation;
        }

        public Population(List<NetworkGenome> genomeList)
            : this(genomeList, 0)
        {
        }

        public Population(int generation)
            : this((List<NetworkGenome>)null, generation)
        {
        }

        public Population()
            : this((List<NetworkGenome>)null, 0)
        {
        }

        public Population(Population copy)
            : this(copy.Genomes, copy.Generation)
        {
        }

        public static bool SaveToXml(string fileName, Population pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                Population.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static Population LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return Population.Serial.Deserialize((TextReader)streamReader) as Population;
        }

        public object Clone()
        {
            return (object)new Population(this);
        }
    }
}
