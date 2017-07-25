/***************************************************************************
                                   FixedKeepawayPlayer.cs
                    Fixed policy kepaway player for Keepaway domain experiments.
                             -------------------
    begin                : JUL-2009
    credit               : Phillip Verbancsics of the
                            Evolutionary Complexity Research Group
     email                : verb@cs.ucf.edu

  
 Based On: 
/*

Copyright (c) 2004 Gregory Kuhlmann, Peter Stone
University of Texas at Austin
All rights reserved.
 
 Base On:

Copyright (c) 2000-2003, Jelle Kok, University of Amsterdam
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

3. Neither the name of the University of Amsterdam nor the names of its
contributors may be used to endorse or promote products derived from this
software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Keepaway.Parameters;
namespace Keepaway
{
    [XmlInclude(typeof(NetworkGenome))]
    [Serializable]
    public class Species
    {
        [XmlArray]
        public List<NetworkGenome> Genomes;

        public static Speciation Compatible { get; set; }

        private static XmlSerializer Serial { get; set; }

        public NetworkGenome Champion
        {
            get
            {
                if (this.Genomes.Count < 1)
                    return (NetworkGenome)null;
                this.Genomes.Sort((Comparison<NetworkGenome>)((a, b) => b.Fitness.CompareTo(a.Fitness)));
                return this.Genomes[0];
            }
            set
            {
            }
        }

        [XmlAttribute]
        public int Created { get; set; }

        [XmlAttribute]
        public int Improved { get; set; }

        [XmlAttribute]
        public double MaximumFitness { get; set; }

        [XmlAttribute]
        public double AverageFitness
        {
            get
            {
                if (this.Genomes.Count < 1)
                    return 0.0;
                double num = 0.0;
                foreach (NetworkGenome networkGenomes in this.Genomes)
                    num += networkGenomes.Fitness;
                return num / (double)this.Genomes.Count;
            }
            set
            {
            }
        }

        [XmlAttribute]
        public bool Extinct { get; set; }

        static Species()
        {
            Species.Serial = new XmlSerializer(typeof(Species));
        }

        public Species(List<NetworkGenome> genomeList, int created, int improved, double maxFitness, bool extinct)
        {
            this.Genomes = new List<NetworkGenome>();
            if (genomeList != null)
            {
                foreach (NetworkGenome networkGenomes in genomeList)
                    this.Genomes.Add(networkGenomes.Clone() as NetworkGenome);
            }
            this.Created = created;
            this.Improved = improved;
            this.MaximumFitness = maxFitness;
            this.Extinct = extinct;
        }

        public Species(int created, int improved, double maxFitness, NetworkGenome genome)
            : this((List<NetworkGenome>)null, created, improved, maxFitness, false)
        {
            this.Genomes.Add(genome);
        }

        public Species()
            : this((List<NetworkGenome>)null, 0, 0, 0.0, false)
        {
        }

        public static bool SaveToXml(string fileName, Species pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                Species.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static Species LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return Species.Serial.Deserialize((TextReader)streamReader) as Species;
        }

        internal double GenomeCompatibility(NetworkGenome genome)
        {
            double num1 = 0.0;
            List<LinkGene> list1 = new List<LinkGene>();
            List<NodeGene> list2 = new List<NodeGene>();
            List<LinkGene> list3 = new List<LinkGene>();
            List<NodeGene> list4 = new List<NodeGene>();
            genome.Nodes.Sort();
            genome.Links.Sort();
            int num2 = 0;
            foreach (NetworkGenome networkGenomes in this.Genomes)
            {
                int num3 = 0;
                networkGenomes.Nodes.Sort();
                networkGenomes.Links.Sort();
                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();
                int index1 = 0;
                int index2 = 0;
                while (index1 < genome.Nodes.Count && index2 < networkGenomes.Nodes.Count)
                {
                    if (genome.Nodes[index1].Id == networkGenomes.Nodes[index2].Id)
                    {
                        list2.Add(genome.Nodes[index1]);
                        list4.Add(networkGenomes.Nodes[index2]);
                        ++index1;
                        ++index2;
                    }
                    else if (genome.Nodes[index1].Id > networkGenomes.Nodes[index2].Id)
                    {
                        ++num3;
                        ++index2;
                    }
                    else
                    {
                        ++num3;
                        ++index1;
                    }
                }
                num1 += Species.Compatible.Disjoint * (double)(num3 + genome.Nodes.Count + networkGenomes.Nodes.Count - (index1 + index2));
                int index3 = 0;
                int index4 = 0;
                int num4 = 0;
                while (index3 < genome.Links.Count && index4 < networkGenomes.Links.Count)
                {
                    if (genome.Links[index3].Id == networkGenomes.Links[index4].Id)
                    {
                        list1.Add(genome.Links[index3]);
                        list3.Add(networkGenomes.Links[index4]);
                        ++index3;
                        ++index4;
                    }
                    else if (genome.Links[index3].Id > networkGenomes.Links[index4].Id)
                    {
                        ++num4;
                        ++index4;
                    }
                    else
                    {
                        ++num4;
                        ++index3;
                    }
                }
                num1 += Species.Compatible.Disjoint * (double)(num4 + genome.Links.Count + networkGenomes.Links.Count - (index3 + index4));
                for (int index5 = 0; index5 < list1.Count; ++index5)
                    num1 += Math.Abs(list1[index5].Weight - list3[index5].Weight) * Species.Compatible.Weight;
                for (int index5 = 0; index5 < list2.Count; ++index5)
                    num1 += list2[index5].Function == list4[index5].Function ? 0.0 : Species.Compatible.Function;
                ++num2;
                if (num2 == Species.Compatible.RepresentitiveSize)
                    break;
            }
            return num1 / (double)num2;
        }
    }
}
