
/*****************************************************************************************************
 * 
 * 
 * Computes Novelty : Computes the team behavioral diversity
 * 
 * 
 *****************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keepaway
{
    public class ComputeNovelty
    {
        public static List<NetworkGenome> archive = new List<NetworkGenome>();
        public static KeepawayConfig config = new KeepawayConfig();
        //public static List<NetworkGenome> ToAddtoArchive = new List<NetworkGenome>();
       // public static List<NetworkGenome> popTomeasureWith = new List<NetworkGenome>();
       // public static double archive_threshold = 0.03;
       // public int nearestNeighbors = 15;
       // public static int minToArchive = 5;
       // public double archiveThreshold;
       // public const int archiveLimit = 2000;
        int scalarFactor = 60;
        /// <summary>
        /// compare2 KeyValuePairs to enable sorting of genomeList
        /// based on novelty distance
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
        /// Computes the genome average novelty
        /// based on k nearest neighbors and against the archive
        /// </summary>
        /// <param name="neatGenome"></param>
        /// <param name="genomeList"></param>
        /// <returns></returns>
        public double computeNovelty(NetworkGenome neatGenome, List<NetworkGenome> genomeList)
        {
            config = KeepawayConfig.Load("KeepawayConfig.xml");
            double novelty = 0.0;
            //double dist = 0.0;
            List<NetworkGenome> Genomes = genomeList;
            int len = Genomes.Count + archive.Count;
            NetworkGenome neatgenomes;
            List<Pair<double, NetworkGenome>> noveltyList = new List<Pair<double, NetworkGenome>>();
            //if (genomeList.Count == 0) novelty = 0.002;
            foreach (NetworkGenome genome in genomeList)
            {
                //add a list of behaviors
                if (neatGenome != genome)
                noveltyList.Add(new Pair<double, NetworkGenome>(Behavior.BehaviorDistance(((NetworkGenome)genome).BehaviorType, neatGenome.BehaviorType), ((NetworkGenome)genome)));
               
            }
            //noveltyList.Sort(CompareFirst);
            //easy break tires 
            noveltyList = noveltyList.OrderBy(o => o.First).ThenBy(o => o.Second.Fitness).ToList();
            int neighbors = config.nearestNeighbors;
            if (noveltyList.Count < config.nearestNeighbors)
            {
                neighbors = noveltyList.Count;
            }
            //test against k nearest neighbors
            for (int i = 0; i < neighbors; i++)
            {
                novelty += noveltyList[i].First;
                neatGenome.nearestNeighbors++;
            }
            //test against the archive
            foreach (NetworkGenome inArchive in archive)
            {
                novelty += Behavior.BehaviorDistance(neatGenome.BehaviorType, inArchive.BehaviorType);
            }
            //average novelty of genome x
            novelty /= (neighbors + archive.Count());
            return novelty ;
        }

        /// <summary>
        /// add to archive 
        /// called at the end of every generation
        /// </summary>
        /// <param name="GenomesList"></param>
        public static void addToArchive(List<NetworkGenome> GenomesList)
        {
            List<NetworkGenome> sortedList = GenomesList.OrderByDescending(g => g.Novelty).Take(config.archiveLimit).ToList();

            foreach (NetworkGenome archiveList in sortedList)
            {
                if (archiveList.Novelty > config.archiveThreshold)
                    archive.Add(archiveList);
            }

         }

    }
    /// <summary>
    /// KeyValuePair of genomeList 
    /// Helps to associate jth nearest neighbor of genome x with its novelty distance
    /// Implements IComparable to enable sorting of the genomeList
    /// </summary>
    /// <typeparam name="F"></typeparam>
    /// <typeparam name="S"></typeparam>
    public class Pair<F, S> : IComparable where F : System.IComparable<F>
    {
        public Pair() { }
        public Pair(F first, S second)
        {
            this.First = first;
            this.Second = second;
        }
        public F First { get; set; }
        public S Second { get; set; }
        int IComparable.CompareTo(object obj)
        {
            Pair<F, S> c = (Pair<F, S>)obj;
            return this.First.CompareTo(c.First);
        }
    };
}
