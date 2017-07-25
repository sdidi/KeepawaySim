
using System;
using System.IO;
using System.Xml.Serialization;

namespace Keepaway.Parameters
{
    [Serializable]
    public class Speciation
    {
        private static XmlSerializer Serial { get; set; }

        public double Weight { get; set; }

        public double Disjoint { get; set; }

        public double Function { get; set; }

        public double InitialThreshold { get; set; }

        public int SpeciesCount { get; set; }

        public int MarkingAge { get; set; }

        public int RepresentitiveSize { get; set; }

        internal double Threshold { get; set; }

        static Speciation()
        {
            Speciation.Serial = new XmlSerializer(typeof(Speciation));
        }

        public Speciation()
        {
            this.Disjoint = 1.0;
            this.Function = 1.0;
            this.InitialThreshold = 100.0;
            this.Threshold = 100.0;
            this.Weight = 0.2;
            this.MarkingAge = 5;
            this.RepresentitiveSize = 1;
        }

        public static bool SaveToXml(string fileName, Speciation pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                Speciation.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static Speciation LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return Speciation.Serial.Deserialize((TextReader)streamReader) as Speciation;
        }
    }
}
