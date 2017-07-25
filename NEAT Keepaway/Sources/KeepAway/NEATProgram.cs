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
using System.Threading;

namespace Keepaway
{
    public class NEATProgram
    {
        static NeatEvolutionAlgorithm<NeatGenome> _ea;
        public static KPExperimentConfig config = new KPExperimentConfig();
        const string CHAMPION_FILE = "NEATChamp/keepaway_champion";
        const string Pop_File = "NEATPop/Pop";
        //const string CHAMPION_FILE = "keepaway_champion";
        //const string Pop_File = "Pop";
        //const string Current_Genomes = "Current_Genomes.csv";
        //static AutoResetEvent eventAuto = new AutoResetEvent(false);
        [MTAThread]
        public static void Main(string[] args)
        {
            NEATGame ng = new NEATGame();
            Thread play = new Thread(new ThreadStart(ng.Game));
            play.IsBackground = true;
            play.Start();

           // Initialise log4net (log to console).
            XmlConfigurator.Configure(new FileInfo("log4net.properties"));

            // Experiment classes encapsulate much of the nuts and bolts of setting up a NEAT search.
            KeepawayExperiment experiment = new KeepawayExperiment();

            // Load config XML.
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load("Keepaway.config.xml");
            experiment.Initialize("Keepaway", xmlConfig.DocumentElement);

            // Create evolution algorithm and attach update event.
            _ea = experiment.CreateEvolutionAlgorithm();
            _ea.UpdateEvent += new EventHandler(ea_UpdateEvent);
            // Start algorithm (it will run on a background thread).
            _ea.StartContinue();
            //Hit return to quit
            //Console.ReadLine();
            NEATProgram np = new NEATProgram();
            np.WorkMethod();
            //ThreadPool.QueueUserWorkItem(new WaitCallback(WorkMethod), eventAuto);
            
        }



        void WorkMethod()
        {
            config = KPExperimentConfig.Load("KPExperimentConfig.xml");
            while (_ea.CurrentGeneration <= config.num_Generations)
            {
                Thread.Sleep(Int32.MaxValue);
            }

        }

        static void ea_UpdateEvent(object sender, EventArgs e)
        {
           /* 
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Current_Genomes, true)) //@"C:\Users\Sabre\Desktop\SHARPNEAT\RoboCupSharp\Sources\KeepAway\bin\Debug\Current_Genomes.csv", true))
            {
                file.WriteLine(string.Format("{0} , {1:f6}", _ea.CurrentGeneration, _ea.CurrentChampGenome.EvaluationInfo.Fitness));
            }
            */
            List<NeatGenome> pop = new List<NeatGenome>(_ea.GenomeList);
            var pops = NeatGenomeXmlIO.SaveComplete(pop, false);
            pops.Save(_ea.CurrentGeneration + "Pop.xml");
            //pops.Save(Pop_File + _ea.CurrentGeneration + ".xml");
            // Save the best genome to file
            var doc = NeatGenomeXmlIO.SaveComplete(new List<NeatGenome>() { _ea.CurrentChampGenome}, false);
            doc.Save(_ea.CurrentGeneration + "Champ.xml");
           // doc.Save(CHAMPION_FILE+_ea.CurrentGeneration+".xml");

        }

       

    }
}
