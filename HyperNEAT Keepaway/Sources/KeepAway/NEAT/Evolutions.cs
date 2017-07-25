
//Adapted and modified from the following author //

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
using System.Linq;
using Keepaway.Parameters;

namespace Keepaway
{
    public delegate void GenerationDone(Evolutions evo);

    public delegate bool EvaluateGenome(List<NetworkGenome> genomes, bool test);
    public class Evolutions
    {
        private double delta = -10.0;
        private bool TooFew;
        private bool TooMany;

        public Initialization Init { get; set; }

        public Mutation Mutate { get; set; }

        public Reproduction Reprod { get; set; }

        public Saving Saves { get; set; }

        public Speciation Speciate { get; set; }

        public Testing Tests { get; set; }

        public double MaxFitness
        {
            get
            {
                return this.Previous.MaxFitness;
            }
            set
            {
            }
        }

        public double AverageFitness
        {
            get
            {
                return this.Previous.MeanFitness;
            }
            set
            {
            }
        }

        public Population Current { get; set; }

        internal Population Previous { get; set; }

        internal Population Next { get; set; }

        public NetworkGenome Champion
        {
            get
            {
                return this.Champions[this.Champions.Count - 1];
            }
            set
            {
            }
        }

        public int Generation { get; set; }

        internal List<NetworkGenome> Champions { get; set; }

        internal List<Species> species { get; set; }

        public EvaluateGenome Evaluator { get; set; }

        public bool CriteriaMet { get; set; }

        internal bool TestRun { get; set; }

        internal StreamWriter ChampionStatsFile { get; set; }

        internal StreamWriter PopulationStatsFile { get; set; }

        internal StreamWriter SpeciesStatsFile { get; set; }

        public event GenerationDone OnGeneration;

        public Evolutions()
        {
            this.Init = new Initialization();
            this.Mutate = new Mutation();
            this.Saves = new Saving();
            this.Speciate = new Speciation();
            this.Tests = new Testing();
            this.Reprod = new Reproduction();
            Species.Compatible = this.Speciate;
            this.Current = new Population(1);
            this.Previous = new Population(0);
            this.Next = new Population(2);
            this.Champions = new List<NetworkGenome>();
            this.species = new List<Species>();
            this.Generation = 1;
        }

        public void Revert()
        {
            this.Current = this.Previous;
        }

        private NetworkGenome Recombine(NetworkGenome parent1, NetworkGenome parent2)
        {
            NetworkGenome NetworkGenomes1 = parent1;
            if (parent1.Fitness < parent2.Fitness)
            {
                parent1 = parent2;
                parent2 = NetworkGenomes1;
            }
            NetworkGenome NetworkGenomes2 = new NetworkGenome();
            NetworkGenomes2.Id = NetworkGenome.NextId;
            NetworkGenomes2.Parent1 = parent1.Id;
            NetworkGenomes2.Parent2 = parent2.Id;
            List<NodeGene> list1 = new List<NodeGene>(Enumerable.Where<NodeGene>((IEnumerable<NodeGene>)parent1.Nodes, (Func<NodeGene, bool>)(genes => parent2.Nodes.Contains(genes))));
            list1.Sort();
            List<NodeGene> list2 = new List<NodeGene>(Enumerable.Where<NodeGene>((IEnumerable<NodeGene>)parent2.Nodes, (Func<NodeGene, bool>)(genes => parent1.Nodes.Contains(genes))));
            list2.Sort();
            List<LinkGene> list3 = new List<LinkGene>(Enumerable.Where<LinkGene>((IEnumerable<LinkGene>)parent1.Links, (Func<LinkGene, bool>)(genes => parent2.Links.Contains(genes))));
            list3.Sort();
            List<LinkGene> list4 = new List<LinkGene>(Enumerable.Where<LinkGene>((IEnumerable<LinkGene>)parent2.Links, (Func<LinkGene, bool>)(genes => parent1.Links.Contains(genes))));
            list4.Sort();
            bool flag = Utilities.Rnd.NextDouble() < this.Reprod.RecombineExcessFromLessFit;
            NetworkGenome NetworkGenomes3 = parent1;
            NetworkGenome notChoice = parent2;
            if (flag)
            {
                NetworkGenomes3 = parent2;
                notChoice = parent1;
            }
            List<NodeGene> list5 = new List<NodeGene>(Enumerable.Where<NodeGene>((IEnumerable<NodeGene>)NetworkGenomes3.Nodes, (Func<NodeGene, bool>)(genes => !notChoice.Nodes.Contains(genes))));
            list5.Sort();
            List<LinkGene> list6 = new List<LinkGene>(Enumerable.Where<LinkGene>((IEnumerable<LinkGene>)NetworkGenomes3.Links, (Func<LinkGene, bool>)(genes => !notChoice.Links.Contains(genes))));
            list6.Sort();
            for (int index = 0; index < list1.Count; ++index)
            {
                if (Utilities.Rnd.NextDouble() < this.Reprod.SelectFitterGene)
                    NetworkGenomes2.Nodes.Add(list1[index]);
                else
                    NetworkGenomes2.Nodes.Add(list2[index]);
            }
            for (int index = 0; index < list3.Count; ++index)
            {
                if (Utilities.Rnd.NextDouble() < this.Reprod.SelectFitterGene)
                    NetworkGenomes2.Links.Add(list3[index]);
                else
                    NetworkGenomes2.Links.Add(list4[index]);
            }
            NetworkGenomes2.Nodes.AddRange((IEnumerable<NodeGene>)list5);
            NetworkGenomes2.Links.AddRange((IEnumerable<LinkGene>)list6);
            return NetworkGenomes2;
        }

        private void MutateGenome(NetworkGenome temp)
        {
            double num1 = Utilities.Rnd.NextDouble() * (this.Mutate.AddConnection + this.Mutate.AddNode + this.Mutate.ChangeNodeFunction + this.Mutate.DeleteConnection + this.Mutate.MutateWeights) - this.Mutate.DeleteConnection;
            if (num1 <= 0.0 && this.DelectConnection(temp))
                return;
            double num2 = num1 - this.Mutate.ChangeNodeFunction;
            if (num2 <= 0.0 && this.ChangeNode(temp))
                return;
            double num3 = num2 - this.Mutate.AddNode;
            if (num3 <= 0.0 && this.AddNode(temp) || num3 - this.Mutate.AddConnection <= 0.0 && this.AddConnection(temp))
                return;
            this.MutateWeights(temp);
        }

        private void MutateWeights(NetworkGenome temp)
        {
            double num1 = 0.0;
            for (int index = 0; index < this.Mutate.WeightProbabilities.Count; ++index)
                num1 += this.Mutate.WeightProbabilities[index].Probability;
            double num2 = Utilities.Rnd.NextDouble() * num1;
            Mutation.WeightMutation weightMutation = new Mutation.WeightMutation();
            for (int index = 0; index < this.Mutate.WeightProbabilities.Count; ++index)
            {
                num2 -= this.Mutate.WeightProbabilities[index].Probability;
                if (num2 <= 0.0)
                {
                    weightMutation = this.Mutate.WeightProbabilities[index];
                    break;
                }
            }
            if (weightMutation.Proportional)
            {
                List<LinkGene> list = new List<LinkGene>(temp.MutatableLinks);
                for (int index = 0; index < list.Count; ++index)
                {
                    if (Utilities.Rnd.NextDouble() < weightMutation.Amount)
                    {
                        if (weightMutation.Delta)
                            list[index].Weight += weightMutation.Normal ? Utilities.Rnd.NextGuassian(0.0, 0.3) * this.Mutate.WeightRange / 10.0 : Utilities.Rnd.NextDouble(-this.Mutate.WeightRange / 10.0, this.Mutate.WeightRange / 10.0);
                        else
                            list[index].Weight = weightMutation.Normal ? Utilities.Rnd.NextGuassian(0.0, 0.3) * this.Mutate.WeightRange / 2.0 : Utilities.Rnd.NextDouble(-this.Mutate.WeightRange / 2.0, this.Mutate.WeightRange / 2.0);
                        list[index].Weight = Math.Max(-this.Mutate.WeightRange / 2.0, Math.Min(this.Mutate.WeightRange / 2.0, list[index].Weight));
                    }
                }
            }
            else
            {
                for (int index1 = 0; (double)index1 < weightMutation.Amount && Enumerable.Count<LinkGene>(temp.MutatableLinks) > 0; ++index1)
                {
                    int index2 = Utilities.Rnd.Next(Enumerable.Count<LinkGene>(temp.MutatableLinks));
                    LinkGene LinkGenes = Enumerable.ElementAt<LinkGene>(temp.MutatableLinks, index2);
                    LinkGenes.Mutated = true;
                    if (weightMutation.Delta)
                        LinkGenes.Weight += weightMutation.Normal ? Utilities.Rnd.NextGuassian(0.0, 1.0) * this.Mutate.WeightRange / 4.0 : Utilities.Rnd.NextDouble(-this.Mutate.WeightRange / 8.0, this.Mutate.WeightRange / 8.0);
                    else
                        LinkGenes.Weight = weightMutation.Normal ? Utilities.Rnd.NextGuassian(0.0, 0.3) * this.Mutate.WeightRange / 2.0 : Utilities.Rnd.NextDouble(-this.Mutate.WeightRange / 2.0, this.Mutate.WeightRange / 2.0);
                    LinkGenes.Weight = Math.Max(-this.Mutate.WeightRange / 2.0, Math.Min(this.Mutate.WeightRange / 2.0, LinkGenes.Weight));
                }
                for (int index = 0; index < temp.Links.Count; ++index)
                    temp.Links[index].Mutated = false;
            }
        }

        private bool DelectConnection(NetworkGenome temp)
        {
            List<LinkGene> list = new List<LinkGene>(temp.MutatableLinks);
            if (list.Count < 1)
                return false;
            LinkGene LinkGenes = list[(int)(Utilities.Rnd.NextDouble() * (double)list.Count)];
            temp.Links.Remove(LinkGenes);
            return true;
        }

        private bool ChangeNode(NetworkGenome temp)
        {
            List<NodeGene> list = new List<NodeGene>(temp.MutatableNodes);
            if (list.Count < 1)
                return false;
            NodeGene NodeGenes = list[(int)(Utilities.Rnd.NextDouble() * (double)list.Count)];
            string str = NodeGenes.Function;
            for (int index = 0; NodeGenes.Function == str && index < 7; ++index)
                NodeGenes.Function = this.SelectFunction();
            return !(NodeGenes.Function == str);
        }

        private bool AddNode(NetworkGenome temp)
        {
            List<LinkGene> list = new List<LinkGene>(temp.MutatableLinks);
            if (list.Count < 1)
                return false;
            LinkGene split = list[(int)(Utilities.Rnd.NextDouble() * (double)list.Count)];
            NodeGene NodeGenes1 = temp.Nodes.Find((Predicate<NodeGene>)(g => g.Id == split.Source));
            NodeGene NodeGenes2 = temp.Nodes.Find((Predicate<NodeGene>)(g => g.Id == split.Target));
            if (Math.Abs(NodeGenes1.Layer - NodeGenes2.Layer) <= 1)
                return false;
            NodeGene NodeGenes3 = new NodeGene("Identity", 0, Utilities.Rnd.Next(Math.Min(NodeGenes1.Layer, NodeGenes2.Layer) + 1, Math.Max(NodeGenes1.Layer, NodeGenes2.Layer)), NodeType.Hidden);
            NodeGenes3.Id = HistoricalMarkings.GetNodeMarking(split.Id, NodeGenes3.Layer, this.Generation);
            NodeGenes3.Function = this.SelectFunction();
            LinkGene LinkGenes1 = new LinkGene(HistoricalMarkings.GetLinkMarking(NodeGenes1.Id, NodeGenes3.Id, this.Generation), NodeGenes1.Id, NodeGenes3.Id, split.Weight);
            LinkGene LinkGenes2 = new LinkGene(HistoricalMarkings.GetLinkMarking(NodeGenes3.Id, NodeGenes2.Id, this.Generation), NodeGenes3.Id, NodeGenes2.Id, Math.Max(-this.Mutate.WeightRange / 2.0, Math.Min(this.Mutate.WeightRange / 2.0, Utilities.Rnd.NextGuassian(0.0, 0.2) * this.Mutate.WeightRange / 2.0)));
            temp.Nodes.Add(NodeGenes3);
            temp.Links.Add(LinkGenes1);
            temp.Links.Add(LinkGenes2);
            if (Utilities.Rnd.NextDouble() > this.Mutate.KeepLinkOnAddNode)
                temp.Links.Remove(split);
            return true;
        }

        private string SelectFunction()
        {
            double num1 = 0.0;
            for (int index = 0; index < this.Mutate.FunctionProbabilities.Count; ++index)
                num1 += this.Mutate.FunctionProbabilities[index].Probability;
            double num2 = Utilities.Rnd.NextDouble() * num1;
            string str = "";
            for (int index = 0; index < this.Mutate.FunctionProbabilities.Count; ++index)
            {
                num2 -= this.Mutate.FunctionProbabilities[index].Probability;
                if (num2 <= 0.0)
                {
                    str = this.Mutate.FunctionProbabilities[index].Function;
                    break;
                }
            }
            return str;
        }

        private bool AddConnection(NetworkGenome temp)
        {
            List<NodeGene> list1 = new List<NodeGene>((IEnumerable<NodeGene>)temp.Nodes);
            NodeGene source = list1[(int)(Utilities.Rnd.NextDouble() * (double)list1.Count)];
            List<NodeGene> list2 = new List<NodeGene>(Enumerable.Where<NodeGene>((IEnumerable<NodeGene>)temp.Nodes, (Func<NodeGene, bool>)(nodes =>
            {
                if (nodes.Type == NodeType.Bias || nodes.Type == NodeType.Input)
                    return false;
                if (!this.Mutate.Recurrence)
                    return nodes.Layer > source.Layer;
                return true;
            })));
            if (list2.Count < 1)
                return false;
            NodeGene target = list2[(int)(Utilities.Rnd.NextDouble() * (double)list2.Count)];
            if (Enumerable.Any<LinkGene>((IEnumerable<LinkGene>)temp.Links, (Func<LinkGene, bool>)(l =>
            {
                if (l.Source == source.Id)
                    return l.Target == target.Id;
                return false;
            })))
                return false;
            temp.Links.Add(new LinkGene(0, source.Id, target.Id, Math.Max(-this.Mutate.WeightRange / 2.0, Math.Min(this.Mutate.WeightRange / 2.0, Utilities.Rnd.NextGuassian(0.0, 0.1) * this.Mutate.WeightRange / 2.0)))
            {
                Id = HistoricalMarkings.GetLinkMarking(source.Id, target.Id, this.Generation)
            });
            return true;
        }

        public void OneGeneration()
        {
            this.CriteriaMet = this.Evaluator(this.Current.Genomes, false);
            if (Enumerable.Count<int>(this.Current.SpeciesIds) != this.Speciate.SpeciesCount)
                this.ChangeThreshold();
            this.UpdateStats();
            this.Save();
            this.NextGeneration();
            this.UpdateSpeciesExtinction();
            if (this.OnGeneration == null)
                return;
            this.OnGeneration(this);
        }

        private void UpdateSpeciesExtinction()
        {
            for (int index = 0; index < this.species.Count; ++index)
            {
                if (!Enumerable.Contains<int>(this.Current.SpeciesIds, index))
                    this.species[index].Extinct = true;
            }
        }

        private void NextGeneration()
        {
            this.IncrementGeneration();
            this.Elitism();
            this.Reproduce();
            this.Previous = this.Current;
            this.Current = this.Next;
            this.Next = new Population(this.Generation + 1);
        }

        private void Reproduce()
        {
            List<int> species = new List<int>(this.Current.SpeciesIds);
            List<double> list1 = new List<double>();
            List<List<NetworkGenome>> list2 = new List<List<NetworkGenome>>();
            List<int> list3 = new List<int>(3);
            for (int i = 0; i < species.Count; ++i)
            {
                list2.Add(new List<NetworkGenome>(Enumerable.Where<NetworkGenome>((IEnumerable<NetworkGenome>)this.Current.Genomes, (Func<NetworkGenome, bool>)(genomes => genomes.Species == species[i]))));
                double num = 0.0;
                for (int index = 0; index < list2[i].Count; ++index)
                    num += list2[i][index].Fitness;
                list1.Add(num / (double)list2[i].Count * this.Stagnation(species[i]));
            }
            while (this.Next.Genomes.Count < this.Init.PopulationSize)
            {
                list3.Clear();
                int index1;
                if (species.Count < 2)
                    index1 = 0;
                else if (species.Count < 3)
                {
                    double num = Utilities.Rnd.NextDouble();
                    index1 = list1[0] <= list1[1] ? (num >= this.Reprod.SelectFitterSpecies ? 0 : 1) : (num >= this.Reprod.SelectFitterSpecies ? 1 : 0);
                }
                else
                {
                    list3.Add((int)(Utilities.Rnd.NextDouble() * (double)species.Count) % species.Count);
                    int num1 = (int)(Utilities.Rnd.NextDouble() * (double)species.Count) % species.Count;
                    if (list3[0] == num1)
                        num1 = (num1 + 1) % species.Count;
                    list3.Add(num1);
                    int num2 = (int)(Utilities.Rnd.NextDouble() * (double)species.Count) % species.Count;
                    while (list3.Contains(num2))
                        num2 = (num2 + 1) % species.Count;
                    list3.Add(num2);
                    if (Utilities.Rnd.NextDouble() < this.Reprod.SelectFitterSpecies)
                    {
                        int index2 = list3[0];
                        for (int index3 = 1; index3 < list3.Count; ++index3)
                        {
                            if (list1[index2] < list1[list3[index3]])
                                index2 = list3[index3];
                        }
                        index1 = index2;
                    }
                    else if (Utilities.Rnd.NextDouble() > this.Reprod.SelectFitterSpecies)
                    {
                        int index2 = list3[0];
                        for (int index3 = 1; index3 < list3.Count; ++index3)
                        {
                            if (list1[index2] > list1[list3[index3]])
                                index2 = list3[index3];
                        }
                        index1 = index2;
                    }
                    else
                        index1 = list1[list3[0]] <= list1[list3[1]] ? (list1[list3[0]] <= list1[list3[2]] ? (list1[list3[1]] <= list1[list3[2]] ? list3[1] : list3[2]) : list3[0]) : (list1[list3[0]] >= list1[list3[2]] ? (list1[list3[1]] <= list1[list3[2]] ? list3[2] : list3[1]) : list3[0]);
                }
                this.Next.Genomes.Add(this.CreateOneOffspringFromSpecies(list2[index1]));
            }
        }

        private double Stagnation(int speciesId)
        {
            if (this.Generation - this.species[speciesId].Improved > this.Reprod.SpeciesStagnation)
                return 1.0 - (double)((this.Generation - this.species[speciesId].Improved - this.Reprod.SpeciesStagnation) / this.Reprod.SpeciesStagnation);
            return 1.0;
        }

        private NetworkGenome CreateOneOffspringFromSpecies(List<NetworkGenome> list)
        {
            NetworkGenome parent1 = this.SelectGenome(list);
            NetworkGenome temp;
            if (Utilities.Rnd.NextDouble() < this.Reprod.AsexualProbability || list.Count < 2)
            {
                temp = parent1.Clone() as NetworkGenome;
                temp.Age = 0;
                temp.Fitness = 0.0;
                temp.Id = NetworkGenome.NextId;
                temp.Parent1 = parent1.Id;
                temp.Parent2 = -1;
                this.MutateGenome(temp);
            }
            else
            {
                NetworkGenome parent2 = this.SelectGenome(list, parent1);
                temp = this.Recombine(parent1, parent2);
            }
            this.DetermineSpecies(temp);
            return temp;
        }

        private void DetermineSpecies(NetworkGenome temp)
        {
            int index1 = -1;
            double num1 = 100000.0;
            for (int index2 = 0; index2 < this.species.Count; ++index2)
            {
                if (!this.species[index2].Extinct)
                {
                    double num2 = this.species[index2].GenomeCompatibility(temp);
                    if (num2 < num1)
                    {
                        num1 = num2;
                        index1 = index2;
                    }
                }
            }
            if (num1 <= this.Speciate.Threshold || (double)Enumerable.Count<Species>((IEnumerable<Species>)this.species, (Func<Species, bool>)(g => !g.Extinct)) > (double)this.Speciate.SpeciesCount * 1.5)
            {
                temp.Species = index1;
                this.species[index1].Genomes.Add(temp);
            }
            else
            {
                temp.Species = this.species.Count;
                this.species.Add(new Species(this.Generation, this.Generation, 0.0, temp));
            }
        }

        private NetworkGenome SelectGenome(List<NetworkGenome> list, NetworkGenome parent1)
        {
            List<NetworkGenome> list1 = new List<NetworkGenome>((IEnumerable<NetworkGenome>)list);
            list1.Remove(parent1);
            return this.SelectGenome(list1);
        }

        private NetworkGenome SelectGenome(List<NetworkGenome> list)
        {
            if (list.Count < 2)
                return list[0];
            if (list.Count < 3)
            {
                NetworkGenome NetworkGenomes1 = list[0].Fitness > list[1].Fitness ? list[0] : list[1];
                NetworkGenome NetworkGenomes2 = list[0].Fitness > list[1].Fitness ? list[1] : list[0];
                if (Utilities.Rnd.NextDouble() < this.Reprod.SelectFitterGenome)
                    return NetworkGenomes1;
                return NetworkGenomes2;
            }
            List<NetworkGenome> list1 = new List<NetworkGenome>(3);
            int index1 = (int)(Utilities.Rnd.NextDouble() * (double)list.Count);
            list1.Add(list[index1]);
            int index2 = (int)(Utilities.Rnd.NextDouble() * (double)list.Count);
            if (list1.Contains(list[index2]))
                index2 = (index2 + 1) % list.Count;
            list1.Add(list[index2]);
            int index3 = (int)(Utilities.Rnd.NextDouble() * (double)list.Count);
            while (list1.Contains(list[index3]))
                index3 = (index3 + 1) % list.Count;
            list1.Add(list[index3]);
            list1.Sort((Comparison<NetworkGenome>)((g, e) => e.Fitness.CompareTo(g.Fitness)));
            if (Utilities.Rnd.NextDouble() < this.Reprod.SelectFitterGenome)
                return list[0];
            if (Utilities.Rnd.NextDouble() > this.Reprod.SelectFitterGenome)
                return list[1];
            return list[2];
        }

        private void Elitism()
        {
            using (IEnumerator<int> enumerator = this.Current.SpeciesIds.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    int spec = enumerator.Current;
                    List<NetworkGenome> list = new List<NetworkGenome>(Enumerable.Where<NetworkGenome>((IEnumerable<NetworkGenome>)this.Current.Genomes, (Func<NetworkGenome, bool>)(g => g.Species == spec)));
                    list.Sort((Comparison<NetworkGenome>)((a, b) => b.Fitness.CompareTo(a.Fitness)));
                    int num = (int)Math.Round(this.Reprod.Elitism * (double)list.Count);
                    for (int index = 0; index < num; ++index)
                        this.Next.Genomes.Add(list[index]);
                }
            }
        }

        private void IncrementGeneration()
        {
            ++this.Generation;
            for (int index = 0; index < this.Current.Genomes.Count; ++index)
                ++this.Current.Genomes[index].Age;
            HistoricalMarkings.CleanUp(this.Generation, this.Speciate.MarkingAge);
        }

        private void Save()
        {
            this.Current.Genomes.Sort((Comparison<NetworkGenome>)((b, a) => a.Fitness.CompareTo(b.Fitness)));
            NetworkGenome genome = this.Current.Genomes[0];
            this.Champions.Add(genome);
            this.Current.Genomes.Sort((Comparison<NetworkGenome>)((a, b) => a.Species.CompareTo(b.Species)));
            if (this.Saves.Champions.save && this.Generation % this.Saves.Champions.inteval == 0)
                NetworkGenome.SaveToFile(genome, this.Saves.Champions.path + "G" + this.Generation.ToString("0000") + "Champ.xml");
            if (this.Saves.Populations.save && this.Generation % this.Saves.Populations.inteval == 0)
                Population.SaveToXml(this.Saves.Populations.path + "G" + this.Generation.ToString("0000") + "Pop.xml", this.Current);
            if (this.Saves.PopulationStatistics.save && (this.Generation % this.Saves.PopulationStatistics.inteval == 0 || this.Generation == 1))
                this.PopulationStatsFile.WriteLine((string)(object)this.Current.Generation + (object)"\t" + (string)(object)this.Current.MeanFitness + "\t" + (string)(object)this.Current.MaxFitness);
            if (this.Saves.Species.save && this.Generation % this.Saves.Species.inteval == 0)
            {
                for (int index = 0; index < this.species.Count; ++index)
                    Species.SaveToXml(this.Saves.Species.path + "G" + this.Generation.ToString("0000") + "Species" + index.ToString("0000") + ".xml", this.species[index]);
            }
            if (!this.Saves.SpeciesStatistics.save || this.Generation % this.Saves.SpeciesStatistics.inteval != 0)
                return;
            this.WriteSpeciesStats();
        }

        private void UpdateStats()
        {
            for (int index = 0; index < this.species.Count; ++index)
            {
                if (this.species[index].Champion.Fitness != this.species[index].MaximumFitness)
                {
                    if (this.species[index].MaximumFitness < this.species[index].Champion.Fitness)
                        this.species[index].Improved = this.Generation;
                    this.species[index].MaximumFitness = this.species[index].Champion.Fitness;
                }
            }
        }

        private void ChangeThreshold()
        {
            if (this.Speciate.SpeciesCount - Enumerable.Count<Species>((IEnumerable<Species>)this.species, (Func<Species, bool>)(g => !g.Extinct)) > 0)
            {
                if (this.TooMany)
                {
                    this.delta *= -0.75;
                }
                else
                {
                    if (this.delta > 0.0)
                        this.delta *= -1.0;
                    this.delta *= 1.15;
                }
                this.TooFew = true;
            }
            else if (this.Speciate.SpeciesCount - Enumerable.Count<Species>((IEnumerable<Species>)this.species, (Func<Species, bool>)(g => !g.Extinct)) < 0)
            {
                if (this.TooFew)
                {
                    this.delta *= -0.75;
                }
                else
                {
                    if (this.delta < 0.0)
                        this.delta *= -1.0;
                    this.delta *= 1.15;
                }
                this.TooMany = true;
            }
            else
            {
                this.TooFew = false;
                this.TooMany = false;
                this.delta = -1.05 * this.Speciate.Threshold;
            }
            this.Speciate.Threshold += this.delta;
        }

        public void AllGenerations()
        {
            if (this.TestRun)
            {
                this.PerformTest();
            }
            else
            {
                while (this.Generation <= this.Init.MaximumGenerations && (!this.CriteriaMet || !this.Init.StopOnFitnessCriteria))
                    this.OneGeneration();
                if (this.Tests.ChampionsAtEnd.test)
                    this.PerformTest();
                this.CompleteSaves();
            }
        }

        private void CompleteSaves()
        {
            if (this.ChampionStatsFile != null)
            {
                for (int index = 0; index < this.Champions.Count; ++index)
                    this.ChampionStatsFile.WriteLine(this.Champions[index].Fitness);
                this.ChampionStatsFile.Flush();
                this.ChampionStatsFile.Close();
            }
            if (this.PopulationStatsFile != null)
            {
                this.PopulationStatsFile.Flush();
                this.PopulationStatsFile.Close();
            }
            if (this.SpeciesStatsFile == null)
                return;
            this.SpeciesStatsFile.Flush();
            this.SpeciesStatsFile.Close();
        }

        private void PerformTest()
        {
            if (this.Tests.Directory.test || this.Tests.Genome.test)
            {
                int num = this.Evaluator(this.Current.Genomes, true) ? 1 : 0;
                if (this.Tests.Genome.test)
                {
                    NetworkGenome.SaveToFile(this.Current.Genomes[0], this.Tests.Genome.path);
                }
                else
                {
                    using (StreamWriter streamWriter = new StreamWriter(this.Tests.Directory.path + "Results.txt"))
                    {
                        for (int index = 0; index < this.Current.Genomes.Count; ++index)
                            streamWriter.WriteLine(this.Current.Genomes[index].Fitness);
                    }
                }
            }
            else
            {
                if (!this.Tests.ChampionsAtEnd.test)
                    return;
                int num = this.Evaluator(this.Champions, true) ? 1 : 0;
                using (StreamWriter streamWriter = new StreamWriter(this.Tests.ChampionsAtEnd.path + "ChampResults.txt"))
                {
                    for (int index = 0; index < this.Champions.Count; ++index)
                        streamWriter.WriteLine(this.Champions[index].Fitness);
                }
            }
        }

        public bool SetParameters(string fileName)
        {
            ParameterFiles parameterFiles = ParameterFiles.LoadFromXml(fileName);
            if (parameterFiles == null)
                return false;
            object obj1 = (object)null;
            object obj2 = (object)Initialization.LoadFromXml(parameterFiles.Initialization);
            if (obj2 == null)
                return false;
            this.Init = (Initialization)obj2;
            obj1 = (object)null;
            object obj3 = (object)Mutation.LoadFromXml(parameterFiles.Mutation);
            if (obj3 == null)
                return false;
            this.Mutate = (Mutation)obj3;
            obj1 = (object)null;
            object obj4 = (object)Reproduction.LoadFromXml(parameterFiles.Reproduction);
            if (obj4 == null)
                return false;
            this.Reprod = (Reproduction)obj4;
            obj1 = (object)null;
            object obj5 = (object)Saving.LoadFromXml(parameterFiles.Saving);
            if (obj5 == null)
                return false;
            this.Saves = (Saving)obj5;
            obj1 = (object)null;
            object obj6 = (object)Speciation.LoadFromXml(parameterFiles.Speciation);
            if (obj6 == null)
                return false;
            this.Speciate = (Speciation)obj6;
            Species.Compatible = this.Speciate;
            obj1 = (object)null;
            object obj7 = (object)Testing.LoadFromXml(parameterFiles.Testing);
            if (obj7 == null)
                return false;
            this.Tests = (Testing)obj7;
            obj1 = (object)null;
            object obj8 = (object)HyperNEAT.LoadFromXml(parameterFiles.HyperNEAT);
            if (obj8 == null)
                return false;
            Substrates.param = (HyperNEAT)obj8;
            return true;
        }

        public bool Initialise()
        {
            bool flag = true;
            return (this.Tests.Directory.test || this.Tests.Genome.test ? flag && this.TestInit() : (!this.Init.Population.load ? (!this.Init.Seed.load ? flag && this.FreshInit() : flag && this.LoadSeedInit()) : flag && this.LoadPopulationInit())) && this.SaveInit();
        }

        private bool SaveInit()
        {
            if (!this.TestRun)
            {
                if (this.Saves.ChampionStatistics.save)
                {
                    if (!Directory.Exists(this.Saves.ChampionStatistics.path))
                        Directory.CreateDirectory(this.Saves.ChampionStatistics.path);
                    this.ChampionStatsFile = new StreamWriter(this.Saves.ChampionStatistics.path + "ChampStats.txt");
                }
                if (this.Saves.Populations.save)
                {
                    if (!Directory.Exists(this.Saves.Populations.path))
                        Directory.CreateDirectory(this.Saves.Populations.path);
                    Population.SaveToXml(this.Saves.Populations.path + "G" + this.Generation.ToString("####") + "Pop.xml", this.Current);
                }
                if (this.Saves.PopulationStatistics.save)
                {
                    if (!Directory.Exists(this.Saves.PopulationStatistics.path))
                        Directory.CreateDirectory(this.Saves.PopulationStatistics.path);
                    this.PopulationStatsFile = new StreamWriter(this.Saves.ChampionStatistics.path + "PopStats.txt");
                }
                if (this.Saves.Species.save)
                {
                    if (!Directory.Exists(this.Saves.Species.path))
                        Directory.CreateDirectory(this.Saves.Species.path);
                    for (int index = 0; index < this.species.Count; ++index)
                        Species.SaveToXml(this.Saves.Species.path + "G" + this.Generation.ToString("####") + "Species" + index.ToString("####") + ".xml", this.species[index]);
                }
                if (this.Saves.SpeciesStatistics.save)
                {
                    if (!Directory.Exists(this.Saves.SpeciesStatistics.path))
                        Directory.CreateDirectory(this.Saves.SpeciesStatistics.path);
                    this.SpeciesStatsFile = new StreamWriter(this.Saves.SpeciesStatistics.path + "SpeciesStats.txt");
                    this.WriteSpeciesStats();
                }
            }
            return true;
        }

        private void WriteSpeciesStats()
        {
            string str = "ActiveSpecies#:\t" + (object)Enumerable.Count<Species>((IEnumerable<Species>)this.species, (Func<Species, bool>)(s => !s.Extinct));
            for (int index = 0; index < this.species.Count; ++index)
            {
                if (!this.species[index].Extinct)
                    str = str + (object)"\tSpecies" + (object)index + ":\tMembers:\t" + this.species[index].Genomes.Count + "\tMaxFitness:\t" + this.species[index].MaximumFitness + "\tAvgFitness:\t" + (object)this.species[index].AverageFitness;
            }
            this.SpeciesStatsFile.WriteLine(str);
        }

        private bool FreshInit()
        {
            NetworkGenome NetworkGenomes1 = new NetworkGenome(-1, 0, -1, -1, -1);
            NetworkGenomes1.Nodes.Add(new NodeGene("Identity", NodeGene.NextId, 0, NodeType.Bias));
            for (int index = 0; index < this.Init.Inputs; ++index)
                NetworkGenomes1.Nodes.Add(new NodeGene("Identity", NodeGene.NextId, 0, NodeType.Input));
            for (int index = 0; index < this.Init.Outputs; ++index)
                NetworkGenomes1.Nodes.Add(new NodeGene("BipolarSigmoid", NodeGene.NextId, this.Init.MaxLayers - 1, NodeType.Output));
            while (this.Current.Genomes.Count < this.Init.PopulationSize)
            {
                NetworkGenome NetworkGenomes2 = NetworkGenomes1.Clone() as NetworkGenome;
                NetworkGenomes2.Id = NetworkGenome.NextId;
                this.InitLinks(NetworkGenomes2);
                double num1 = 1000000.0;
                int index1 = 0;
                for (int index2 = 0; index2 < this.species.Count; ++index2)
                {
                    double num2 = this.species[index2].GenomeCompatibility(NetworkGenomes2);
                    if (num2 < num1)
                    {
                        num1 = num2;
                        index1 = index2;
                    }
                }
                if (num1 <= this.Speciate.Threshold)
                {
                    this.species[index1].Genomes.Add(NetworkGenomes2);
                    NetworkGenomes2.Species = index1;
                }
                else
                {
                    this.species.Add(new Species(0, 0, 0.0, NetworkGenomes2));
                    NetworkGenomes2.Species = this.species.Count - 1;
                }
                this.Current.Genomes.Add(NetworkGenomes2);
            }
            return true;
        }

        private void InitLinks(NetworkGenome temp)
        {
            foreach (NodeGene NodeGenes1 in Enumerable.Where<NodeGene>((IEnumerable<NodeGene>)temp.Nodes, (Func<NodeGene, bool>)(nodes =>
            {
                if (nodes.Type != NodeType.Bias)
                    return nodes.Type == NodeType.Input;
                return true;
            })))
            {
                foreach (NodeGene NodeGenes2 in Enumerable.Where<NodeGene>((IEnumerable<NodeGene>)temp.Nodes, (Func<NodeGene, bool>)(nodes => nodes.Type == NodeType.Output)))
                {
                    if (Utilities.Rnd.NextDouble() < this.Init.InitalConnectionPercentage)
                        temp.Links.Add(new LinkGene(HistoricalMarkings.GetLinkMarking(NodeGenes1.Id, NodeGenes2.Id, 0), NodeGenes1.Id, NodeGenes2.Id, Utilities.Rnd.NextDouble(-this.Mutate.WeightRange / 2.0, this.Mutate.WeightRange / 2.0)));
                }
            }
        }

        private bool LoadSeedInit()
        {
            NetworkGenome genome = NetworkGenome.LoadFromFile(this.Init.Seed.file);
            if (genome == null)
                return false;
            genome.Fitness = 0.0;
            genome.Age = 0;
            genome.Species = 0;
            genome.Id = 0;
            this.Current.Genomes.Add(genome);
            this.species.Add(new Species(0, 0, 0.0, genome));
            this.Init.MaxLayers = Enumerable.First<NodeGene>((IEnumerable<NodeGene>)genome.Nodes, (Func<NodeGene, bool>)(g => g.Type == NodeType.Output)).Layer + 1;
            this.Init.Inputs = Enumerable.Count<NodeGene>((IEnumerable<NodeGene>)genome.Nodes, (Func<NodeGene, bool>)(g => g.Type == NodeType.Input));
            this.Init.Outputs = Enumerable.Count<NodeGene>((IEnumerable<NodeGene>)genome.Nodes, (Func<NodeGene, bool>)(g => g.Type == NodeType.Output));
            NetworkGenome.NextId = 1;
            foreach (LinkGene LinkGenes in genome.Links)
                LinkGene.NextId = Math.Max(LinkGene.NextId, LinkGenes.Id);
            foreach (NodeGene NodeGenes in genome.Nodes)
                NodeGene.NextId = Math.Max(NodeGene.NextId, NodeGenes.Id);
            while (this.Current.Genomes.Count < this.Init.PopulationSize)
            {
                NetworkGenome networkGenomes = genome.Clone() as NetworkGenome;
                networkGenomes.Id = NetworkGenome.NextId;
                networkGenomes.Parent1 = 0;
                networkGenomes.Parent2 = -1;
                this.RandomizeWeights(networkGenomes);
                double num1 = 1000000.0;
                int index1 = 0;
                for (int index2 = 0; index2 < this.species.Count; ++index2)
                {
                    double num2 = this.species[index2].GenomeCompatibility(networkGenomes);
                    if (num2 < num1)
                    {
                        num1 = num2;
                        index1 = index2;
                    }
                }
                if (num1 <= this.Speciate.Threshold)
                {
                    this.species[index1].Genomes.Add(networkGenomes);
                    networkGenomes.Species = index1;
                }
                else
                {
                    this.species.Add(new Species(0, 0, 0.0, networkGenomes));
                    networkGenomes.Species = this.species.Count - 1;
                }
                this.Current.Genomes.Add(networkGenomes);
            }
            return true;
        }

        private void RandomizeWeights(NetworkGenome temp)
        {
            foreach (LinkGene LinkGenes in temp.Links)
                LinkGenes.Weight = Utilities.Rnd.NextDouble(-this.Mutate.WeightRange / 2.0, this.Mutate.WeightRange / 2.0);
        }

        private bool LoadPopulationInit()
        {
            this.Current = Population.LoadFromXml(this.Init.Population.file);
            if (this.Current == null)
                return false;
            this.Generation = this.Current.Generation;
            this.Init.PopulationSize = this.Current.Genomes.Count;
            this.Init.Inputs = Enumerable.Count<NodeGene>((IEnumerable<NodeGene>)this.Current.Genomes[0].Nodes, (Func<NodeGene, bool>)(gene => gene.Type == NodeType.Input));
            this.Init.Outputs = Enumerable.Count<NodeGene>((IEnumerable<NodeGene>)this.Current.Genomes[0].Nodes, (Func<NodeGene, bool>)(gene => gene.Type == NodeType.Output));
            this.Init.MaxLayers = Enumerable.First<NodeGene>((IEnumerable<NodeGene>)this.Current.Genomes[0].Nodes, (Func<NodeGene, bool>)(gene => gene.Type == NodeType.Output)).Layer + 1;
            List<int> species = new List<int>();
            foreach (int num in Enumerable.Distinct<int>(Enumerable.Select<NetworkGenome, int>((IEnumerable<NetworkGenome>)this.Current.Genomes, (Func<NetworkGenome, int>)(genomes => genomes.Species))))
                species.Add(num);
            species.Sort();
            for (int i = 0; i < species.Count; ++i)
            {
                Species species1 = new Species();
                species1.Improved = 0;
                species1.Created = 0;
                species1.Extinct = false;
                foreach (NetworkGenome networkGenomes in Enumerable.Where<NetworkGenome>((IEnumerable<NetworkGenome>)this.Current.Genomes, (Func<NetworkGenome, bool>)(genomes => genomes.Species == species[i])))
                {
                    networkGenomes.Species = i;
                    species1.Genomes.Add(networkGenomes);
                }
                this.species.Add(species1);
            }
            foreach (NetworkGenome networkGenomes in this.Current.Genomes)
            {
                networkGenomes.Id = NetworkGenome.NextId;
                foreach (LinkGene LinkGenes in networkGenomes.Links)
                    LinkGene.NextId = Math.Max(LinkGene.NextId, LinkGenes.Id);
                foreach (NodeGene NodeGenes in networkGenomes.Nodes)
                    NodeGene.NextId = Math.Max(NodeGene.NextId, NodeGenes.Id);
            }
            return true;
        }

        private bool TestInit()
        {
            this.TestRun = true;
            if (this.Tests.Directory.test)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(this.Tests.Directory.path);
                foreach (FileSystemInfo fileSystemInfo in directoryInfo.GetFiles("*.xml"))
                {
                    NetworkGenome networkGenomes = NetworkGenome.LoadFromFile(fileSystemInfo.FullName);
                    if (networkGenomes != null)
                        this.Current.Genomes.Add(networkGenomes);
                }
            }
            else
                this.Current.Genomes.Add(NetworkGenome.LoadFromFile(this.Tests.Genome.path));
            return true;
        }
    }
}
