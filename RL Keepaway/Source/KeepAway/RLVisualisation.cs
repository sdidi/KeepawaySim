using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keepaway
{
    //Declare a delegate type for processing an episode
    public delegate void EpisodeDone(RLVisualisation rlv);
    public class RLVisualisation
    {

        const string performanceFile = "Performance.csv";
        double fitness;
        public static double reward;
        public static RLConfig config = new RLConfig();
        public event EpisodeDone OnEpisode;

        //public KPEnvironment Current { get; set; }

        //internal KPEnvironment Previous { get; set; }

        //internal KPEnvironment Next { get; set; }

        public int Episodes { get; set; }
        public static int epi;
        public RLVisualisation()
        {
            this.fitness = 1;
            //Current = new KPEnvironment(1);
           // Previous = new KPEnvironment(0);
            //NextEpisode = new KPEnvironment(2);

        }
        public void OneEpisode(SMDPAgent agent)
        {
            // call the evaluator
            epi++;
            this.NextEpisode(agent);
           if (this.OnEpisode == null)
            {
               // Console.WriteLine("OnEpisode is null !!!!!!");
                return; } 
            this.OnEpisode(this); 
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
                        while (this.Episodes <= config.numEpisodes)
                        {
                            qlp.saveWeights("weightsFile.data");
                            this.OneEpisode(qlp); }
                       
                        break;
                    }
                case "SARSA":
                    {
                        SarsaAgent sap = new SarsaAgent(config.numFeatures, config.numActions, true, fitness, widthArray, "weightsFile.data", "weightsFile.data");
                        Console.WriteLine("Gets to call AllEpisodes");
                        while (this.Episodes <= config.numEpisodes)
                        {
                            if (this.Episodes % 50 == 0)
                                sap.saveWeights("weightsFile.data");
                            this.OneEpisode(sap); }
                        
                        break;
                    }
                default:
                    {

                        Console.WriteLine("Default case does not exist");
                        break;
                    }
            }
            
                           
        }

        public void recordPerformance(int num, double performance)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(performanceFile, true))
            {
                file.WriteLine(string.Format("{0},{1:f6}", num, performance));
            }

        }
        int cnt;
       double sum, average_fitness;
       int count_fit = 20;
       int counter;
        private void NextEpisode(SMDPAgent agent)
        {
             
            config = RLConfig.Load("RLConfig.xml");
            if (this.Episodes > config.numEpisodes)
                return;
            Console.WriteLine("Gets to NextEpisode() {0} times",cnt++);
            if (this.Episodes == 0)
            {
                fitness = 1;
                sum = 0;
                counter = 0;
            }
            this.Episodes++;
            
            agent.setEpsilon(config.lambda);
            fitness = RLGameWorld.FitnessValue(agent);
            reward = fitness; // This is to send a reward to endEpisode
            sum += fitness;
            if (this.Episodes % count_fit ==0)
            {
                counter++;
                average_fitness = sum / count_fit;
               // recordPerformance(this.Episodes, fitness);
                recordPerformance(counter, average_fitness);
                sum = 0;
            }
            
            //this.Previous = this.Current;
            //this.Current = this.Next;
            //this.Next = new KPEnvironment(this.Episode + 1);
        }
    }
}
