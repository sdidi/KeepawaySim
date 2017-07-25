using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using System.Windows.Forms;
using System.Threading;
using SharpNeat.EvolutionAlgorithms;
namespace Keepaway
{
    public class KeepawayEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        
        //evolution variables
        private static ulong _evalCount;
        private bool _stopConditionSatisfied;
        private double scale = 15;
        public static KPExperimentConfig config = new KPExperimentConfig();

        /// <summary>
        /// Gets the total number of evaluations that have been performed.
        /// </summary>
        public ulong EvaluationCount
        {
            get { return _evalCount; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the the evolutionary algorithm/search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        public bool StopConditionSatisfied
        {
            get { return _stopConditionSatisfied; }
        }

        public FitnessInfo Evaluate(IBlackBox agent, out double[] behVector)
        {
            config = KPExperimentConfig.Load("KPExperimentConfig.xml");
            double fitness = 0;
            double[] values = new double[4];
            //synchronize the access to game controller
            lock (this)
            {
                //call a keepaway game controller
                fitness = NEATGame.FitnessValue(agent, out values);
            }
            behVector = values;
            _evalCount++;
            fitness /= scale;
            //termination condition
            if (fitness >= 1)
                _stopConditionSatisfied = true;

            return new FitnessInfo(fitness, fitness);
        }

        public FitnessInfo Evaluate(IBlackBox agent)
        {
            double fitness=0;
            double[] score = new double[4];
            //synchronize the access to game controller
            lock (this)
            {
                //call a keepaway game controller
                fitness = NEATGame.FitnessValue(agent, out score);
            }
            _evalCount++;
            fitness /= scale;
            //termination condition
            if (fitness >= 1 )
                _stopConditionSatisfied = true;

            return new FitnessInfo(fitness, score);

        }


        
        public void Reset()
        {


        }

    }
}
