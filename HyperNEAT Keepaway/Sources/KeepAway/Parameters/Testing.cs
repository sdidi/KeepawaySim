using System;
using System.IO;
using System.Xml.Serialization;

namespace Keepaway.Parameters
{
    [XmlInclude(typeof(Testing.Test))]
    [Serializable]
    public class Testing
    {
        public Testing.Test Genome;
        public Testing.Test Directory;
        public Testing.Test ChampionsAtEnd;

        private static XmlSerializer Serial { get; set; }

        static Testing()
        {
            Testing.Serial = new XmlSerializer(typeof(Testing));
        }

        public Testing()
        {
            this.Genome.test = false;
            this.Genome.path = "Genome.xml";
            this.Directory.test = false;
            this.Directory.path = "Champs" + (object)Path.DirectorySeparatorChar;
            this.ChampionsAtEnd.test = false;
            this.ChampionsAtEnd.path = "Champs" + (object)Path.DirectorySeparatorChar;
        }

        public static bool SaveToXml(string fileName, Testing pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                Testing.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static Testing LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return Testing.Serial.Deserialize((TextReader)streamReader) as Testing;
        }

        [Serializable]
        public struct Test
        {
            [XmlAttribute]
            public bool test;
            [XmlAttribute]
            public string path;
        }
    }
}
