using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keepaway
{
    internal static class HistoricalMarkings
    {
        private static Dictionary<string, HistoricalMarkings.Marking> NodeMarkings { get; set; }

        private static Dictionary<string, HistoricalMarkings.Marking> LinkMarkings { get; set; }

        static HistoricalMarkings()
        {
            HistoricalMarkings.NodeMarkings = new Dictionary<string, HistoricalMarkings.Marking>();
            HistoricalMarkings.LinkMarkings = new Dictionary<string, HistoricalMarkings.Marking>();
        }

        internal static int GetLinkMarking(int sourceId, int targetId, int generation)
        {
            string key = "(" + (object)sourceId + "," + (object)targetId + ")";// string key = "(" + (object)sourceId + "," + (string)(object)targetId + ")";
            if (HistoricalMarkings.LinkMarkings.ContainsKey(key))
                return HistoricalMarkings.LinkMarkings[key].Id;
            HistoricalMarkings.Marking marking = new HistoricalMarkings.Marking();
            marking.GenerationCreated = generation;
            marking.Id = LinkGene.NextId;
            HistoricalMarkings.LinkMarkings[key] = marking;
            return marking.Id;
        }

        internal static int GetNodeMarking(int linkId, int layer, int generation)
        {
            string key = "(" + (object)linkId + "," + (object)layer + ")";//string key = "(" + (object)linkId + "," + (string)(object)layer + ")";
            if (HistoricalMarkings.NodeMarkings.ContainsKey(key))
                return HistoricalMarkings.NodeMarkings[key].Id;
            HistoricalMarkings.Marking marking = new HistoricalMarkings.Marking();
            marking.GenerationCreated = generation;
            marking.Id = NodeGene.NextId;
            HistoricalMarkings.NodeMarkings[key] = marking;
            return marking.Id;
        }

        internal static void CleanUp(int currentGeneration, int maxGenerations)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, HistoricalMarkings.Marking> keyValuePair in HistoricalMarkings.LinkMarkings)
            {
                if (currentGeneration - keyValuePair.Value.GenerationCreated > maxGenerations)
                    list.Add(keyValuePair.Key);
            }
            foreach (string key in list)
                HistoricalMarkings.LinkMarkings.Remove(key);
            list.Clear();
            foreach (KeyValuePair<string, HistoricalMarkings.Marking> keyValuePair in HistoricalMarkings.NodeMarkings)
            {
                if (currentGeneration - keyValuePair.Value.GenerationCreated > maxGenerations)
                    list.Add(keyValuePair.Key);
            }
            foreach (string key in list)
                HistoricalMarkings.NodeMarkings.Remove(key);
        }

        public static void ResetMarkings()
        {
            NodeGene.NextId = 0;
            LinkGene.NextId = 0;
            NetworkGenome.NextId = 0;
            HistoricalMarkings.NodeMarkings.Clear();
            HistoricalMarkings.LinkMarkings.Clear();
        }

        private struct Marking
        {
            internal int Id { get; set; }

            internal int GenerationCreated { get; set; }
        }
    }
}
