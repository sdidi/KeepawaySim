/**********************************Keepaway Experiment********************************************
 * 
 * Adapted for the Keep away Experiment from Colin Green SharpNEATv2 code
 * 
 * ***********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Decoders;
using System.Threading.Tasks;
using SharpNeat.Core;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Decoders.Neat;
using SharpNeat.Phenomes;
using SharpNeat.DistanceMetrics;
using SharpNeat.SpeciationStrategies;
using System.Xml;

using log4net;


namespace Keepaway
{
    class KeepawayExperiment : INeatExperiment
    {
        private static readonly ILog __log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        NeatEvolutionAlgorithmParameters _eaParams;
        NeatGenomeParameters _neatGenomeParams;
        string _name;
        int _populationSize;
        int _specieCount;
        bool _transfer;
        bool _seed;
        NetworkActivationScheme _activationScheme;
        string _complexityRegulationStr;
        int? _complexityThreshold;

        int _trialsPerEvaluation;
        string _description;
        ParallelOptions _parallelOptions;

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public KeepawayExperiment()
        {
        }

        #endregion

        #region INeatExperiment

        /// <summary>
        /// Gets the name of the experiment.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets human readable explanatory text for the experiment.
        /// </summary>
        public string Description
        {
            get { return _description; }
        }

        /// <summary>
        /// Gets the number of inputs required by the network/black-box that the underlying problem domain is based on.
        /// </summary>
        public int InputCount
        {
            get { return 13; }
        }

        /// <summary>
        /// Gets the number of outputs required by the network/black-box that the underlying problem domain is based on.
        /// </summary>
        public int OutputCount
        {
            get { return 3; }
        }

        /// <summary>
        /// Gets the default population size to use for the experiment.
        /// </summary>
        public int DefaultPopulationSize
        {
            get { return _populationSize; }
        }

        /// <summary>
        /// Gets the NeatEvolutionAlgorithmParameters to be used for the experiment. Parameters on this object can be 
        /// modified. Calls to CreateEvolutionAlgorithm() make a copy of and use this object in whatever state it is in 
        /// at the time of the call.
        /// </summary>
        public NeatEvolutionAlgorithmParameters NeatEvolutionAlgorithmParameters
        {
            get { return _eaParams; }
        }

        /// <summary>
        /// Gets the NeatGenomeParameters to be used for the experiment. Parameters on this object can be modified. Calls
        /// to CreateEvolutionAlgorithm() make a copy of and use this object in whatever state it is in at the time of the call.
        /// </summary>
        public NeatGenomeParameters NeatGenomeParameters
        {
            get { return _neatGenomeParams; }
        }

        /// <summary>
        /// Initialize the experiment with some optional XML configutation data.
        /// </summary>
        public void Initialize(string name, XmlElement xmlConfig)
        {
            _name = name;
            _populationSize = XmlUtils.GetValueAsInt(xmlConfig, "PopulationSize");
            _specieCount = XmlUtils.GetValueAsInt(xmlConfig, "SpecieCount");
            _transfer = XmlUtils.GetValueAsBool(xmlConfig, "Transfer");
            _seed = XmlUtils.GetValueAsBool(xmlConfig, "Seed");
            _activationScheme = ExperimentUtils.CreateActivationScheme(xmlConfig, "Activation");
            _complexityRegulationStr = XmlUtils.TryGetValueAsString(xmlConfig, "ComplexityRegulationStrategy");
            _complexityThreshold = XmlUtils.TryGetValueAsInt(xmlConfig, "ComplexityThreshold");

            _trialsPerEvaluation = XmlUtils.GetValueAsInt(xmlConfig, "TrialsPerEvaluation");
            _description = XmlUtils.TryGetValueAsString(xmlConfig, "Description");
            _parallelOptions = ExperimentUtils.ReadParallelOptions(xmlConfig);
            _eaParams = new NeatEvolutionAlgorithmParameters();
            _eaParams.SpecieCount = _specieCount;
            _neatGenomeParams = new NeatGenomeParameters();
        }

        /// <summary>
        /// Load a population of genomes from an XmlReader and returns the genomes in a new list.
        /// The genome factory for the genomes can be obtained from any one of the genomes.
        /// </summary>
        public List<NeatGenome> LoadPopulation(XmlReader xr)
        {
            NeatGenomeFactory genomeFactory = (NeatGenomeFactory)CreateGenomeFactory();
            return NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, genomeFactory);
            // return NeatGenomeUtils.LoadPopulation(xr, false, this.InputCount, this.OutputCount);
        }

        /// <summary>
        /// Save a population of genomes to an XmlWriter.
        /// </summary>
        public void SavePopulation(XmlWriter xw, IList<NeatGenome> genomeList)
        {
            // Writing node IDs is not necessary for NEAT.
            NeatGenomeXmlIO.WriteComplete(xw, genomeList, true);

        }

        /// <summary>
        /// Create a genome decoder for the experiment.
        /// </summary>
        public IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
        {
            return new NeatGenomeDecoder(_activationScheme);
        }

        /// <summary>
        /// Create a genome factory for the experiment.
        /// Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
        /// </summary>
        public IGenomeFactory<NeatGenome> CreateGenomeFactory()
        {
            return new NeatGenomeFactory(InputCount, OutputCount, _neatGenomeParams);
        }

        /// <summary>
        /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
        /// of the algorithm are also constructed and connected up.
        /// Uses the experiments default population size defined in the experiment's config XML.
        /// </summary>
        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm()
        {
            return CreateEvolutionAlgorithm(_populationSize);
        }

        /// <summary>
        /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
        /// of the algorithm are also constructed and connected up.
        /// includes a choice to transfer a trained policy or generate a new population from scratch
        /// </summary>
        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(int populationSize)
        {
            // Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            IGenomeFactory<NeatGenome> genomeFactory = CreateGenomeFactory();
            List<NeatGenome> genomeList = new List<NeatGenome>(); //creates a genomeList object
            XmlReader xrd = XmlReader.Create("Population.xml"); //facilitates transfer
            //XmlReader xs = XmlReader.Create("Champ.xml"); // facilitates seeding of Champ
            //the following code implements the policy transfer
            if (_transfer)
            {
                //if policy transfer selected, load the "Population.xml" file
                genomeList = this.LoadPopulation(xrd);
            }
            else if(_seed) {
                //if seeding the initial population
                //NeatGenome genome = NeatGenomeXmlIO.ReadGenome(xs, false);
                //genomeList = genomeFactory.CreateGenomeList(populationSize, 0, genome);
                genomeList = genomeFactory.CreateGenomeList(populationSize, 0);
            }
            else
            {
                // if training from scratch.
                genomeList = genomeFactory.CreateGenomeList(populationSize, 0);
            }
            // Create evolution algorithm.
            return CreateEvolutionAlgorithm(genomeFactory, genomeList);
        }

        /// <summary>
        /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
        /// of the algorithm are also constructed and connected up.
        /// This overload accepts a pre-built genome population and their associated/parent genome factory.
        /// </summary>
        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenome> genomeFactory, List<NeatGenome> genomeList)
        {
            // Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weigth difference.
            IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
            ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, _parallelOptions);

            // Create complexity regulation strategy.
            IComplexityRegulationStrategy complexityRegulationStrategy = ExperimentUtils.CreateComplexityRegulationStrategy(_complexityRegulationStr, _complexityThreshold);

            // Create the evolution algorithm.
            NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, complexityRegulationStrategy);

            // Create IBlackBox evaluator.
            KeepawayEvaluator evaluator = new KeepawayEvaluator();

            // Create genome decoder.
            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = CreateGenomeDecoder();

            // TODO: evaulation scheme that re-evaulates existing genomes and takes average over time.
            // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
           // IGenomeListEvaluator<NeatGenome> genomeListEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator, _parallelOptions);
            //IGenomeListEvaluator<NeatGenome> genomeListEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator);
            IGenomeListEvaluator<NeatGenome> genomeListEvaluator = new SerialGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator, true);
            // Initialize the evolution algorithm.
            ea.Initialize(genomeListEvaluator, genomeFactory, genomeList);

            // Finished. Return the evolution algorithm
            return ea;
        }



        #endregion
    }
}
