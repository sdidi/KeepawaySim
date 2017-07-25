
/***************************************************************************
                                   FixedKeepawayPlayer.cs
                    Fixed policy kepaway player for Keepaway domain experiments.
                             -------------------
    begin                : JUL-2009
    credit               : Phillip Verbancsics of the
                            Evolutionary Complexity Research Group
     email                : verb@cs.ucf.edu

  
 Based On: 
/*

Copyright (c) 2004 Gregory Kuhlmann, Peter Stone
University of Texas at Austin
All rights reserved.
 
 Base On:

Copyright (c) 2000-2003, Jelle Kok, University of Amsterdam
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

3. Neither the name of the University of Amsterdam nor the names of its
contributors may be used to endorse or promote products derived from this
software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
 ***************************************************************************/

using NEAT.Genetics;
using NEAT.HyperNEAT;
using NEAT.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Keepaway
{
    public class Evaluator
    {
        public static Substrates sub;

        public static void Main()
        {
            int num = 0;
            for (int index = 0; index < 50; ++index)
            {
                if (Evaluator.Test())
                    ++num;
            }
            Console.WriteLine(num);
        }

        public static bool Test()
        {
            Evolutions evolution = new Evolutions();
            evolution.SetParameters("Parameters.xml");
            evolution.Evaluator = new EvaluateGenome(Evaluator.NormalXor);
            evolution.Initialise();
            evolution.AllGenerations();
            return Enumerable.Last<NetworkGenome>((IEnumerable<NetworkGenome>)evolution.Champions).Fitness == 4.0;
        }

        public static List<NodeGene> XorNode()
        {
            List<NodeGene> list = new List<NodeGene>();
            SubstrateNodes substrateNode1 = new SubstrateNodes();
            substrateNode1.Function = "Identity";
            substrateNode1.Id = 1;
            substrateNode1.Layer = 0;
            substrateNode1.Type = NodeType.Input;
            substrateNode1.SetCoordinate(-1.0, -1.0);
            list.Add((NodeGene)substrateNode1);
            SubstrateNodes substrateNode2 = new SubstrateNodes();
            substrateNode2.Function = "Identity";
            substrateNode2.Id = 2;
            substrateNode2.Layer = 0;
            substrateNode2.Type = NodeType.Input;
            substrateNode2.SetCoordinate(1.0, -1.0);
            list.Add((NodeGene)substrateNode2);
            SubstrateNodes substrateNode3 = new SubstrateNodes();
            substrateNode3.Function = "Identity";
            substrateNode3.Id = 3;
            substrateNode3.Layer = 0;
            substrateNode3.Type = NodeType.Input;
            substrateNode3.SetCoordinate(-1.0, 1.0);
            list.Add((NodeGene)substrateNode3);
            SubstrateNodes substrateNode4 = new SubstrateNodes();
            substrateNode4.Function = "Identity";
            substrateNode4.Id = 4;
            substrateNode4.Layer = 0;
            substrateNode4.Type = NodeType.Input;
            substrateNode4.SetCoordinate(1.0, 1.0);
            list.Add((NodeGene)substrateNode4);
            SubstrateNodes substrateNode5 = new SubstrateNodes();
            substrateNode5.Function = "BipolarSigmoid";
            substrateNode5.Id = 5;
            substrateNode5.Layer = 1;
            substrateNode5.Type = NodeType.Output;
            substrateNode5.SetCoordinate(new double[2]);
            list.Add((NodeGene)substrateNode5);
            return list;
        }

        public static bool HyperXor(List<NetworkGenome> genomes, bool test)
        {
            if (Evaluator.sub == null)
            {
                Evaluator.sub = new Substrates();
                Evaluator.sub.GenerateNodes(new NodeGen(Evaluator.XorNode));
            }
            bool flag = false;
            for (int index = 0; index < genomes.Count; ++index)
            {
                Evaluator.subXor(genomes[index]);
                flag = flag || genomes[index].Fitness == 4.0;
            }
            return flag;
        }

        public static void XorCreate(NetworkGenome sub, DecodedNetworks cppn)
        {
            SubstrateNodes substrateNode1 = sub.Nodes.Find((Predicate<NodeGene>)(n => n.Type == NodeType.Output)) as SubstrateNodes;
            double[] inputs = new double[4];
            int id = 0;
            for (int index = 0; index < sub.Nodes.Count - 1; ++index)
            {
                SubstrateNodes substrateNode2 = sub.Nodes[index] as SubstrateNodes;
                cppn.Flush();
                inputs[0] = substrateNode2.Coordinate[0];
                inputs[1] = substrateNode2.Coordinate[1];
                inputs[2] = substrateNode1.Coordinate[0];
                inputs[3] = substrateNode1.Coordinate[1];
                cppn.SetInputs(inputs);
                cppn.ActivateNetwork(5);
                double[] outputs = cppn.GetOutputs();
                sub.Links.Add(new LinkGene(id, substrateNode2.Id, substrateNode1.Id, outputs[0]));
            }
        }

        public static void subXor(NetworkGenome cppn)
        {
            DecodedNetworks decodedNetwork = (DecodedNetworks)null;
            double[] inputs = new double[4];
            bool flag1 = true;
            inputs[0] = 1.0;
            inputs[1] = 0.0;
            inputs[2] = 0.0;
            inputs[3] = 0.0;
            decodedNetwork.SetInputs(inputs);
            decodedNetwork.ActivateNetwork(2);
            double[] outputs1 = decodedNetwork.GetOutputs();
            cppn.Fitness = 0.0;
            cppn.Fitness += Math.Min(1.0, Math.Max(0.0, 1.0 - outputs1[0]));
            bool flag2 = flag1 && outputs1[0] < 0.5;
            decodedNetwork.Flush();
            inputs[0] = 0.0;
            inputs[1] = 0.0;
            inputs[2] = 0.0;
            inputs[3] = 1.0;
            decodedNetwork.SetInputs(inputs);
            decodedNetwork.ActivateNetwork(2);
            double[] outputs2 = decodedNetwork.GetOutputs();
            cppn.Fitness += Math.Min(1.0, Math.Max(0.0, 1.0 - outputs2[0]));
            bool flag3 = flag2 && outputs2[0] < 0.5;
            decodedNetwork.Flush();
            inputs[0] = 0.0;
            inputs[1] = 1.0;
            inputs[2] = 0.0;
            inputs[3] = 0.0;
            decodedNetwork.SetInputs(inputs);
            decodedNetwork.ActivateNetwork(2);
            double[] outputs3 = decodedNetwork.GetOutputs();
            cppn.Fitness += Math.Min(1.0, Math.Max(0.0, outputs3[0]));
            bool flag4 = flag3 && outputs3[0] >= 0.5;
            decodedNetwork.Flush();
            inputs[0] = 0.0;
            inputs[1] = 0.0;
            inputs[2] = 1.0;
            inputs[3] = 0.0;
            decodedNetwork.SetInputs(inputs);
            decodedNetwork.ActivateNetwork(2);
            double[] outputs4 = decodedNetwork.GetOutputs();
            cppn.Fitness += Math.Min(1.0, Math.Max(0.0, outputs4[0]));
            bool flag5 = flag4 && outputs4[0] >= 0.5;
            decodedNetwork.Flush();
            if (!flag5)
                return;
            cppn.Fitness = 4.0;
        }

        public static bool ThreadedXor(List<NetworkGenome> genomes)
        {
            Semaphore semaphore = new Semaphore(Environment.ProcessorCount, Environment.ProcessorCount);
            for (int index = 0; index < genomes.Count; ++index)
            {
                semaphore.WaitOne();
                ThreadPool.QueueUserWorkItem(new WaitCallback(Evaluator.Xor), (object)genomes[index]);
            }
            for (int index = 0; index < genomes.Count; ++index)
            {
                if (genomes[index].Fitness == 4.0)
                    return true;
            }
            return false;
        }

        public static void Xor(object target)
        {
            NetworkGenome genome = target as NetworkGenome;
            double[] inputs = new double[2];
            bool flag1 = true;
            DecodedNetworks decodedNetwork = DecodedNetworks.DecodeGenome(genome);
            inputs[0] = 0.0;
            inputs[1] = 0.0;
            decodedNetwork.SetInputs(inputs);
            decodedNetwork.ActivateNetwork(6);
            double[] outputs1 = decodedNetwork.GetOutputs();
            genome.Fitness = 0.0;
            genome.Fitness += Math.Min(1.0, Math.Max(0.0, 1.0 - outputs1[0]));
            bool flag2 = flag1 && outputs1[0] < 0.5;
            decodedNetwork.Flush();
            inputs[0] = 1.0;
            inputs[1] = 1.0;
            decodedNetwork.SetInputs(inputs);
            decodedNetwork.ActivateNetwork(6);
            double[] outputs2 = decodedNetwork.GetOutputs();
            genome.Fitness += Math.Min(1.0, Math.Max(0.0, 1.0 - outputs2[0]));
            bool flag3 = flag2 && outputs2[0] < 0.5;
            decodedNetwork.Flush();
            inputs[0] = 0.0;
            inputs[1] = 1.0;
            decodedNetwork.SetInputs(inputs);
            decodedNetwork.ActivateNetwork(6);
            double[] outputs3 = decodedNetwork.GetOutputs();
            genome.Fitness += Math.Min(1.0, Math.Max(0.0, outputs3[0]));
            bool flag4 = flag3 && outputs3[0] >= 0.5;
            decodedNetwork.Flush();
            inputs[0] = 1.0;
            inputs[1] = 0.0;
            decodedNetwork.SetInputs(inputs);
            decodedNetwork.ActivateNetwork(6);
            double[] outputs4 = decodedNetwork.GetOutputs();
            genome.Fitness += Math.Min(1.0, Math.Max(0.0, outputs4[0]));
            bool flag5 = flag4 && outputs4[0] >= 0.5;
            decodedNetwork.Flush();
            if (!flag5)
                return;
            genome.Fitness = 4.0;
        }

        public static bool NormalXor(List<NetworkGenome> genomes, bool test)
        {
            double[] inputs = new double[2];
            bool flag1 = false;
            foreach (NetworkGenome genome in genomes)
            {
                bool flag2 = true;
                DecodedNetworks decodedNetwork = DecodedNetworks.DecodeGenome(genome);
                inputs[0] = 0.0;
                inputs[1] = 0.0;
                decodedNetwork.SetInputs(inputs);
                decodedNetwork.ActivateNetwork(6);
                double[] outputs1 = decodedNetwork.GetOutputs();
                genome.Fitness = 0.0;
                genome.Fitness += Math.Min(1.0, Math.Max(0.0, 1.0 - outputs1[0]));
                bool flag3 = flag2 && outputs1[0] < 0.5;
                decodedNetwork.Flush();
                inputs[0] = 1.0;
                inputs[1] = 1.0;
                decodedNetwork.SetInputs(inputs);
                decodedNetwork.ActivateNetwork(6);
                double[] outputs2 = decodedNetwork.GetOutputs();
                genome.Fitness += Math.Min(1.0, Math.Max(0.0, 1.0 - outputs2[0]));
                bool flag4 = flag3 && outputs2[0] < 0.5;
                decodedNetwork.Flush();
                inputs[0] = 0.0;
                inputs[1] = 1.0;
                decodedNetwork.SetInputs(inputs);
                decodedNetwork.ActivateNetwork(6);
                double[] outputs3 = decodedNetwork.GetOutputs();
                genome.Fitness += Math.Min(1.0, Math.Max(0.0, outputs3[0]));
                bool flag5 = flag4 && outputs3[0] >= 0.5;
                decodedNetwork.Flush();
                inputs[0] = 1.0;
                inputs[1] = 0.0;
                decodedNetwork.SetInputs(inputs);
                decodedNetwork.ActivateNetwork(6);
                double[] outputs4 = decodedNetwork.GetOutputs();
                genome.Fitness += Math.Min(1.0, Math.Max(0.0, outputs4[0]));
                bool flag6 = flag5 && outputs4[0] >= 0.5;
                decodedNetwork.Flush();
                if (flag6)
                    genome.Fitness = 4.0;
                flag1 = flag1 || flag6;
            }
            return flag1;
        }
    }
}
