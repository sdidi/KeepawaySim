using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Keepaway
{
    class ReadChamps
    {
        const string Champs = "Champs.csv";
        static void Main(string[] args)
        {
            double genotypic_diversity = 0.0, realfitness = 0.0, novelty = 0.0;
            int generation=0;
            XDocument docu;
            int numRuns = 20;
            for (int j = 1; j < numRuns + 1; j++)
            {
                int i = 1;
                while (i < 51)
                {
                    if (i < 10)
                        docu = XDocument.Load("Run"+j+"\\Champs\\G000" + i + "Champ.xml");
                    else
                        docu = XDocument.Load("Run"+j+"\\Champs\\G00" + i + "Champ.xml");
                    var gen = docu.Root.Attribute("Generation").Value;
                    var nov = docu.Root.Attribute("Novelty").Value;
                    var g_diversity = docu.Root.Attribute("Genotypic_Diversity").Value;
                    var rfitness = docu.Root.Attribute("RealFitness").Value;
                    generation = int.Parse(gen);
                    novelty = Double.Parse(nov);
                    genotypic_diversity = Double.Parse(g_diversity);
                    realfitness = Double.Parse(rfitness);
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(Champs, true))
                    {
                        file.WriteLine(string.Format("{0} , {1:f3}, {2:f3}, {3:f3} ", generation, novelty, realfitness, genotypic_diversity));
                    }
                    i++;
                }
            }
            Console.ReadLine();
        }


    }
}
