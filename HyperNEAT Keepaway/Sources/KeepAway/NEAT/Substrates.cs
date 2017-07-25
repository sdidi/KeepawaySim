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
using NEAT.Networks;
using System;
using System.Collections.Generic;
using Keepaway.Parameters;

namespace Keepaway
{
    public delegate void MapWeights(NetworkGenome sub, DecodedNetworks CPPN);
    public delegate List<NodeGene> NodeGen();

    public delegate void LinkGen(NetworkGenome genome);

    public class Substrates
    {
        private DecodedNetworks net = new DecodedNetworks();
        private NetworkGenome sub;

        internal static HyperNEAT param { get; set; }

        static Substrates()
        {
            Substrates.param = new HyperNEAT();
        }

        public Substrates()
        {
            this.sub = new NetworkGenome();
        }

        public void GenerateNodes(NodeGen nodeGen)
        {
            this.sub.Nodes.AddRange((IEnumerable<NodeGene>)nodeGen());
        }

        public void GenerateLinks(LinkGen linkGen)
        {
            linkGen(this.sub);
            this.net = DecodedNetworks.DecodeGenome(this.sub);
        }

        public DecodedNetworks GenerateNetwork(MapWeights create, DecodedNetworks cppn)
        {
            for (int index = 0; index < this.net.ActivatingNodes.Count; ++index)
                this.net.ActivatingNodes[index].links.Clear();
            create(this.sub, cppn);
            double val1 = 0.0;
            foreach (LinkGene linkGene in this.sub.Links)
                val1 = Math.Max(val1, Math.Abs(linkGene.Weight));
            List<LinkGene> list = new List<LinkGene>();
            double staticThreshold1 = Substrates.param.StaticThreshold;
            DecodedNetworks.Link link1;
            using (List<LinkGene>.Enumerator enumerator = this.sub.Links.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    LinkGene link = enumerator.Current;
                    link.Weight *= Substrates.param.WeightRange / (2.0 * val1);
                    double staticThreshold2 = Substrates.param.StaticThreshold;
                    if (Substrates.param.ThresholdByDistance)
                    {
                        SubstrateNodes substrateNode1 = this.sub.Nodes.Find((Predicate<NodeGene>)(n => n.Id == link.Source)) as SubstrateNodes;
                        SubstrateNodes substrateNode2 = this.sub.Nodes.Find((Predicate<NodeGene>)(n => n.Id == link.Target)) as SubstrateNodes;
                        double d = 0.0;
                        for (int index = 0; index < substrateNode1.Coordinate.Count; ++index)
                        {
                            double num = substrateNode1.Coordinate[index] - substrateNode2.Coordinate[index];
                            d += num * num;
                        }
                        double num1 = Math.Sqrt(d);
                        staticThreshold2 += num1 * Substrates.param.StaticThreshold;
                    }
                    if (Math.Abs(link.Weight) >= staticThreshold2)
                    {
                        link1.sourceNode = link.Source;
                        link1.weight = link.Weight;
                        this.net.Neurons[link.Target].links.Add(link1);
                    }
                }
            }
            for (int index = 0; index < this.net.ActivatingNodes.Count; ++index)
            {
                link1.sourceNode = -1;
                link1.weight = 0.0;
                this.net.ActivatingNodes[index].links.Add(link1);
            }
            if (Substrates.param.Normalize)
            {
                double num1 = 1.0;
                for (int index1 = 0; index1 < this.net.ActivatingNodes.Count; ++index1)
                {
                    double d = 0.0;
                    for (int index2 = 0; index2 < this.net.ActivatingNodes[index1].links.Count - 1; ++index2)
                        d += this.net.ActivatingNodes[index1].links[index2].weight * this.net.ActivatingNodes[index1].links[index2].weight;
                    num1 = Math.Max(num1, Math.Sqrt(d));
                }
                double num2 = Math.Sqrt(num1);
                for (int index1 = 0; index1 < this.net.ActivatingNodes.Count; ++index1)
                {
                    for (int index2 = 0; index2 < this.net.ActivatingNodes[index1].links.Count - 1; ++index2)
                    {
                        DecodedNetworks.Link link2;
                        link2.sourceNode = this.net.ActivatingNodes[index1].links[index2].sourceNode;
                        link2.weight = this.net.ActivatingNodes[index1].links[index2].weight / num2;
                        this.net.ActivatingNodes[index1].links[index2] = link2;
                    }
                }
            }
            return this.net;
        }
    }
}
