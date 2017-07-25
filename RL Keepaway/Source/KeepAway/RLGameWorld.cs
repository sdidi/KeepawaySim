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
using System.Windows.Forms; // added
using System.IO.Pipes;

namespace Keepaway
{

    [Serializable]
    public class RLConfig
    {

        public double fieldX;
        public double fieldY;
        public int num_Keepers;
        public int num_Takers;
        public int numFeatures;
        public int numActions;
        public int numEpisodes;
        public int source_Keepers;
        public int source_Takers;
        public int transfer;
        public int numTrials;
        public int visualize;
        public int learning;
        public string learningMethod;
        public string description;
        public double alpha;
        public double gamma;
        public double epsilon;
        public double lambda;
        public double traceability;

        
        private static System.Xml.Serialization.XmlSerializer serials = new System.Xml.Serialization.XmlSerializer(typeof(RLConfig));

        public static RLConfig Load(string file)
        {
            using (System.IO.StreamReader read = new System.IO.StreamReader(file))
            {

                return serials.Deserialize(read) as RLConfig;

            }


        }

        public static void Save(string file, RLConfig conf)
        {
            using (System.IO.StreamWriter write = new System.IO.StreamWriter(file))
            {

                serials.Serialize(write, conf);

            }


        }
    }
    /// <summary>
    /// Implementation of the NEAT evolution

    public class RLGameWorld
    {

        #region Static Variables

        public static int no_passes = 0;
        public static double ballPos = 0;

        public static RoboCup.Stadium std = new RoboCup.Stadium();

        public static RoboCup.Server timer = new RoboCup.Server(std);

        public static RoboCup.Referees.KeepawayRef kref = new RoboCup.Referees.KeepawayRef(std);
        //create a reference to a RL class that provides visualisation
        public static RLVisualisation rlv = new RLVisualisation();
       
        public static bool NoVis = false;
        public static List<RLKeepawayPlayer> players = new List<RLKeepawayPlayer>();
        public static RLConfig config = new RLConfig();
        public static int round_count = 0;
        
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {

            new RLGameWorld().Game();

        }

        public void Game()
        {


            #region Initialization

            timer.AddReferee(kref);
            config = RLConfig.Load("RLConfig.xml");

            #endregion

           
            if (config.visualize == 1 && config.learning == 0) //Heuristic Policy with Visualisation
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                RoboCup.FieldVisualizer f = new RoboCup.FieldVisualizer();
                std.Realtime = false;
                timer.OnCycle += f.OnStadiumUpdate;
                for (int i = 0; i < config.num_Keepers; i++)
                {
                    std.addPlayer(new FixedKeepawayPlayer(std, "keepers", i + 1, "l", config.num_Keepers, config.num_Takers));


                }

                for (int i = 0; i < config.num_Takers; i++)
                {
                    std.addPlayer(new FixedKeepawayPlayer(std, "takers", i + 1, "r", config.num_Keepers, config.num_Takers));
                }
                f.std = std;
                f.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker1_DoWork);


                Application.Run(f);

            }
            else if (config.visualize == 1 && config.learning == 1) //Reinforcement Learning with Visualisation
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                //Visualizer form = new Visualizer();

               // rlv.OneEpisode += form.UpdateGraph;

                RoboCup.FieldVisualizer f = new RoboCup.FieldVisualizer();
                std.Realtime = false;
                timer.OnCycle += f.OnStadiumUpdate;

                for (int i = 0; i < config.num_Keepers; i++)
                {
                    players.Add(new RLKeepawayPlayer(std, "keepers", i + 1, "l", config.num_Keepers, config.num_Takers));


                }

                for (int i = 0; i < config.num_Takers; i++)
                {
                    players.Add(new RLKeepawayPlayer(std, "takers", i + 1, "r", config.num_Keepers, config.num_Takers));
                }

                for (int i = 0; i < players.Count; i++)
                {
                    std.addPlayer(players[i]);
                }

                f.std = std;
                //invokes an event handler that calls RLVisualisation methods
                f.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker2_DoWork);

                 Application.Run(f);
                

            }
            else //Reinforcement learning without Visualisation
            {
                Console.WriteLine("Initialises the players well ");
                std.Realtime = false;

                for (int i = 0; i < config.num_Keepers; i++)
                {
                    players.Add(new RLKeepawayPlayer(std, "keepers", i + 1, "l", config.num_Keepers, config.num_Takers));
                }

                for (int i = 0; i < config.num_Takers; i++)
                {
                    players.Add(new RLKeepawayPlayer(std, "takers", i + 1, "r", config.num_Keepers, config.num_Takers));
                }

                for (int i = 0; i < players.Count; i++)
                {
                    std.addPlayer(players[i]);
                }

            }
        }

        #region Fitness evaluation helper methods
        public static double FitnessValue(SMDPAgent policy)
        {
            int cycles = 0;
            config = RLConfig.Load("RLConfig.xml");
            
            switch (config.learningMethod)
            {
                case "Q_Learning":
                    {   for (int i = 0; i < players.Count; i++)
                          players[i].qlPolicy = (Q_LearningAgent)policy;
                        break;
                    }
                case "SARSA":
                    {   for (int i = 0; i < players.Count; i++)
                           players[i].saPolicy = (SarsaAgent)policy;
                        break;
                    }
                default:
                    Console.WriteLine("Default case does not exist");
                    break;
            }
            
                do
                {

                    timer.RunCycle();

                    cycles++;

                } while (!kref.episodeEnded);
                
                kref.episodeEnded = false;
                return cycles / (10.0); 
        }

        #endregion

        #region Reinforcement Learning helper method
        static void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            System.ComponentModel.BackgroundWorker w = sender as System.ComponentModel.BackgroundWorker;
            try
            {
                rlv.AllEpisodes(); 
               
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        #endregion



        #region Fixed policy setup helper methods

        static void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            System.ComponentModel.BackgroundWorker w = sender as System.ComponentModel.BackgroundWorker;

            while (true)
            {
                int cycles = 0;

                do
                {

                    timer.RunCycle();
                    // w.ReportProgress(cycles % 99);
                    cycles++;



                    //    Console.ReadKey(); ;
                } while (!kref.episodeEnded);

                kref.episodeEnded = false;

            }



        }

        #endregion


    }
}
