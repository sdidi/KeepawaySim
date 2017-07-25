using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net.Config;
using SharpNeat.Core;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System.Xml;
//using System.Windows.Forms;
using System.IO.Pipes;

namespace Keepaway
{

    public class NEATGame
    {

        #region Static Variables

        public static int no_passes = 0;
        public static double ballPos = 0;

        public static RoboCup.Stadium std = new RoboCup.Stadium();

        public static RoboCup.Server timer = new RoboCup.Server(std);

        public static RoboCup.Referees.KeepawayRef kref = new RoboCup.Referees.KeepawayRef(std);

       
        public static bool NoVis = false;
        public static List<NEATKeepawayPlayer> players = new List<NEATKeepawayPlayer>();
        public static KPExperimentConfig config = new KPExperimentConfig();

        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {

            new NEATGame().Game();

        }

        public void Game()
        {
            #region Initialization

            timer.AddReferee(kref);
            config = KPExperimentConfig.Load("KPExperimentConfig.xml");

            #endregion

           
            std.Realtime = false;

            for (int i = 0; i < config.num_Keepers; i++)
            {
                players.Add(new NEATKeepawayPlayer(std, "keepers", i + 1, "l", config.num_Keepers, config.num_Takers));
            }

            for (int i = 0; i < config.num_Takers; i++)
            {
                players.Add(new NEATKeepawayPlayer(std, "takers", i + 1, "r", config.num_Keepers,config.num_Takers));
            }

            for (int i = 0; i < players.Count; i++)
            {
                std.addPlayer(players[i]);
            }

        }
        

        #region Fitness evaluation helper methods
        
        public static double FitnessValue(IBlackBox box, out double[] behVec)
        {
            int cycles = 0; double score = 0; double distfromcentre = 0;
            double[] dist = new double[players.Count];

            for (int i = 0; i < players.Count; i++)
            {
                players[i].Brain = box;
            }

           // for (int i = 0; i < config.trials_per_Evaluation; i++)
           // {

                do
                {

                    timer.RunCycle();
                    cycles++;
                    //computes the team dispersion on the field of play 
                    for (int j = 0; j < players[0].teammates.Count; j++)
                    {
                        dist[j] += players[0].teammatesp[j].distance(players[0].Position);
                    }
                    distfromcentre += ballPos; //add accummulative distance of the ball to the center of field of play

                } while (!kref.episodeEnded);
                kref.episodeEnded = false;
           // }
            double[] fitvalue = new double[4];
            fitvalue[0] = cycles / (config.trials_per_Evaluation * 10.0);
            for (int j = 0; j < players[0].teammates.Count; j++)
            {
                score += (dist[j] / cycles);
            }
            distfromcentre /= cycles;
            score /= (players[0].teammates.Count);
            fitvalue[1] = score;
            fitvalue[2] = no_passes; 
            fitvalue[3] = distfromcentre;
            behVec = fitvalue;
            no_passes = 0; distfromcentre = 0; score = 0;
            return fitvalue[0] ;
        }


        public static double FitnessValue(IBlackBox box)
        {
            int cycles = 0;

            for (int i = 0; i < players.Count; i++)
            {
                players[i].Brain = box;
            }

          //  for (int i = 0; i < config.trials_per_Evaluation; i++)
          //  {

                do
                {

                    timer.RunCycle();

                    cycles++;

                } while (!kref.episodeEnded);

                kref.episodeEnded = false;
            //}
                return cycles / 10.0;
            //return cycles / (config.trials_per_Evaluation * 10.0);
        }

        #endregion




    }
}
