using System;
using System.IO;
using System.Xml.Serialization;
namespace Keepaway.Parameters
{
    [Serializable]
    public class HyperNEAT
    {
        private static XmlSerializer Serial { get; set; }

        public bool Normalize { get; set; }

        public double StaticThreshold { get; set; }

        public bool ThresholdByDistance { get; set; }

        public double WeightRange { get; set; }

        static HyperNEAT()
        {
            HyperNEAT.Serial = new XmlSerializer(typeof(HyperNEAT));
        }

        public HyperNEAT()
        {
            this.Normalize = true;
            this.StaticThreshold = 0.2;
            this.ThresholdByDistance = true;
            this.WeightRange = 10.0;
        }

        public static bool SaveToXml(string fileName, HyperNEAT pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                HyperNEAT.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static HyperNEAT LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return HyperNEAT.Serial.Deserialize((TextReader)streamReader) as HyperNEAT;
        }
    }
}
