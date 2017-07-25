/*****************************************************************************************************
 * 
 * 
 * Computes Diversity : Computes the genotypic diversity 
 * 
 * 
 *****************************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keepaway
{
    [Serializable]
    public class Diversity
    {
        public double genotype_Diversity;
        public static KeepawayConfig config = new KeepawayConfig();
        public static List<NetworkGenome> archiveGD = new List<NetworkGenome>();
       
        
        public Diversity()
        {
           
        }
        /// <summary>
        /// compare1 KeyValuePairs to enable sorting of genomeList
        /// based on distance
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static int CompareFirst(Pair<double, NetworkGenome> x, Pair<double, NetworkGenome> y)
        {
            return x.First.CompareTo(y.First);
        }

        /// <summary>
        /// compare2 KeyValuePairs to enable sorting of genomeList
        /// based on genome Fitness
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static int CompareSecond(Pair<double, NetworkGenome> x, Pair<double, NetworkGenome> y)
        {
            return x.Second.Fitness.CompareTo(y.Second.Fitness);
        }
        /// <summary>
        /// returns the genome diversity 
        /// considers the k-nearest neighbors and the archive
        /// </summary>
        /// <param name="GenomesList"></param>
        /// <param name="genomes"></param>
        /// <returns></returns>
        public double DiversityDistance(List<NetworkGenome> GenomesList , NetworkGenome genomes)
        {
            config = KeepawayConfig.Load("KeepawayConfig.xml"); 
            List<NetworkGenome> Genomes = GenomesList;
            double diversity_dist = 0;
            List<Pair<double, NetworkGenome>> diversityList = new List<Pair<double, NetworkGenome>>();
            foreach (NetworkGenome networkGenomes in Genomes)
            {
                if (networkGenomes != genomes)
                    diversityList.Add(new Pair<double, NetworkGenome>(this.GenomeCompatibility(genomes, networkGenomes), networkGenomes));
            }

            //diversityList.Sort(CompareFirstFirst);
            //sort order
            diversityList = diversityList.OrderBy(o => o.First).ThenBy(o => o.Second.Fitness).ToList();
            int neighbors = config.nearestNeighbors;
            //if list less than k nearestneighbors
            if (diversityList.Count < config.nearestNeighbors)
                neighbors = diversityList.Count;
            //compute a sum of the first k nearestneighbors distances of genome x
            for (int i = 0; i < neighbors; i++)
            {
                diversity_dist += diversityList[i].First;
                genomes.nearestNeighbors++;
            }
            //test against the archive
            foreach (NetworkGenome inArchive in archiveGD)
                diversity_dist += this.GenomeCompatibility(genomes, inArchive);
            //compute average diversity
            genotype_Diversity = diversity_dist / ((neighbors + archiveGD.Count)); 

            return genotype_Diversity;           
        }

        /// <summary>
        /// compute the distance measure
        /// </summary>
        /// <param name="genome1"></param>
        /// <param name="genome2"></param>
        /// <returns></returns>
        internal double GenomeCompatibility(NetworkGenome genome1, NetworkGenome genome2)
        {
            double num1 = 0.0;
            List<LinkGene> list1 = new List<LinkGene>();
            List<NodeGene> list2 = new List<NodeGene>();
            List<LinkGene> list3 = new List<LinkGene>();
            List<NodeGene> list4 = new List<NodeGene>();
            genome1.Nodes.Sort();
            genome1.Links.Sort();
                int num3 = 0;
                genome2.Nodes.Sort();
                genome2.Links.Sort();
                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();
                int index1 = 0;
                int index2 = 0;
                 //finding the last nodes ids
                
                while (index1 < genome1.Nodes.Count && index2 < genome2.Nodes.Count)
                {
                    if (genome1.Nodes[index1].Id == genome2.Nodes[index2].Id) // common nodes between genomes
                    {
                        list2.Add(genome1.Nodes[index1]); //add nodes of genome 1 to list2
                        list4.Add(genome2.Nodes[index2]); // add nodes of genome 2 to list4
                        ++index1; //increase index of nodes of genome 1
                        ++index2; //increase index of nodes of genome 2
                    }
                    else if (genome1.Nodes[index1].Id > genome2.Nodes[index2].Id) //genome 1 id greater than genome 2 id , checking if its a disjoint or excess
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
                num1 += Species.Compatible.Disjoint * (double)(num3 + genome1.Nodes.Count + genome2.Nodes.Count - (index1 + index2));
                int index3 = 0;
                int index4 = 0;
                int num4 = 0;
                while (index3 < genome1.Links.Count && index4 < genome2.Links.Count)
                {
                    if (genome1.Links[index3].Id == genome2.Links[index4].Id)
                    {
                        list1.Add(genome1.Links[index3]);
                        list3.Add(genome2.Links[index4]);
                        ++index3;
                        ++index4;
                    }
                    else if (genome1.Links[index3].Id > genome2.Links[index4].Id)
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
                num1 += Species.Compatible.Disjoint * (double)(num4 + genome1.Links.Count + genome2.Links.Count - (index3 + index4));
                for (int index5 = 0; index5 < list1.Count; ++index5)
                    num1 += Math.Abs(list1[index5].Weight - list3[index5].Weight) * Species.Compatible.Weight;
                for (int index5 = 0; index5 < list2.Count; ++index5)
                    num1 += list2[index5].Function == list4[index5].Function ? 0.0 : Species.Compatible.Function;
         
                return num1;
        }
        /// <summary>
        /// to add genomes to the archive after every generation
        /// </summary>
        /// <param name="GenomesList"></param>
        public static void addToArchive(List<NetworkGenome> GenomesList)
        {
            List<NetworkGenome> sortedList = GenomesList.OrderByDescending(g => g.Genotypic_Diversity).Take(config.archiveLimit).ToList();

            foreach (NetworkGenome archiveList in sortedList)
            {
                if (archiveGD.Count < config.archiveSize - 1)
                {
                    if (archiveList.Genotypic_Diversity > config.archiveThreshold)
                        archiveGD.Add(archiveList);
                }
                else
                {
                    archiveGD = trimmedArchive(archiveGD);
                    if (archiveList.Genotypic_Diversity > config.archiveThreshold)
                        archiveGD.Add(archiveList);
                }
            }

        }
        /// <summary>
        /// manage the archive in the event of exceeding the archive Size
        /// </summary>
        /// <param name="ogArchive"></param>
        /// <returns></returns>
        public static List<NetworkGenome> trimmedArchive(List<NetworkGenome> ogArchive)
        {
            int lenTrim = config.archiveSize - 5;
            List<NetworkGenome> trimArchive = ogArchive.OrderByDescending(g => g.Genotypic_Diversity).Take(lenTrim).ToList();
            return trimArchive;
        }

    }
}
