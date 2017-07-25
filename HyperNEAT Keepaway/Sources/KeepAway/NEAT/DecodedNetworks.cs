
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEAT.Networks;
namespace Keepaway
{
    public class DecodedNetworks : ICloneable
    {
        private double[] OutputArray;
        internal List<DecodedNetworks.Node> Neurons;
        private List<DecodedNetworks.Node> Outputs;
        private List<DecodedNetworks.Node> Inputs;
        internal List<DecodedNetworks.Node> ActivatingNodes;

        public DecodedNetworks()
        {
            this.Outputs = new List<DecodedNetworks.Node>();
            this.Neurons = new List<DecodedNetworks.Node>();
            this.Inputs = new List<DecodedNetworks.Node>();
            this.OutputArray = (double[])null;
            this.ActivatingNodes = new List<DecodedNetworks.Node>();
        }

        public static DecodedNetworks DecodeGenome(NetworkGenome genome)
        {
            DecodedNetworks decodedNetwork = new DecodedNetworks();
            NetworkGenome NetworkGenome = genome.Clone() as NetworkGenome;
            NetworkGenome.Nodes.Sort();
            for (int index = 0; index < NetworkGenome.Nodes.Count; ++index)
            {
                if (NetworkGenome.Nodes[index].Id != index)
                {
                    foreach (LinkGene LinkGenes in NetworkGenome.Links)
                    {
                        if (LinkGenes.Source == NetworkGenome.Nodes[index].Id)
                            LinkGenes.Source = index;
                        if (LinkGenes.Target == NetworkGenome.Nodes[index].Id)
                            LinkGenes.Target = index;
                    }
                }
                NetworkGenome.Nodes[index].Id = index;
            }
            foreach (NodeGene NodeGenes in NetworkGenome.Nodes)
            {
                DecodedNetworks.Node node = new DecodedNetworks.Node();
                node.activation = ActivationFunctions.GetFunction(NodeGenes.Function);
                foreach (LinkGene LinkGenes in NetworkGenome.Links)
                {
                    if (LinkGenes.Target == NodeGenes.Id)
                        node.links.Add(new DecodedNetworks.Link()
                        {
                            sourceNode = LinkGenes.Source,
                            weight = LinkGenes.Weight
                        });
                }
                if (NodeGenes.Type == NodeType.Bias)
                {
                    node.signal = 1.0;
                    decodedNetwork.Neurons.Add(node);
                }
                else if (NodeGenes.Type == NodeType.Input)
                {
                    decodedNetwork.Neurons.Add(node);
                    decodedNetwork.Inputs.Add(node);
                }
                else if (NodeGenes.Type == NodeType.Hidden)
                {
                    decodedNetwork.Neurons.Add(node);
                    decodedNetwork.ActivatingNodes.Add(node);
                }
                else
                {
                    decodedNetwork.Neurons.Add(node);
                    decodedNetwork.ActivatingNodes.Add(node);
                    decodedNetwork.Outputs.Add(node);
                }
            }
            decodedNetwork.OutputArray = new double[decodedNetwork.Outputs.Count];
            return decodedNetwork;
        }

        public void Flush()
        {
            for (int index = 0; index < this.ActivatingNodes.Count; ++index)
                this.ActivatingNodes[index].signal = 0.0;
            for (int index = 0; index < this.Inputs.Count; ++index)
                this.Inputs[index].signal = 0.0;
        }

        public double[] GetOutputs()
        {
            for (int index = 0; index < this.Outputs.Count; ++index)
                this.OutputArray[index] = this.Outputs[index].signal;
            return this.OutputArray;
        }

        public void SetInputs(double[] inputs)
        {
            for (int index = 0; index < Math.Min(inputs.Length, this.Inputs.Count); ++index)
                this.Inputs[index].signal = inputs[index];
        }

        public void ActivateNetwork(int numActivations)
        {
            for (int index1 = 0; index1 < numActivations; ++index1)
            {
                for (int index2 = 0; index2 < this.ActivatingNodes.Count; ++index2)
                {
                    this.ActivatingNodes[index2].tempSignal = 0.0;
                    for (int index3 = 0; index3 < this.ActivatingNodes[index2].links.Count && this.ActivatingNodes[index2].links[index3].sourceNode != -1; ++index3)
                        this.ActivatingNodes[index2].tempSignal += this.Neurons[this.ActivatingNodes[index2].links[index3].sourceNode].signal * this.ActivatingNodes[index2].links[index3].weight;
                }
                for (int index2 = 0; index2 < this.ActivatingNodes.Count; ++index2)
                    this.ActivatingNodes[index2].signal = this.ActivatingNodes[index2].activation(this.ActivatingNodes[index2].tempSignal);
            }
        }

        public object Clone()
        {
            DecodedNetworks decodedNetwork = new DecodedNetworks();
            decodedNetwork.Inputs = new List<DecodedNetworks.Node>(this.Inputs.Count);
            decodedNetwork.Neurons = new List<DecodedNetworks.Node>(this.Neurons.Count);
            decodedNetwork.ActivatingNodes = new List<DecodedNetworks.Node>(this.ActivatingNodes.Count);
            decodedNetwork.OutputArray = new double[this.OutputArray.Length];
            decodedNetwork.Outputs = new List<DecodedNetworks.Node>(this.Outputs.Count);
            for (int index1 = 0; index1 < this.Neurons.Count; ++index1)
            {
                DecodedNetworks.Node node = new DecodedNetworks.Node();
                node.activation = this.Neurons[index1].activation;
                node.links = new List<DecodedNetworks.Link>(this.Neurons[index1].links.Count);
                for (int index2 = 0; index2 < this.Neurons[index1].links.Count; ++index2)
                    node.links.Add(this.Neurons[index1].links[index2]);
                node.signal = 0.0;
                node.tempSignal = 0.0;
                decodedNetwork.Neurons.Add(node);
                if (this.Inputs.Contains(this.Neurons[index1]))
                    decodedNetwork.Inputs.Add(node);
                else if (this.Outputs.Contains(this.Neurons[index1]))
                {
                    decodedNetwork.Outputs.Add(node);
                    decodedNetwork.ActivatingNodes.Add(node);
                }
                else if (this.ActivatingNodes.Contains(this.Neurons[index1]))
                    decodedNetwork.ActivatingNodes.Add(node);
            }
            return (object)decodedNetwork;
        }

        public void CopyLinks(DecodedNetworks net)
        {
            DecodedNetworks.Link link = new DecodedNetworks.Link();
            link.sourceNode = -1;
            link.weight = 0.0;
            for (int index1 = 0; index1 < this.ActivatingNodes.Count && index1 < net.ActivatingNodes.Count; ++index1)
            {
                this.ActivatingNodes[index1].links.Clear();
                for (int index2 = 0; index2 < net.ActivatingNodes[index1].links.Count && net.ActivatingNodes[index1].links[index2].sourceNode != -1; ++index2)
                    this.ActivatingNodes[index1].links.Add(net.ActivatingNodes[index1].links[index2]);
                this.ActivatingNodes[index1].links.Add(link);
            }
        }

        public class Node
        {
            public double signal;
            public double tempSignal;
            public Function activation;
            public List<DecodedNetworks.Link> links;

            public Node()
            {
                this.signal = 0.0;
                this.tempSignal = 0.0;
                this.activation = ActivationFunctions.GetFunction("Identity");
                this.links = new List<DecodedNetworks.Link>();
            }
        }

        public struct Link
        {
            public int sourceNode;
            public double weight;
        }
    }
}
