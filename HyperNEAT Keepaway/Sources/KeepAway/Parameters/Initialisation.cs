using System;
using System.IO;
using System.Xml.Serialization;

namespace Keepaway.Parameters
{
    [XmlInclude(typeof(Initialization.Load))]
    [Serializable]
    public class Initialization
    {
        public Initialization.Load Population;
        public Initialization.Load Seed;

        private static XmlSerializer Serial { get; set; }

        public int MaximumGenerations { get; set; }

        public bool StopOnFitnessCriteria { get; set; }

        public int PopulationSize { get; set; }

        public double InitalConnectionPercentage { get; set; }

        public int Inputs { get; set; }

        public int Outputs { get; set; }

        public int MaxLayers { get; set; }

        static Initialization()
        {
            Initialization.Serial = new XmlSerializer(typeof(Initialization));
        }

        public Initialization()
        {
            this.MaximumGenerations = 1000;
            this.PopulationSize = 200;
            this.InitalConnectionPercentage = 1.0;
            this.Inputs = 2;
            this.Outputs = 1;
            this.Population.load = false;
            this.Population.file = "Population.xml";
            this.Seed.load = false;
            this.Seed.file = "Seed.xml";
            this.MaxLayers = 5;
        }

        public static bool SaveToXml(string fileName, Initialization pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                Initialization.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static Initialization LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return Initialization.Serial.Deserialize((TextReader)streamReader) as Initialization;
        }

        [Serializable]
        public struct Load
        {
            [XmlAttribute]
            public bool load;
            [XmlAttribute]
            public string file;
        }
    }
}
