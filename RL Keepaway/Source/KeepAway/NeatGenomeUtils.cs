using log4net;
using SharpNeat.Genomes.Neat;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Keepaway
{
    public static class NeatGenomeUtils
    {
        private static readonly ILog __log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static List<NeatGenome> LoadPopulation(XmlReader xr, bool nodeFnIds, int inputCount, int outputCount)
        {
            
            List<NeatGenome> list = new List<NeatGenome>() ;//= NeatGenomeXmlIO.ReadCompleteGenomeList(xr, nodeFnIds);
            //List<NeatGenome> list = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, nodeFnIds);
           /*
            for (int index = list.Count - 1; index > -1; --index)
            {
                NeatGenome neatGenome = list[index];
                if (neatGenome.InputNodeCount != inputCount)
                {
                    NeatGenomeUtils.__log.ErrorFormat("Loaded genome has wrong number of inputs for currently selected experiment. Has [{0}], expected [{1}].", (object)neatGenome.InputNodeCount, (object)inputCount);
                    list.RemoveAt(index);
                }
                else if (neatGenome.OutputNodeCount != outputCount)
                {
                    NeatGenomeUtils.__log.ErrorFormat("Loaded genome has wrong number of outputs for currently selected experiment. Has [{0}], expected [{1}].", (object)neatGenome.OutputNodeCount, (object)outputCount);
                    list.RemoveAt(index);
                }
            } */
            return list;
        }
    }
}
