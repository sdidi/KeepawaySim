using System;
using System.IO;
using System.Xml.Serialization;

namespace Keepaway.Parameters
{
    [Serializable]
    public class ParameterFiles
    {
        private static XmlSerializer Serial { get; set; }

        public string Initialization { get; set; }

        public string Mutation { get; set; }

        public string Reproduction { get; set; }

        public string Saving { get; set; }

        public string Speciation { get; set; }

        public string Testing { get; set; }

        public string HyperNEAT { get; set; }

        static ParameterFiles()
        {
            ParameterFiles.Serial = new XmlSerializer(typeof(ParameterFiles));
        }

        public ParameterFiles()
        {
            this.Initialization = "Initialization.xml";
            this.Mutation = "Mutation.xml";
            this.Reproduction = "Reproduction.xml";
            this.Saving = "Saving.xml";
            this.Speciation = "Speciation.xml";
            this.Testing = "Testing.xml";
            this.HyperNEAT = "HyperNEAT.xml";
        }

        public static bool SaveToXml(string fileName, ParameterFiles pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                ParameterFiles.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static ParameterFiles LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return ParameterFiles.Serial.Deserialize((TextReader)streamReader) as ParameterFiles;
        }
    }
}
