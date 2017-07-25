using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Keepaway.Parameters
{
    [XmlInclude(typeof(Mutation.FunctionMutation))]
    [XmlInclude(typeof(Mutation.WeightMutation))]
    [Serializable]
    public class Mutation
    {
        private static XmlSerializer Serial { get; set; }

        public bool Recurrence { get; set; }

        public double AddNode { get; set; }

        public double KeepLinkOnAddNode { get; set; }

        public double ChangeNodeFunction { get; set; }

        [XmlArray]
        public List<Mutation.FunctionMutation> FunctionProbabilities { get; set; }

        public double AddConnection { get; set; }

        public double DeleteConnection { get; set; }

        public double MutateWeights { get; set; }

        [XmlArray]
        public List<Mutation.WeightMutation> WeightProbabilities { get; set; }

        public double WeightRange { get; set; }

        static Mutation()
        {
            Mutation.Serial = new XmlSerializer(typeof(Mutation));
        }

        public Mutation()
        {
            this.AddConnection = 0.3;
            this.AddNode = 0.1;
            this.ChangeNodeFunction = 0.05;
            this.DeleteConnection = 0.15;
            this.FunctionProbabilities = new List<Mutation.FunctionMutation>();
            this.KeepLinkOnAddNode = 0.3;
            this.MutateWeights = 0.7;
            this.Recurrence = false;
            this.WeightProbabilities = new List<Mutation.WeightMutation>();
        }

        public static bool SaveToXml(string fileName, Mutation pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                Mutation.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static Mutation LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return Mutation.Serial.Deserialize((TextReader)streamReader) as Mutation;
        }

        [Serializable]
        public struct WeightMutation
        {
            [XmlAttribute]
            public bool Normal;
            [XmlAttribute]
            public bool Delta;
            [XmlAttribute]
            public bool Proportional;
            [XmlAttribute]
            public double Amount;
            [XmlAttribute]
            public double Probability;
        }

        [Serializable]
        public struct FunctionMutation
        {
            [XmlAttribute]
            public string Function;
            [XmlAttribute]
            public double Probability;
        }
    }
}
