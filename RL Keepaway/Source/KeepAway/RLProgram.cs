using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net.Config;
namespace Keepaway
{
    public class RLProgram
    {
        public static RLConfig config = new RLConfig();
        const string performanceFile = "Performance.csv";
        public static int epi;
        public static double reward;
        public static int episode_factored_Counter;
        double fitness , meanfitness;
        static double sum = 0, average_fitness = 0;
        static int count_fit = 150;
        [MTAThread]
        public static void Main(string[] args)
        {
            RLGameWorld ng = new RLGameWorld();
            Thread play = new Thread(new ThreadStart(ng.Game));
            play.IsBackground = true;
            play.Start();
            config = RLConfig.Load("RLConfig.xml");
            RLProgram mainprog = new RLProgram();
            Console.WriteLine("Gets here before it calls main program visualize = {0} and learning ={1}",config.visualize,config.learning);
            if (config.visualize == 0 && config.learning == 1)
            {
                Console.WriteLine("Gets to call main program ");
                mainprog.AllEpisodes();
            }
            Console.WriteLine("It got here");
            Console.ReadLine();
        }

        public static int Episodes { get; set; }
       // public static int epi;

        public void OneEpisode(SMDPAgent agent)
        {
            // call the evaluator
            epi++;
            this.NextEpisode(agent);
            
        }

        int cnt;
        
        
       // int counter;
        private void NextEpisode(SMDPAgent agent)
        {

            config = RLConfig.Load("RLConfig.xml");
            if (Episodes > config.numEpisodes)
                return;
            Console.WriteLine("Gets to NextEpisode() {0} times", cnt++);
            if (Episodes == 0)
            {
                fitness = 0;
                sum = 0;
                episode_factored_Counter = 0;
            }
            Episodes++;

            agent.setEpsilon(config.lambda);
            int num_trails = 0; meanfitness = 0;
            while (num_trails < config.numTrials)
            {
                fitness = RLGameWorld.FitnessValue(agent);
                meanfitness += fitness;
            }
            reward = meanfitness/config.numTrials; // This is to send a reward to endEpisode
            sum += meanfitness/config.numTrials;
            if (Episodes % count_fit == 0)
            {
                episode_factored_Counter++;
                average_fitness = sum / count_fit;
                // recordPerformance(this.Episodes, fitness);
                recordPerformance(episode_factored_Counter, average_fitness);
                sum = 0;
            }



            //this.Previous = this.Current;
            //this.Current = this.Next;
            //this.Next = new KPEnvironment(this.Episode + 1);
        }

        public void recordPerformance(int num, double performance)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(performanceFile, true)) 
            {
                file.WriteLine(string.Format("{0},{1:f6}", num, performance));
            }

        }

        public void AllEpisodes()
        {
            config = RLConfig.Load("RLConfig.xml");
            double[] widthArray = new double[config.numFeatures];
            for (int i = 0; i < config.numFeatures; i++)
                widthArray[i] = 1 / config.numFeatures;

            switch (config.learningMethod)
            {
                case "Q_Learning":
                    {
                        Q_LearningAgent qlp = new Q_LearningAgent(config.numFeatures, config.numActions, true, fitness, widthArray, "weightsFile.data", "weightsFile.data");
                        Console.WriteLine("Gets to call AllEpisodes");
                        while (Episodes <= config.numEpisodes)
                        {
                            qlp.saveWeights("weightsFile.data");
                            this.OneEpisode(qlp);
                        }

                        break;
                    }
                case "SARSA":
                    {
                        SarsaAgent sap = new SarsaAgent(config.numFeatures, config.numActions, true, fitness, widthArray, "weightsFile.data", "weightsFile.data");
                        Console.WriteLine("Gets to call AllEpisodes");
                        while (Episodes <= config.numEpisodes)
                        {
                            if (Episodes % 50 == 0)
                                sap.saveWeights("weightsFile.data");
                            this.OneEpisode(sap);
                        }

                        break;
                    }
                default:
                    {

                        Console.WriteLine("Default case does not exist");
                        break;
                    }
            }


        }


        


    }
}
