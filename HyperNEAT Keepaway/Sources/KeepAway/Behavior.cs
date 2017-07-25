/*****************************************************************************************************
 * 
 * 
 * Behavior Class : Quantifies the team behavior and finds the distance between behaviors
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
    public class Behavior
    {
        //public double[] behaviorVector = new double[3];//4
        public double[] bVector = new double[4];
        public double[] objective;
        public static List<Behavior> archive = new List<Behavior>();
        //public static double archive_threshold = 0.03;
        public static List<Behavior> tempArchive = new List<Behavior>();
        public static KeepawayConfig config = new KeepawayConfig();
        public Behavior()
        {
           
        }
      
        public Behavior(Behavior copy)
        {
            if (copy.bVector != null)
            {
                bVector = copy.bVector;
            }

        }
        /*
        public double BehaviorDistance(List <NetworkGenome> GenomesList, NetworkGenome genome){
             List<NetworkGenome> Genomes = GenomesList;
            double dist = 0;
            int len = Genomes.Count+archive.Count;
           foreach (NetworkGenome networkGenomes in Genomes)
            {
                if (networkGenomes != genome)
                dist += BehaviorDistanceMeasure(genome.BehaviorType,networkGenomes.BehaviorType);
            }
           
           foreach (Behavior inArchive in archive)
           {
                dist += BehaviorDistanceMeasure(genome.BehaviorType, inArchive);
           }
            

            dist /= len;

            if (dist > archive_threshold)
                tempArchive.Add(genome.BehaviorType);

            return dist; //>1.0 ? 1.0:dist;
        }
     
        public static double BehaviorDistance(double[] x, double[] y)
        {
            double dist = 0.0; double delta = 0.0;
            for (int i = 0; i < x.Count(); i++)
            {
                delta = x[i] - y[i];
                dist += delta * delta;
            }
            dist /= x.Count();

            return dist;
        }
      */
      public static double BehaviorDistance(Behavior x, Behavior y)
        {
            double dist = 0.0; double delta = 0.0;
           for (int i = 0; i < x.bVector.Count()-1; i++)
            {
                delta = x.bVector[i] - y.bVector[i];
                if (delta < 0) delta = 0; //testing  
                dist += Math.Sqrt(delta * delta);
            }
            dist /= (x.bVector.Count()-1);

            return dist;
        }


      public static void addToArchive(List<NetworkGenome> GenomesList)
      {
          config = KeepawayConfig.Load("KeepawayConfig.xml");
          List<NetworkGenome> sortedList = GenomesList.OrderByDescending(g => g.Novelty).Take(config.archiveLimit).ToList();
          
          foreach (NetworkGenome archiveList in sortedList)
          {
              if (archiveList.Novelty > config.archiveThreshold)
              archive.Add(archiveList.BehaviorType);
          }
                            
      }

      
    }


    
}
