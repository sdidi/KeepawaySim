using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Keepaway
{
    class ReadPopulations
    {
        const string Population = "Population.csv";
       
        static void Main(string[] args)
        {
            int generation = 0;
            double fitness = 0.0, genotypic_diversity = 0.0, novelty = 0.0, realfitness=0.0, maxfitness = 0.0;
            XDocument docu;
            int numRuns = 20;
            for (int j = 1; j < numRuns+1; j++)
            {
                int i = 1;
                while (i < 51)
                {
                    if (i < 10)
                        docu = XDocument.Load("Run"+j+"\\Populations\\G000" + i + "Pop.xml");
                    else
                        docu = XDocument.Load("Run"+j+"\\Populations\\G00" + i + "Pop.xml");
                    var gen = docu.Root.Attribute("Generation").Value;
                    var fit = docu.Root.Attribute("MeanFitness").Value;
                    var g_diversity = docu.Root.Attribute("MeanDiversity").Value;
                    var nov = docu.Root.Attribute("MeanNovelty").Value;
                    var rfitness = docu.Root.Attribute("MeanRealFitness").Value;
                    var maxfit = docu.Root.Attribute("MaxFitness").Value;
                    generation = int.Parse(gen);
                    fitness = Double.Parse(fit);
                    genotypic_diversity = Double.Parse(g_diversity);
                    novelty = Double.Parse(nov);
                    realfitness = Double.Parse(rfitness);
                    maxfitness = Double.Parse(maxfit);
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(Population, true))
                    {
                        file.WriteLine(string.Format("{0} , {1:f3}, {2:f3}, {3:f3}, {4:f3}, {5:f3} ", generation, fitness, genotypic_diversity, novelty, maxfitness));
                    }
                    i++;
                }
            }
            Console.ReadLine();

        }  


    }
}
