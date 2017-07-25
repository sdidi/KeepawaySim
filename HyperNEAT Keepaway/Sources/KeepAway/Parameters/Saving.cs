
using System;
using System.IO;
using System.Xml.Serialization;

namespace Keepaway.Parameters
{
    [XmlInclude(typeof(Saving.Save))]
    [Serializable]
    public class Saving
    {
        public Saving.Save Champions;
        public Saving.Save Populations;
        public Saving.Save Species;
        public Saving.Save ChampionStatistics;
        public Saving.Save PopulationStatistics;
        public Saving.Save SpeciesStatistics;

        private static XmlSerializer Serial { get; set; }

        static Saving()
        {
            Saving.Serial = new XmlSerializer(typeof(Saving));
        }

        public Saving()
        {
            this.Champions.save = true;
            this.Champions.path = "Champs" + (object)Path.DirectorySeparatorChar;
            this.Champions.inteval = 1;
            this.ChampionStatistics.save = true;
            this.ChampionStatistics.path = "Champs" + (object)Path.DirectorySeparatorChar;
            this.ChampionStatistics.inteval = 1;
            this.Populations.save = true;
            this.Populations.path = "Populations" + (object)Path.DirectorySeparatorChar;
            this.Populations.inteval = 10;
            this.PopulationStatistics.save = true;
            this.PopulationStatistics.path = "Populations" + (object)Path.DirectorySeparatorChar;
            this.PopulationStatistics.inteval = 1;
            this.Species.save = false;
            this.Species.path = "Species" + (object)Path.DirectorySeparatorChar;
            this.Species.inteval = 10;
            this.SpeciesStatistics.save = true;
            this.SpeciesStatistics.path = "Species" + (object)Path.DirectorySeparatorChar;
            this.SpeciesStatistics.inteval = 1;
        }

        public static bool SaveToXml(string fileName, Saving pop)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                Saving.Serial.Serialize((TextWriter)streamWriter, (object)pop);
                return true;
            }
        }

        public static Saving LoadFromXml(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName))
                return Saving.Serial.Deserialize((TextReader)streamReader) as Saving;
        }

        [Serializable]
        public struct Save
        {
            [XmlAttribute]
            public bool save;
            [XmlAttribute]
            public string path;
            [XmlAttribute]
            public int inteval;
        }
    }
}
