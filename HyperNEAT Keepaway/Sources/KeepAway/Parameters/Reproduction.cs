
using System;
using System.IO;
using System.Xml.Serialization;

namespace Keepaway.Parameters
{
    [Serializable]
    public class Reproduction
    {
        private static XmlSerializer Serial { get; set; }

        public double Elitism { get; set; }

        public double AsexualProbability { get; set; }

        public double SelectFitterSpecies { get; set; }

        public double SelectFitterGenome { get; set; }

        public double SelectFitterGene { get; set; }

        public double RecombineExcessFromLessFit { get; set; }

        public int SpeciesStagnation { get; set; }

        static Reproduction()
        {
            Reproduction.Serial = new XmlSerializer(typeof(Reproduction));
        }

        public Reproduction()
        {
            this.AsexualProbability = 0.75;
            this.Elitism = 0.05;
            this.RecombineExcessFromLessFit = 0.1;
            this.SelectFitterGene = 0.75;
            this.SelectFitterGenome = 0.8;
            this.SelectFitterSpecies = 0.7;
            this.SpeciesStagnation = 25;
        }

        public static bool SaveToXml(string fileName, Reproduction pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                Reproduction.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static Reproduction LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return Reproduction.Serial.Deserialize((TextReader)streamReader) as Reproduction;
        }
    }
}
