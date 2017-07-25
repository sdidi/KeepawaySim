

/****Adapted from RoboCupSharp benchmark task by Phillip Verbancsics - http://eplex.cs.ufc.edu ***************/
// Modified to suit our experiment for testing policy transfer with behavioral diversity and genotype diversity
// Paper : Multi-Agent Behavior-Based Policy Transfer
// Visualisation removed so that it could be run in a Linux HPC Cluster.
/************************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;


namespace Keepaway
{
    /// <summary>
    /// Configuration file for HyperNEAT keepaway
    /// </summary>
    [Serializable]
    public class KeepawayConfig
    {
        public double FieldX;
        public double FieldY;
        public double SubstrateX;
        public double SubstrateY;
        public double SubstrateStartX;
        public double SubstrateStartY;
        public double SubstrateEndX;
        public double SubstrateEndY;
        public int NumKeepers;
        public int NumTakers;
        public double scalariser;
        public double archiveThreshold;
        public int Episodes;
        public int nearestNeighbors;
        public int archiveLimit;
        public int archiveSize;        
        public int SearchMethod;
        public string Description;

        private static System.Xml.Serialization.XmlSerializer serial = new System.Xml.Serialization.XmlSerializer(typeof(KeepawayConfig));

        public static KeepawayConfig Load(string file)
        {
            using (System.IO.StreamReader read = new System.IO.StreamReader(file))
            {

                return serial.Deserialize(read) as KeepawayConfig;

            }


        }
        public static void Save(string file, KeepawayConfig conf)
        {
            using (System.IO.StreamWriter write = new System.IO.StreamWriter(file))
            {

                serial.Serialize(write, conf);

            }


        }
    }
    static class Program
    {

        #region Static Variables
        public static int no_passes = 0;
        public static double ballPos = 0;
        public static RoboCup.Stadium std = new RoboCup.Stadium();
        public static RoboCup.Server timer = new RoboCup.Server(std);
        public static RoboCup.Referees.KeepawayRef kref = new RoboCup.Referees.KeepawayRef(std);
        public static bool HyperNEATExperiment = false;
        public static List<HyperNEATKeepawayPlayer> players = new List<HyperNEATKeepawayPlayer>();
        public static KeepawayConfig config = new KeepawayConfig();
        public static Substrates sub = new Substrates();
        public static Evolutions evo = new Evolutions();
        //public static double max_teamDispersion = 0.0, max_distCenter = 0.0, max_noPasses = 0.0;
        public static double[] maxVec = new double[4];
        public static double fitnormaliser = 12;
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] args)
        {

            #region Initialization

            timer.AddReferee(kref);
            
           config = KeepawayConfig.Load("KeepawayConfig.xml");
           HyperNEATKeepawayPlayer.config = config;
            

            evo.SetParameters("Parameters.xml");

            evo.Evaluator = new EvaluateGenome(HyperNEATKeepaway);
            evo.Initialise();

            sub.GenerateNodes(NodeGeneration);
            sub.GenerateLinks(LinkCreation);

            std.Realtime = false;

            for (int i = 0; i < config.NumKeepers; i++)
            {
                players.Add(new HyperNEATKeepawayPlayer(std, "keepers", i + 1, "l", config.NumKeepers, config.NumTakers));


            }

            for (int i = 0; i < config.NumTakers; i++)
            {
                players.Add(new HyperNEATKeepawayPlayer(std, "takers", i + 1, "r", config.NumKeepers, config.NumTakers));
            }

            for (int i = 0; i < players.Count; i++)
            {
                std.addPlayer(players[i]);
            }



            evo.AllGenerations();


            #endregion


        }

        #region HyperNEAT helper methods
        //phillip
        static void LinkCreation(NetworkGenome sub)
        {
            LinkGene temp;
            int linkId = 0;
            for (int i = 0; i < sub.Nodes.Count; i++)
            {
                for (int j = 0; j < sub.Nodes.Count && sub.Nodes[i].Layer > sub.Nodes[j].Layer; j++)
                {

                    if (sub.Nodes[i].Layer - sub.Nodes[j].Layer == 1)
                    {
                        temp = new LinkGene();
                        temp.Id = linkId;
                        linkId++;
                        temp.Source = sub.Nodes[j].Id;
                        temp.Target = sub.Nodes[i].Id;
                        temp.Weight = 0;
                        sub.Links.Add(temp);

                    }
                }
            }
        }
        //phillip
        static List<NodeGene> NodeGeneration()
        {
            List<NodeGene> list = new List<NodeGene>();
            SubstrateNodes temp;

            int nodeId = 0;
            temp = new SubstrateNodes();
            temp.Function = "Identity";
            temp.Id = nodeId;
            nodeId++;
            temp.Layer = 0;
            temp.Coordinate.Add(0);
            temp.Coordinate.Add(0);
            temp.Type = NodeType.Bias;
            list.Add(temp);


            double xIncrement = Math.Abs(config.SubstrateEndX - config.SubstrateStartX) / config.SubstrateX;
            double yIncrement = Math.Abs(config.SubstrateEndY - config.SubstrateStartY) / config.SubstrateY;
            for (double y = config.SubstrateStartY + yIncrement / 2; y <= config.SubstrateEndY - yIncrement / 2; y += yIncrement)
            {
                for (double x = config.SubstrateStartX + xIncrement / 2; x <= config.SubstrateEndX - xIncrement / 2; x += xIncrement)
                {

                    temp = new SubstrateNodes();

                    temp.Type = NodeType.Input;
                    temp.Layer = 0;
                    temp.Id = nodeId;
                    nodeId++;
                    temp.Function = "Identity";
                    temp.Coordinate.Add(x);
                    temp.Coordinate.Add(y);
                    list.Add(temp);


                }
            }

            for (double y = config.SubstrateStartY + yIncrement / 2; y <= config.SubstrateEndY - yIncrement / 2; y += yIncrement)
            {
                for (double x = config.SubstrateStartX + xIncrement / 2; x <= config.SubstrateEndX - xIncrement / 2; x += xIncrement)
                {

                    temp = new SubstrateNodes();

                    temp.Type = NodeType.Output;
                    temp.Layer = 1;
                    temp.Id = nodeId;
                    nodeId++;
                    temp.Function = "BipolarSigmoid";
                    temp.Coordinate.Add(x);
                    temp.Coordinate.Add(y);
                    list.Add(temp);


                }
            }

            return list;
        }

        static Dictionary<int, int> mapping = new Dictionary<int, int>();

        static List<DecodedNetworks> createdNetworks = new List<DecodedNetworks>();

        static List<int> possibleValues = new List<int>();
        //based on phillip modified 
        static bool HyperNEATKeepaway(List<NetworkGenome> genomes, bool test)
        {
            if (!test)
            {
                int evaluations = 0;
                Dictionary<int, int> evalCount = new Dictionary<int, int>();
                List<int> keys = new List<int>(mapping.Keys);
                for (int i = 0; i < genomes.Count; i++)
                {

                    possibleValues.Add(i);
                }

                for (int i = 0; i < keys.Count; i++)
                {
                    if (!genomes.Any(gen => gen.Id == keys[i]))
                    {

                        mapping.Remove(keys[i]);
                    }
                    else
                    {

                        possibleValues.Remove(mapping[keys[i]]);
                    }

                }

                for (int i = 0; i < genomes.Count; i++)
                {
                    genomes[i].Fitness = 0;
                    for (int l = 0; l < maxVec.Length; l++)
                    {
                        genomes[i].BehaviorType.bVector[l] = 0;
                    }
                    if (!mapping.ContainsKey(genomes[i].Id))
                    {
                        DecodedNetworks net = sub.GenerateNetwork(MapWeights, DecodedNetworks.DecodeGenome(genomes[i]));

                        if (createdNetworks.Count < genomes.Count)
                        {
                            createdNetworks.Add(net.Clone() as DecodedNetworks);
                            mapping.Add(genomes[i].Id, i);
                        }
                        else
                        {
                            createdNetworks[possibleValues[0]].CopyLinks(net);
                            mapping.Add(genomes[i].Id, possibleValues[0]);
                            possibleValues.RemoveAt(0);
                        }

                    }

                }

                for (int i = 0; i < genomes.Count; i++)
                {

                    //receives the fitness value and behavioral characterisation values from
                    //Fitness Evaluation function.
                    double[] fitness = new double[4];

                    for (int j = 0; j < config.Episodes; j++)
                    {
                        fitness = Evaluate(createdNetworks[mapping[genomes[i].Id]]);
                        genomes[i].RealFitness += fitness[0];
                        for (int v = 0; v < fitness.Length; v++)
                        {
                            genomes[i].BehaviorType.bVector[v] += fitness[v];
                        }
                        
                    }
                    genomes[i].RealFitness /= config.Episodes;
                    for (int v = 0; v < fitness.Length; v++)
                    {
                        genomes[i].BehaviorType.bVector[v] /= config.Episodes;
                    }
                    evalCount[genomes[i].Id] = config.Episodes;
                    evaluations += config.Episodes;
                }


                List<double> lists = new List<double>();

                genomes.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
                double Max0 = 0; double Max1 = 0; double Max2 = 0; double fit = 0;

                for (int i = 0; i < genomes.Count; i++)
                {
                    maxVec[0] = genomes[i].RealFitness;
                    for (int v = 1; v < maxVec.Length; v++)
                    {
                        if (genomes[i].BehaviorType.bVector[v] > maxVec[v])
                        {
                            maxVec[v] = genomes[i].BehaviorType.bVector[v];
                        }
                    }
                }  
        
                //double fitnormaliser = 11;
                for (int j = 0; j < genomes.Count; j++)
                {
                    genomes[j].RealFitness /= fitnormaliser;
                    genomes[j].BehaviorType.bVector[0] /= fitnormaliser;
                    for (int v = 1; v < maxVec.Length; v++)
                    {
                        genomes[j].BehaviorType.bVector[v] /= maxVec[v];
                    }
                }
                
                foreach (NetworkGenome genome in genomes)
                {                    
                    genome.Novelty = genome.NoveltyMeasure.computeNovelty(genome, genomes);
                    genome.Genotypic_Diversity = genome.GenomeDiversity.DiversityDistance(genomes, genome)/110;
                    switch (config.SearchMethod)
                    {

                        case 1:
                            {
                                genome.Fitness = genome.RealFitness;//Objective based Search
                                break;
                            }
                        case 2:
                            {
                                genome.Fitness = genome.Novelty; //Novelty Search 
                                break;
                            }
                        case 3:
                            {
                                genome.Fitness = genome.RealFitness * config.scalariser + genome.Novelty * (1 - config.scalariser);
                                break; //Hybrid (Novelty + Objective based Search)
                            }
                        case 4:
                            {
                                genome.Fitness = genome.Genotypic_Diversity; //Genotypic Diversity Search
                                break;
                            }
                        case 5:
                            {
                                genome.Fitness = genome.RealFitness * config.scalariser + genome.Genotypic_Diversity * (1 - config.scalariser);
                                break; //Hybrid (Objective + Genotypic Diversity)
                            }
                        default:
                            {
                                genome.Fitness = genome.RealFitness;
                                break;
                            }
                    }
                    

                }

                ComputeNovelty.addToArchive(genomes); //add the best N genomes to archive
                
                return false;

            }
            else
            {

                for (int i = 0; i < genomes.Count; i++)
                {
                    if (genomes.FindIndex(g => genomes[i].Id == g.Id) < i) continue;
                    genomes[i].Fitness = 0;

                    {
                        DecodedNetworks net = sub.GenerateNetwork(MapWeights, DecodedNetworks.DecodeGenome(genomes[i]));
                        double[] fitness = new double[4];
                        for (int j = 0; j < config.Episodes; j++)
                        {
                            fitness = Evaluate(createdNetworks[mapping[genomes[i].Id]]);
                            genomes[i].RealFitness += fitness[0];
                            for (int v = 0; v < fitness.Length; v++)
                            {
                                genomes[i].BehaviorType.bVector[v] += fitness[v];
                            }
                        }
                        genomes[i].RealFitness /= config.Episodes;
                        for (int v = 0; v < fitness.Length; v++)
                        {
                            genomes[i].BehaviorType.bVector[v] /= config.Episodes;
                        }
                        
                    }

                }
                //double fitnormaliser = 11;                
                for (int i = 0; i < genomes.Count; i++)
                {
                    maxVec[0] = genomes[i].RealFitness;
                    for (int v = 1; v < maxVec.Length; v++)
                    {
                        if (genomes[i].BehaviorType.bVector[v] > maxVec[v])
                        {
                            maxVec[v] = genomes[i].BehaviorType.bVector[v];
                        }
                    }                    
                }  
                    for (int j = 0; j < genomes.Count; j++)
                    {
                        genomes[j].RealFitness /= fitnormaliser;
                        genomes[j].BehaviorType.bVector[0] /= fitnormaliser ;
                        for (int v = 1; v < maxVec.Length; v++)
                        {
                            genomes[j].BehaviorType.bVector[v] /= maxVec[v];
                        }
                     }

                foreach (NetworkGenome genome in genomes)
                {
                    genome.Novelty = genome.NoveltyMeasure.computeNovelty(genome, genomes);
                    genome.Genotypic_Diversity = genome.GenomeDiversity.DiversityDistance(genomes, genome) / 110;
                    switch (config.SearchMethod)
                    {

                        case 1:
                            {
                                genome.Fitness = genome.RealFitness;//Objective based Search
                                break;
                            }
                        case 2:
                            {
                                genome.Fitness = genome.Novelty; //Novelty Search 
                                break;
                            }
                        case 3:
                            {
                                genome.Fitness = genome.RealFitness * config.scalariser + genome.Novelty * (1 - config.scalariser);
                                break; //Hybrid (Novelty + Objective based Search)
                            }
                        case 4:
                            {
                                genome.Fitness = genome.Genotypic_Diversity; //Genotypic Diversity Search
                                break;
                            }
                        case 5:
                            {
                                genome.Fitness = genome.RealFitness * config.scalariser + genome.Genotypic_Diversity * (1 - config.scalariser);
                                break; //Hybrid (Objective + Genotypic Diversity)
                            }
                        default:
                            {
                                genome.Fitness = genome.RealFitness;
                                break;
                            }
                    }
                }
                ComputeNovelty.addToArchive(genomes); //add the N genomes to archive
            }

            return false;
        }
        //modified
        private static double[] Evaluate(DecodedNetworks network)
        {
            int cycles = 0; double teamDispersion = 0; double distfromcentre = 0;
            double[] dist = new double[players.Count];
            double[] values = new double[4];
            for (int i = 0; i < players.Count; i++)
            {
                players[i].net = network;
            }

            do
            {

                timer.RunCycle();
                cycles++;
                //computes the team dispersion on the field of play 
                for (int j = 0; j < players[0].teammates.Count; j++)
                {
                    dist[j] += players[0].teammatesp[j].distance(players[0].Position);
                }
                distfromcentre += ballPos;

            } while (!kref.episodeEnded);
            kref.episodeEnded = false;

            for (int j = 0; j < players[0].teammates.Count; j++)
            {
                teamDispersion += (dist[j] / cycles);
            }
            distfromcentre /= cycles;
            teamDispersion /= (players[0].teammates.Count);
            cycles /= 10;
            values[0] = cycles; values[1]= teamDispersion;
            values[2] = no_passes; values[3] = distfromcentre;
            no_passes = 0; distfromcentre = 0; teamDispersion = 0;
            return values;
        }

        static bool saveSub = true;
        //by phillip
        static void MapWeights(NetworkGenome genome, DecodedNetworks cppn)
        {

            double[] coords = new double[4];
            double[] output;
            LinkGene temp;
            SubstrateNodes source;
            SubstrateNodes target;

            int linkId = 0;
            for (int i = 1; i < genome.Links.Count; i++)
            {

                temp = genome.Links[i];
                source = (SubstrateNodes)genome.Nodes[temp.Source];
                target = (SubstrateNodes)genome.Nodes[temp.Target];
                if (temp.Source != 0)
                {
                    coords[0] = source.Coordinate[0];
                    coords[1] = source.Coordinate[1];

                }
                else
                {
                    coords[0] = target.Coordinate[0];
                    coords[1] = target.Coordinate[1];
                }
                coords[2] = target.Coordinate[0];
                coords[3] = target.Coordinate[1];
                cppn.Flush();
                cppn.SetInputs(coords);
                cppn.ActivateNetwork(5);
                output = cppn.GetOutputs();


                if (source.Id != 0)
                {
                    temp.Weight = output[0];
                }
                else
                {
                    temp.Weight = output[1];
                }



            }

            if (saveSub)
            {
                NetworkGenome.SaveToFile(genome, "Sub.xml");
                saveSub = false;
            }
        }
        //by Phillip
        static void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            System.ComponentModel.BackgroundWorker w = sender as System.ComponentModel.BackgroundWorker;
            try
            {
                evo.AllGenerations();

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        #endregion

        #region Fixed policy setup helper methods
        //by phillip
        static void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            System.ComponentModel.BackgroundWorker w = sender as System.ComponentModel.BackgroundWorker;

            while (true)
            {
                int cycles = 0;

                do
                {

                    timer.RunCycle();
                       cycles++;

                                       
                } while (!kref.episodeEnded);


                kref.episodeEnded = false;

            }



        }

        #endregion




    }
}
