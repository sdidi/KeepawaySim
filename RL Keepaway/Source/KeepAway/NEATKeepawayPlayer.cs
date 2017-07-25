

/*******************************************NEATKeepawayPlayer*******************************
Adapted from HyperNEAT Keepaway Player by Phillip Verbancsics to suit Keepaway NEAT player for an 
 our experiments 
 * 
Based On:
Copyright (c) 2010 Phillip Verbancsics

Based On: 
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
using System.Xml;
using RoboCup.Geometry;
using SharpNeat.Phenomes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Keepaway
{

   public class NEATKeepawayPlayer : FixedKeepawayPlayer
    {

        #region Variables
        int lastAction = 0;
        int lastActionTime = 0;
        double[] inputs = new double[13];
        int max = 0;
        Dictionary<int, int> layerToIndex = new Dictionary<int, int>();
        #endregion

        public IBlackBox Brain { get; set; }

        int numK, numT; RoboCup.Stadium std; string team, side; int unum;
        #region Constructor
        public NEATKeepawayPlayer(RoboCup.Stadium std, string team, int unum, string side, int numK, int numT, IBlackBox box)
            : base(std, team, unum, side, numK, numT)
        {
            this.numK = numK;
            this.numT = numT;
            this.std = std;
            this.team = team;
            this.side = side;
            this.unum = unum;
            this.Brain = box; // to pass the brain controller to an agent with ball
        }
        
        public NEATKeepawayPlayer(RoboCup.Stadium std, string team, int unum, string side, int numK, int numT)
            : base(std, team, unum, side, numK, numT)
        {
            this.numK = numK;
            this.numT = numT;
            this.std = std;
            this.team = team;
            this.side = side;
            this.unum = unum;
        }
        
        #endregion

        private void setInputIsignalArray(ISignalArray inputArr, double[] inp)
        {
            for (int i = 0; i < inp.Length; i++)
            {
                inputArr[i] = inp[i];
            }

        }



        /*
         the method maintains the network input-output structure, replaces teammatesp with tmatesp and opp with oppPlayersp  
         */
        #region Mapping helper method
        public Vector[] ClosestTwoPlayers(Vector[] vector, int num)
       {

           Vector[] playerListp = new Vector[num];
           for (int i = 0; i < num; i++)
           {
               playerListp[i] = new Vector(0, 0);
           }
           double close = 1000; int val = 0, temp = 0; ;
           for (int j = 0; j < 2; j++)
           {
               for (int i = 0; i < vector.Length; i++)
               {
                   if (i != Unum - 1)
                   {
                       if (j == 1 && i == temp) continue;
                       if (vector[i].distance(mySelfp) < close)
                       {
                           close = vector[i].distance(mySelfp);
                           val = i;
                       }
                   }
               }
               temp = val;
               playerListp[j] = vector[val];
           }
           return playerListp;
       }
        #endregion 

        #region NEAT controlled Keeper With Ball action
        public override void keeperWithBall()
        {
            Vector[] tmatesp = null;
            Vector[] oppPlayersp = null;
            Vector posToPass = new Vector(0, 0);
            Command soc = new Command();
            soc.Com = "illegal";
            int action = 0, j = 0, h = 0, v =0, g = 0; double ang1, ang2, ang3;
            tmatesp = ClosestTwoPlayers(teammatesp, 3);
            oppPlayersp = ClosestTwoPlayers(opp,2);
            tmatesp[2] = mySelfp;
            double[] closestmateOpp = new double[teammatesp.Length - 1];
            Brain.ResetState();

            if (inputs == null)
            {
                inputs = new double[13];
            }
        
            for (int i = 0; i < closestmateOpp.Length; i++)
            {
                closestmateOpp[i] = 1000;
            }
            
           
            for (int i = 0; i < tmatesp.Length; i++)
            {
                if (i != Unum - 1)
                {
                    inputs[j++] = tmatesp[i].distance(mySelfp);
                    getClosestInSetTo(oppPlayersp, tmatesp[i], ref closestmateOpp[h]);
                    inputs[j++] = closestmateOpp[h++];
                    inputs[j++] = DistFromCenter(tmatesp[i]);
                    ang1 = mySelfp.angle(tmatesp[i]);
                    ang2 = mySelfp.angle(oppPlayersp[0]);
                    ang3 = mySelfp.angle(oppPlayersp[1]);
                    inputs[j++] = Math.Min(Magnitude(ang1 - ang2), Magnitude(ang1 - ang3));
                }
            }
            
            for (int i = 0; i < oppPlayersp.Length; i++)
            {
                inputs[v++] = oppPlayersp[i].distance(mySelfp);
                inputs[v++] = DistFromCenter(oppPlayersp[i]);

            }

            inputs[12] = DistFromCenter(mySelfp);

            setInputIsignalArray(Brain.InputSignalArray, inputs);
            Brain.Activate();

            double[] outputs = new double[3];

            for (int i = 0; i < Brain.OutputSignalArray.Length; i++)
            {
                outputs[i] = Brain.OutputSignalArray[i];
            }
            max = Unum - 1;
            posToPass = mySelfp;

            double maxVal = outputs[0];
            // The following code in this method adapted from Phillip Verbancsics HyperNEATKeepawayPlayer 
            for (int i = 0; i < tmatesp.Length; i++)
            {
                if (i == Unum - 1)
                {
                    max = i;
                    continue;

                }
                else
                {
                    if (outputs[g] > maxVal)
                    {
                        posToPass = tmatesp[i];
                        max = i;
                        maxVal = outputs[g];
                    }
                    else if (maxVal < outputs[g])
                    {

                        posToPass = tmatesp[i];
                        max = i;
                        maxVal = outputs[g];

                    }
                    else
                    {
                        maxVal = outputs[g];
                    }
                    g++;
                }
            }
            action = max;

            lastAction = (action);
            lastActionTime = body.Time;

            if (action == Unum - 1)
            {
                holdBall(0.7);
                return;
            }
            else
            {
                Vector pos = new Vector(0, 0);
                pos.assign(teammates[action + 1].X, teammates[action + 1].Y);
                pos.assign(predictPlayerPosAfterNrCycles(teammates[action + 1], 4, 30, null, null, false));
                pos.assign((float)Math.Min(20 / 2, Math.Max(-20 / 2, pos.X)), (float)Math.Min(20 / 2, Math.Max(-20 / 2, pos.Y)));
                directPass(pos, "fast");
                NEATGame.ballPos = Math.Sqrt((pos.X * pos.X) + (pos.Y * pos.Y));
                NEATGame.no_passes++;
                return;
            }

        }
        #endregion

        #region Vector Helper methods
        private double DistFromCenter(Vector pos)
        {
            return Math.Sqrt(pos.X * pos.X + pos.Y * pos.Y);
        }

        private double Magnitude(double val)
        {
            return Math.Sqrt(val * val);
        }

        #endregion
    }
}
