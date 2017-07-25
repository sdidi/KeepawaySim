
// -*-C#-*-

/***************************************************************************
                                   HyperNEATKeepawayPlayer.cs
                    HyperNEAT based keepaway player for Keepaway domain experiments.
                             -------------------
    begin                : JUL-2009
    credit               : Phillip Verbancsics of the
                            Evolutionary Complexity Research Group
    email                : verb@cs.ucf.edu
 
 ***************************************************************************/

/***************************************************************************
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU LGPL as published by the Free Software  *
 *   Foundation; either version 3 of the License, or (at your option) any  *
 *   later version.                                                        *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoboCup.Geometry;
using RoboCup;

namespace Keepaway
{

    

        

    /// <summary>
    /// Keepaway player where HyperNEAT controls the keeper with the ball decision process
    /// </summary>
    public class HyperNEATKeepawayPlayer : FixedKeepawayPlayer
    {
        #region Variables
        public DecodedNetworks net { get; set; }
        public static KeepawayConfig config { get; set; }
        int lastAction = 0;
        int lastActionTime = 0;
        double[] inputs = null;
        Dictionary<int, int> playerToIndex = new Dictionary<int, int>();
        #endregion

        #region Constructor
        public HyperNEATKeepawayPlayer(RoboCup.Stadium std, string team, int unum, string side, int numK, int numT)
            : base(std, team, unum, side, numK, numT)
        {

        }
        #endregion

        #region HyperNEAT controlled Keeper with ball action
        override public void keeperWithBall()
        {

            Vector posToPass = new Vector(0, 0);
            Command soc = new Command();
            soc.Com = "illegal";

            if (inputs == null)
            {
                inputs = new double[(int)(config.SubstrateX * config.SubstrateY)];
            }
            

            int max = 0;
            double maxD = config.FieldX * Math.Sqrt(2) * 1.1;



            for (int i = 0; i < config.NumKeepers; i++)
            {

                if (teammatesp[i].Radius > maxD)
                {

                    lastAction = Unum - 1;
                    timeLastAction = body.Time;
                    holdBall(0.7);
                    return;

                }

            }

            for (int i = 0; i < config.NumTakers; i++)
            {

                if (opp[i].Radius > maxD)
                {

                    lastAction = Unum - 1;
                    timeLastAction = body.Time;
                    holdBall(0.7);
                    return;

                }

            }

            int action = 0;

            if (lastAction != Unum - 1 && lastActionTime == body.Time - 1)
            {

                action = lastAction;
                if (action == config.NumKeepers)
                {
                    lastAction = Unum - 1;
                    lastActionTime = body.Time;
                    freezeBall();
                    return;
                }
                lastAction = (action);
                lastActionTime = body.Time;

            }
            else
            {

                if (body.Time - lastActionTime > 5)
                {

                    action = config.NumKeepers;
                    lastAction = (action);
                    lastActionTime = body.Time;
                    holdBall(0.7);
                    return;

                }
                else
                {

                    for (int inp = 0; inp < inputs.Length; inp++)
                    {
                        inputs[inp] = 0.0f;
                    }

                    Line path;

                    Vector curSpot = new Vector(0, 0);

                    float xInterval = (float)(Math.Abs(config.SubstrateEndX - config.SubstrateStartX) / config.SubstrateX);
                    float yInterval = (float)(Math.Abs(config.SubstrateEndY - config.SubstrateStartY) / config.SubstrateY);
                    float margin = (float)(xInterval * config.FieldX / 2.0);
                    float xPos = mySelfp.X;
                    float yPos = mySelfp.Y;

                    for (int yCord = 0; yCord < config.SubstrateY; yCord++)
                    {
                        for (int xCord = 0; xCord < config.SubstrateX; xCord++)
                        {

                            float x = (float)(config.SubstrateStartX + xInterval / 2 + xCord * xInterval);
                            float y = (float)(config.SubstrateStartY + yInterval / 2 + yCord * yInterval);
                            x *= (float)(config.FieldX / 2.0);
                            y *= (float)(config.FieldY / 2.0);



                            curSpot.assign(x, y);

                            if (Math.Abs(curSpot.X - xPos) <= margin / 2 && Math.Abs(curSpot.Y - yPos) <= margin / 2)
                            {
                                playerToIndex[Unum] = subPosition(x - (config.FieldX / config.SubstrateX) / 2, y - (config.FieldY / config.SubstrateY) / 2);

                                inputs[subPosition(x - (config.FieldX / config.SubstrateX) / 2, y - (config.FieldY / config.SubstrateY) / 2)] += 1.0;
                                break;

                            }


                        }
                    }

                    for (int i = 0; i < config.NumKeepers; i++)
                    {
                        if (i == Unum - 1) continue;

                        if (teammatesp[i].Radius < maxD)
                        {
                            path = Line.makeLineFromTwoPoints(mySelfp, teammatesp[i]);

                            xPos = teammatesp[i].X;
                            yPos = teammatesp[i].Y;

                            float minX = Math.Min(teammatesp[i].X, mySelfp.X);
                            float minY = Math.Min(teammatesp[i].Y, mySelfp.Y);

                            float maxX = Math.Max(teammatesp[i].X, mySelfp.X);
                            float maxY = Math.Max(teammatesp[i].Y, mySelfp.Y);


                            minX = minX - (minX % margin) - margin;
                            minY = minY - (minY % margin) - margin;
                            maxX = maxX + margin - (maxX % margin);
                            maxY = maxY + margin - (maxY % margin);

                            for (int yCord = 0; yCord < config.SubstrateY; yCord++)
                            {
                                for (int xCord = 0; xCord < config.SubstrateX; xCord++)
                                {

                                    float x = (float)(config.SubstrateStartX + xInterval / 2 + xCord * xInterval);
                                    float y = (float)(config.SubstrateStartY + yInterval / 2 + yCord * yInterval);
                                    x *= (float)(config.FieldX / 2.0);
                                    y *= (float)(config.FieldY / 2.0);



                                    curSpot.assign(x, y);

                                    if ((curSpot.X >= minX && curSpot.Y >= minY && curSpot.X <= maxX && curSpot.Y <= maxY) && path.getDistanceWithPoint(curSpot) <= margin / 2)
                                    {

                                        inputs[subPosition(x - (config.FieldX / config.SubstrateX) / 2, y - (config.FieldY / config.SubstrateY) / 2)] += 1.0 / (1 + mySelfp.distance(teammatesp[i]));

                                    }

                                    if (Math.Abs(curSpot.X - xPos) <= margin / 2 && Math.Abs(curSpot.Y - yPos) <= margin / 2)
                                    {
                                        playerToIndex[i + 1] = subPosition(x - (config.FieldX / config.SubstrateX) / 2, y - (config.FieldY / config.SubstrateY) / 2);

                                        inputs[subPosition(x - (config.FieldX / config.SubstrateX) / 2, y - (config.FieldY / config.SubstrateY) / 2)] += .5;


                                    }


                                }

                            }

                        }
                    }


                    for (int i = 0; i < config.NumTakers; i++)
                    {
                        if (opp[i].Radius < maxD)
                        {
                            path = Line.makeLineFromTwoPoints(mySelfp, opp[i]);

                            xPos = opp[i].X;
                            yPos = opp[i].Y;

                            float minX = Math.Min(opp[i].X, mySelfp.X);
                            float minY = Math.Min(opp[i].Y, mySelfp.Y);

                            float maxX = Math.Max(opp[i].X, mySelfp.X);
                            float maxY = Math.Max(opp[i].Y, mySelfp.Y);
                            minX = minX - (minX % margin) - margin;
                            minY = minY - (minY % margin) - margin;
                            maxX = maxX + margin - (maxX % margin);
                            maxY = maxY + margin - (maxY % margin);

                            for (int yCord = 0; yCord < config.SubstrateY; yCord++)
                            {
                                for (int xCord = 0; xCord < config.SubstrateX; xCord++)
                                {
                                    float x = (float)(config.SubstrateStartX + xInterval / 2 + xCord * xInterval);
                                    float y = (float)(config.SubstrateStartY + yInterval / 2 + yCord * yInterval);
                                    x *= (float)(config.FieldX / 2.0);
                                    y *= (float)(config.FieldY / 2.0);
                                    curSpot.assign(x, y);
                                    if ((curSpot.X >= minX && curSpot.Y >= minY && curSpot.X <= maxX && curSpot.Y <= maxY) && path.getDistanceWithPoint(curSpot) <= margin / 2)
                                    {

                                        inputs[subPosition(x - (config.FieldX / config.SubstrateX) / 2, y - (config.FieldY / config.SubstrateY) / 2)] -= 1.0 / (1 + mySelfp.distance(opp[i]));

                                    }

                                    if (Math.Abs(curSpot.X - xPos) <= margin / 2 && Math.Abs(curSpot.Y - yPos) <= margin / 2)
                                    {

                                        inputs[subPosition(x - (config.FieldX / config.SubstrateX) / 2, y - (config.FieldY / config.SubstrateY) / 2)] -= 1.0;

                                    }


                                }

                            }

                        }

                    }


                    net.Flush();
                    net.SetInputs(inputs);

                    net.ActivateNetwork(2);


                    double[] outputs = net.GetOutputs();

                    max = Unum - 1;
                    posToPass = mySelfp;


                    double maxVal = outputs[playerToIndex[Unum]];

                    for (int i = 0; i < config.NumKeepers; i++)
                    {

                        if (i == Unum - 1) continue;
                        if (outputs[playerToIndex[i + 1]] > maxVal)
                        {
                            posToPass = teammatesp[i];
                            max = i;
                            maxVal = outputs[playerToIndex[i + 1]];

                        }
                    }

                    action = max;


                   

                }
            }

            lastAction = (action);
            lastActionTime = body.Time;

            if (action == Unum - 1)
            {
                holdBall(0.7);
                Program.ballPos = Math.Sqrt((ball.X * ball.X) + (ball.Y * ball.Y));
                Program.no_passes++;
                return;
            }
            else
            {


                Vector pos = new Vector(0, 0);
                pos.assign(teammates[action + 1].X, teammates[action + 1].Y);
                pos.assign(predictPlayerPosAfterNrCycles(teammates[action + 1], 4, 30, null, null, false));
                pos.assign((float)Math.Min(config.FieldX / 2, Math.Max(-config.FieldX / 2, pos.X)), (float)Math.Min(config.FieldY / 2, Math.Max(-config.FieldY / 2, pos.Y)));
                directPass(pos, "fast");
                Program.ballPos = Math.Sqrt((pos.X * pos.X) + (pos.Y * pos.Y));
                Program.no_passes++;
                return;

            }


        }

        #endregion

        #region Helper Methods
        private int subPosition(double x, double y)
        {
            x /= (config.FieldX / 2);
            y /= (config.FieldY / 2);

            x += 1.0;
            x /= 2;

            y += 1.0;
            y /= 2;

            x = Math.Max(0, x);
            x = Math.Min(1, x);
            y = Math.Max(0, y);
            y = Math.Min(1, y);


            return (int)(Math.Max(0, Math.Min(Math.Min(x * config.SubstrateX, config.SubstrateX - 1) + Math.Min(y * config.SubstrateY, config.SubstrateY - 1) * config.SubstrateX, config.SubstrateX * config.SubstrateY - 1)));

        }
        #endregion
    }
}

