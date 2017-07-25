
// -*-C#-*-

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
using RoboCup;
using RoboCup.Geometry;

namespace Keepaway
{
    /// <summary>
    /// A fixed policy keepaway player
    /// </summary>
    public class FixedKeepawayPlayer : RoboCup.Objects.Player
    {
        #region Instance Variables

        public Dictionary<int, FullPlayerData> teammates = new Dictionary<int, FullPlayerData>();
        public Dictionary<int, FullPlayerData> opponents = new Dictionary<int, FullPlayerData>();
        public FullPlayerData myself;
        public FullBallData ball;
        public Vector[] teammatesp;
        protected Vector[] opp;
        protected Vector mySelfp = new Vector(0, 0);
        protected Vector ballp = new Vector(0, 0);
        protected Vector posToPass = new Vector(0, 0);
        protected int timeLastAction = 0;
        int trajectoryChanges = 0;
        int trajectorySame = 0;
        Vector prevBallVel = new Vector(0, 0);
        Vector ballVel = new Vector(0, 0);
        List<Vector> ballVelocities = new List<Vector>();
        int turns = 0;
        Vector prevBallPos = new Vector(0, 0);
        int timeLastIntercepted = 0;
        Vector posOldIntercept = new Vector(-1000, -1000);

        int lastAction = 0;
        int lastActionTime = 0;
        #endregion

        #region Constructor

        public FixedKeepawayPlayer(Stadium stad, string team, int unum, string side, int numK, int numT)
            : base(stad, team, unum, side)
        {

            teammatesp = new Vector[(Team == "keepers") ? numK : numT];
            opp = new Vector[(Team == "takers") ? numK : numT];

            for (int i = 0; i < teammatesp.Length; i++)
            {
                teammatesp[i] = new Vector(0, 0);
            }


            for (int i = 0; i < opp.Length; i++)
            {
                opp[i] = new Vector(0, 0);
            }



        }

        #endregion

        #region MainLoop of Player [Begin edits here to change player]
        public override void PlayerCommand()
        {


            if (FullState.Players.Count != teammates.Count + opponents.Count)
            {

                foreach (var p in FullState.Players)
                {
                    if (p.Value.Team == Team)
                    {
                        if (p.Value.Unum == Unum) { myself = p.Value; mySelfp.X = p.Value.X; mySelfp.Y = p.Value.Y; }
                        teammates[p.Value.Unum] = (p.Value);
                        teammatesp[p.Value.Unum - 1].assign(p.Value.X, p.Value.Y);
                    }
                    else
                    {
                        opponents[p.Value.Unum] = (p.Value);
                        opp[p.Value.Unum - 1].assign(p.Value.X, p.Value.Y);
                    }
                }


                foreach (var p in FullState.Balls)
                {
                    ball = p.Value;
                    ballp.assign(p.Value.X, p.Value.Y);
                }
            }
            else
            {


                foreach (var p in FullState.Players)
                {
                    if (p.Value.Team == Team)
                    {
                        if (p.Value.Unum == Unum) { mySelfp.X = p.Value.X; mySelfp.Y = p.Value.Y; }
                        teammatesp[p.Value.Unum - 1].assign(p.Value.X, p.Value.Y);
                    }
                    else
                    {
                        opp[p.Value.Unum - 1].assign(p.Value.X, p.Value.Y);
                    }
                }

                foreach (var p in FullState.Balls)
                {
                    ballp.assign(p.Value.X, p.Value.Y);
                }
            }

            prevBallVel.assign(ballVel);
            ballVel.assign(ball.VelX, ball.VelY);

            if (Math.Abs(prevBallVel.angle(ballVel) * 180.0 / Math.PI) > 7)
            {
                trajectoryChanges++;
                trajectorySame = 0;
            }
            else
            {
                trajectoryChanges = 0;
                trajectorySame++;
            }

            ballVelocities.Add(new Vector(ball.X, ball.Y));

            if (ballVelocities.Count > 5) ballVelocities.RemoveAt(0);


            if (Team == "keepers")
            {


                keeper();

            }
            else
            {
                taker();
            }



        }

        #endregion

        #region Keeper Actions [Begin Edits here to change keeper behavior]

        private void keeperSupport(int fastest)
        {
            Command soc;

            int iCycles = (fastest > 0) ? predictNrCyclesToBall(teammates[fastest], ballp) : 31;
            Vector posPassFrom = predictBallPosAfterNrCycles(iCycles);
            RoboCup.Geometry.Rectangle rect = new Rectangle(new Vector(0, 0), new Vector(serverParameters.Keepaway_Length, serverParameters.Keepaway_Width));
            soc = getOpenForPassFromInRectangle(rect, posPassFrom);

            if (soc.Com == "dash")
            {
                dash((float)soc.powAng);
            }
            else if (soc.Com == "turn")
            {
                turn((float)soc.powAng);
            }


        }

        public void keeper()
        {




            // If the ball is kickable,
            // call main action selection routine.
            if (isBallKickable())
            {
                turns = 0;
                trajectoryChanges = 0;
                trajectorySame = 2;
                keeperWithBall();
            }

            // Get fastest to ball
            int iTmp = 0;
            int fastest = getFastestInSetTo(teammates, ballp, ref iTmp);


            // If fastest, intercept the ball.
            if (fastest == Unum)
            {


                intercept();
                return;
            }

            // Not fastest, get open
            turns = 0;
            keeperSupport(fastest);
        }

        public virtual void keeperWithBall()
        {


            double closestOpp = 1000;

            getClosestInSetTo(opp, mySelfp, ref closestOpp);

            if (closestOpp > 5)
            {
                holdBall(0.7);
                return;
            }

            int teamateToPassTo = -1;
            closestOpp = 1000;
            double close = 1000;
            for (int i = 0; i < teammatesp.Length; i++)
            {
                if (i != Unum - 1)
                {
                    if (teammatesp[i].distance(mySelfp) < close)
                    {
                        teamateToPassTo = i;
                        close = teammatesp[i].distance(mySelfp);
                    }
                }
            }

            directPass(predictPlayerPosAfterNrCycles(teammates[teamateToPassTo + 1], 4, 100, null, null, false), "normal");



        }

        #endregion

        #region Taker Actions [Begin edits here to change taker behavior]

        private void taker()
        {
            Command soc = new Command();
            soc.Com = "illegal";
            double meh = 1000;



            // Maintain possession if you have the ball.
            if (isBallKickable() && getClosestInSetTo(teammatesp, ballp, ref meh) == Unum)
            {
                trajectoryChanges = 0;
                trajectorySame = 2;
                turns = 0;
                holdBall(0.3);
                return;
            }

            // If not first or second closest, then mark open opponent
            int numT = teammates.Count;
            List<FullPlayerData> T = new List<FullPlayerData>(teammates.Values);
            T.Sort((p, q) => (teammatesp[p.Unum - 1].distance(ballp).CompareTo(teammatesp[q.Unum - 1].distance(ballp))));


            if (numT > 2 && T[0].Unum != Unum && T[1].Unum != Unum)
            {
                int temp = 0;
                int withBall = getFastestInSetTo(opponents, ballp, ref temp);
                markMostOpenOpponent(withBall);

                return;
            }

            // If teammate has it, don't mess with it
            double oppDist = 1000.0;
            double teamDist = 1000.0;

            int closest = getClosestInSetTo(teammatesp, ballp, ref teamDist);
            int oppClosest = getClosestInSetTo(opp, ballp, ref oppDist);

            if (oppDist < teamDist &&
                 closest != Unum &&
                 teamDist < MaximalKickDistance)
            {


                soc = turnBodyToBall();

                if (soc.Com == "turn")
                {
                    turn((float)soc.powAng);
                }
                else if (soc.Com == "dash")
                {
                    dash((float)soc.powAng);
                }


                return;
            }


            // Otherwise try to intercept the ball
            intercept();

            return;
        }

        #endregion

        #region Instance Methods
        public void directPass(Vector pos, string passType)
        {
            if (passType == "normal")
                kickTo(pos, 1.2);
            else if (passType == "fast")
                kickTo(pos, 1.6);

        }
        public double getKickSpeedToTravel(double dDistance, double dEndSpeed)
        {
            // if endspeed is zero we have an infinite series and return the first term
            // that corresponds to the distance that has to be travelled.
            if (dEndSpeed < 0.0001)
                return getFirstInfGeomSeries(dDistance, serverParameters.Ball_decay);

            // use geometric series to calculate number of steps and with that the
            // velocity to give to the ball, we start at endspeed and therefore use
            // the inverse of the ball decay (r).
            // s = a + a*r + .. a*r^n since we calculated from endspeed (a) to
            // firstspeed, firstspeed equals a*r^n = endspeed*r^nr_steps
            double dNrSteps = getLengthGeomSeries(dEndSpeed,
                                            1.0 / serverParameters.Ball_decay, dDistance);
            return getFirstSpeedFromEndSpeed(dEndSpeed, (int)Math.Round(dNrSteps, MidpointRounding.AwayFromZero), 0);
        }

        public static double getFirstInfGeomSeries(double dSum, double dRatio)
        {

            // s = a(1-r^n)/(1-r) with r->inf and 0<r<1 => r^n = 0 => a = s ( 1 - r)
            return dSum * (1 - dRatio);
        }

        public static double getLengthGeomSeries(double dFirst, double dRatio, double dSum)
        {

            // s = a + ar + ar^2 + .. + ar^n-1 and thus sr = ar + ar^2 + .. + ar^n
            // subtract: sr - s = - a + ar^n) =>  s(1-r)/a + 1 = r^n = temp
            // log r^n / n = n log r / log r = n = length
            double temp = (dSum * (dRatio - 1) / dFirst) + 1;
            if (temp <= 0)
                return -1.0;
            return Math.Log(temp) / Math.Log(dRatio);
        }

        public double getFirstSpeedFromEndSpeed(double dEndSpeed, double dCycles, double dDecay)
        {
            if (dDecay <= 0)
                dDecay = serverParameters.Ball_decay;

            // geometric serie: s = a + a*r^1 + .. + a*r^n, now given endspeed = a*r^n .
            // endspeed = firstspeed * ratio ^ length .
            // firstpeed = endspeed * ( 1 / ratio ) ^ length
            return dEndSpeed * Math.Pow(1 / dDecay, dCycles);
        }

        public void kickTo(Vector posTarget, double dEndSpeed)
        {
            Vector posBall = new Vector(ballp.X, ballp.Y);
            Vector velBall = new Vector(ball.VelX, ball.VelY);
            Vector posTraj = posTarget - posBall;
            Vector posAgent = new Vector(mySelfp.X, mySelfp.Y);

            if (BallSpeed > 0.1 && Math.Abs(posTraj.angle(ballVel) * 180.0 / Math.PI) > 7)
            {
                //  freezeBall();
                //  return;
            }

            Vector velDes = Vector.fromPolar((float)getKickSpeedToTravel(posTraj.Radius, dEndSpeed), (float)posTraj.Theta);
            double dPower;
            double angActual;

            if (predictAgentPos(1, 0).distance(posBall + velDes) <
                serverParameters.Ball_size + serverParameters.Player_size)
            {
                Line line = Line.makeLineFromTwoPoints(posBall, posBall + velDes);
                Vector posBodyProj = line.getPointOnLineClosestTo(posAgent);
                double dDist = posBall.distance(posBodyProj);
                if (velDes.Radius < dDist)
                    dDist -= serverParameters.Ball_size + serverParameters.Player_size;
                else
                    dDist += serverParameters.Ball_size + serverParameters.Player_size;

                velDes.assign(Vector.fromPolar((float)dDist, (float)velDes.Theta));
            }




            double dDistOpp = 1000;
            int objOpp = getClosestInSetTo(opp, ballp, ref dDistOpp);

            if (velDes.Radius > serverParameters.Ball_speed_max) // can never reach point
            {
                dPower = serverParameters.Maxpower;
                double dSpeed = getActualKickPowerRate() * dPower;
                velBall.rotate(-(float)velDes.Theta);
                double tmp = velBall.Y;
                angActual = normalizeAngle(velDes.Theta * 180.0 / Math.PI - asinDeg(tmp / dSpeed));
                double dSpeedPred = (new Vector(ball.VelX, ball.VelY) +
                                       Vector.fromPolar((float)dSpeed, (float)(angActual * Math.PI / 180.0))
                                      ).Radius;

                // but ball acceleration in right direction is very high
                if (dSpeedPred > 0.85 * serverParameters.Ball_accel_max)
                {

                    accelerateBallToVelocity(velDes);    // shoot nevertheless

                    return;
                }
                else if (getActualKickPowerRate() >
                         0.85 * serverParameters.Kick_power_rate)
                {
                    // ball well-positioned
                    freezeBall();                          // freeze ball
                    return;
                }
                else
                {

                    kickBallCloseToBody(0, 0.16);            // else position ball better
                    return;
                }
            }
            else                                            // can reach point
            {
                Vector accBallDes = velDes - velBall;
                dPower = getKickPowerForSpeed(accBallDes.Radius);
                if (dPower <= 1.05 * serverParameters.Maxpower || // with current ball speed
                (dDistOpp < 2.0 && dPower <= 1.30 * serverParameters.Maxpower))
                {                               // 1.05 since cannot get ball fully perfect

                    accelerateBallToVelocity(velDes);  // perform shooting action
                    return;
                }
                else
                {

                    kickBallCloseToBody(0, 0.16);

                    return;            // position ball better
                }
            }
        }


        public bool isBallKickable()
        {
            return (mySelfp - ballp).Radius < MaximalKickDistance;
        }

        public double MaximalKickDistance { get { return (serverParameters.Kickable_margin + serverParameters.Player_size + serverParameters.Ball_size); } }


        public int getFastestInSetTo(Dictionary<int, FullPlayerData> set, Vector obj, ref int iCyclesToIntercept)
        {
            int iCyclesToObj;
            int iMinCycles = 100;
            Vector posObj = new Vector(0, 0);
            Vector pos1 = new Vector(0, 0);
            int fastestObject = -1;
            int iCyclesFastestOpp = 30; // how much do we try

            bool bSkip = false;
            int iCycles = -1;
            Vector globalPosition = new Vector(0, 0);
            while (bSkip == false &&
                   iCycles < iMinCycles &&
                   iCycles <= iCyclesFastestOpp)
            {
                iCycles++;
                iMinCycles = 100;
                posObj = predictBallPosAfterNrCycles(iCycles);

                foreach (var p in set)
                {
                    globalPosition.assign(p.Value.X, p.Value.Y);
                    if (globalPosition.distance(posObj) / serverParameters.Player_speed_max
                            < iMinCycles &&
                        globalPosition.distance(posObj) / serverParameters.Player_speed_max

                            < iCycles + 1)
                    {

                        iCyclesToObj = predictNrCyclesToPoint(p.Value, posObj);
                        if (iCyclesToObj < iMinCycles)
                        {
                            iMinCycles = iCyclesToObj;
                            fastestObject = p.Value.Unum;
                        }
                    }
                }

            }


            iCyclesToIntercept = iCycles;


            return fastestObject;
        }

        public int predictNrCyclesToPoint(FullPlayerData o, Vector posTo1)
        {

            Vector posTo = new Vector(posTo1.X, posTo1.Y);

            Vector posGlobal = new Vector(o.X, o.Y), posPred = new Vector(0, 0);
            Vector vel = new Vector(0, 0);
            int iCycles;
            double angBody, angNeck = 0, ang;
            double angDes = (posTo - posGlobal).Theta * 180.0 / Math.PI;
            Command soc;


            // if already in kickable distance, return 0
            if (posTo.distance(posGlobal) < MaximalKickDistance)
            {
                return 0;
            }



            angBody = o.BodyDirection;
            vel = new Vector(o.VelX, o.VelY);
            posPred = new Vector(o.X, o.Y);
            iCycles = 0;




            soc = predictCommandToMoveToPos(o, posTo, 1, 2.5, false, posPred, vel, angBody);
            ang = normalizeAngle(angBody - angDes);

            // sometimes we dash to stand still and turn then
            while (soc.Com == "turn" &&
                   (Math.Abs(ang) > 20 || soc.Com == "dash" && soc.powAng < 0))
            {
                iCycles++;
                predictStateAfterCommand(soc, posPred, vel, ref angBody, ref angNeck, o);
                if (posTo.distance(posPred) < MaximalKickDistance)
                {

                    return iCycles;
                }
                soc = (predictCommandToMoveToPos(o, posTo, 1, 2.5, false, posPred, vel, angBody));
                ang = normalizeAngle(angBody - angDes);
            }


            if (!o.Equals(myself))
            {
                // iCycles++; // do not count last dash . predictState not called
                vel.rotate(-(float)(Math.PI / 180.0 * angBody));
                double dVel = vel.X; // get distance in direction
                iCycles += predictNrCyclesForDistance(o, posPred.distance(posTo), dVel);
            }
            else
            {
                while (posPred.distance(posTo) > MaximalKickDistance)
                {
                    soc = predictCommandToMoveToPos(o, posTo, 1, 2.5, false, posPred, vel, angBody);
                    predictStateAfterCommand(soc, posPred, vel, ref angBody, ref angNeck, o);
                    iCycles++;
                }
            }

            return iCycles;

        }

        public double normalizeAngle(double angle)
        {
            while (angle > 180.0) angle -= 360.0;
            while (angle < -180.0) angle += 360.0;

            return (angle);
        }

        public int predictNrCyclesForDistance(FullPlayerData o, double dDist, double dSpeed)
        {
            double dSpeedPrev = -1.0;
            int iCycles = 0;
            double dDecay = serverParameters.Player_decay;
            double dDashRate = serverParameters.Dash_power_rate;
            double dMinDist = MaximalKickDistance;

            // stop this loop when max speed is reached or the distance is traveled.
            while (dDist > dMinDist &&
                   (Math.Abs(dSpeed - dSpeedPrev) > 10e-5 || dSpeed < 0.3) &&
                   iCycles < 40) // ignore standing still and turning
            {
                dSpeedPrev = dSpeed;
                dSpeed += serverParameters.Maxpower * dDashRate;
                if (dSpeed > serverParameters.Player_speed_max)
                    dSpeed = serverParameters.Player_speed_max;
                dDist = Math.Max(0, dDist - dSpeed);
                dSpeed *= dDecay;
                iCycles++;
            }
            dSpeed /= dDecay;

            // if distance not completely traveled yet, count the number of cycles to 
            // travel the remaining distance with this speed.
            if (dDist > dMinDist)
                iCycles += (int)Math.Ceiling((dDist - dMinDist) / dSpeed);
            return Math.Max(0, iCycles);
        }

        public bool predictStateAfterCommand(Command com, Vector pos, Vector vel, ref double angGlobalBody, ref double angGlobalNeck, FullPlayerData obj)
        {
            switch (com.Com) // based on kind of command, choose action
            {
                case "dash":
                    predictStateAfterDash(com.powAng, pos, vel, angGlobalBody, obj);
                    break;
                case "turn":
                    predictStateAfterTurn(com.powAng, pos, vel, ref angGlobalBody, obj);
                    break;
                case "illegal":
                    predictStateAfterDash(0.01, pos, vel, angGlobalBody, obj);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public void predictStateAfterTurn(double dSendAngle, Vector pos, Vector vel, ref double angBody, FullPlayerData obj)
        {

            // calculate effective turning angle and neck accordingly
            double dEffectiveAngle;
            dEffectiveAngle = getActualTurnAngle(dSendAngle, vel.Radius, obj);
            angBody = normalizeAngle(angBody + dEffectiveAngle);

            // update as if dashed with no power
            predictStateAfterDash(0.0, pos, vel, angBody, obj);
            return;
        }

        public double getActualTurnAngle(double angTurn, double dSpeed, FullPlayerData o)
        {
            return angTurn / (1.0 + serverParameters.Inertia_moment * dSpeed);
        }

        public void predictStateAfterDash(double dActualPower, Vector pos, Vector vel, double dDirection, FullPlayerData obj)
        {
            // get acceleration associated with actualpower
            double dEffort = 1.0;
            double dAcc = dActualPower * serverParameters.Dash_power_rate * dEffort;
            Vector temp;
            // add it to the velocity; negative acceleration in backward direction
            if (dAcc > 0)
            {
                temp = vel + Vector.fromPolar((float)dAcc, (float)(dDirection * Math.PI / 180.0));
                vel.assign(temp);
            }
            else
            {
                temp = vel + Vector.fromPolar((float)Math.Abs(dAcc), (float)(normalizeAngle(dDirection + 180) * Math.PI / 180.0));
                vel.assign(temp);

            }
            // check if velocity doesn't exceed maximum speed
            if (vel.Radius > serverParameters.Player_speed_max)
                vel.normalize(serverParameters.Player_speed_max);

            // add velocity to current global position and decrease velocity
            temp = pos + vel;
            pos.assign(temp);

            temp = vel * serverParameters.Player_decay;
            vel.assign(temp);

        }

        public Command predictCommandToMoveToPos(FullPlayerData obj, Vector posTo, int iCycles, double dDistBack, bool bMoveBack, Vector posIn, Vector velIn, double angBody)
        {
            Vector pos = posIn, vel = velIn;
            Command soc = new Command();
            soc.Com = "illegal";
            double dPower;


            soc = predictCommandTurnTowards(obj, posTo, iCycles, dDistBack, bMoveBack, posIn, velIn, angBody);
            if (soc.Com != "illegal")
                return soc;

            dPower = getPowerForDash(posTo - pos, angBody, vel, 1.0, iCycles);
            soc.Com = "dash";
            soc.powAng = dPower;
            return soc;
        }

        public double getPowerForDash(Vector posRelTo1, double angBody, Vector vel1, double dEffort, int iCycles)
        {
            Vector posRelTo = new Vector(posRelTo1.X, posRelTo1.Y);
            Vector vel = new Vector(vel1.X, vel1.Y);
            // the distance desired is the x-direction to the relative position we
            // we want to move to. If point lies far away, we dash maximal. Furthermore
            // we subtract the x contribution of the velocity because it is not necessary
            // to dash maximal.
            posRelTo.rotate(-(float)(Math.PI / 180.0 * angBody));
            double dDist = posRelTo.X; // get distance in direction
            if (iCycles <= 0) iCycles = 1;
            double dAcc = getFirstSpeedFromDist(dDist, iCycles, serverParameters.Player_decay);
            // get speed to travel now
            if (dAcc > serverParameters.Player_speed_max)             // if too far away
                dAcc = serverParameters.Player_speed_max;// set maximum speed
            vel.rotate(-(float)(Math.PI / 180.0 * angBody));
            dAcc -= vel.X;             // subtract current velocity

            // acceleration = dash_power * dash_power_rate * effort .
            // dash_power = acceleration / (dash_power_rate * effort )
            double dDashPower = dAcc / (serverParameters.Dash_power_rate * dEffort);
            if (dDashPower > serverParameters.Maxpower)
                return serverParameters.Maxpower;
            else if (dDashPower < serverParameters.Minpower)
                return serverParameters.Minpower;
            else
                return dDashPower;
        }

        public double getFirstSpeedFromDist(double dDist, double dCycles, double dDecay)
        {

            return getFirstGeomSeries(dDist, dDecay, dCycles);
        }

        public static double getFirstGeomSeries(double dSum, double dRatio, double dLength)
        {
            // s = a + ar + ar^2 + .. + ar^n-1 and thus sr = ar + ar^2 + .. + ar^n
            // subtract: s - sr = a - ar^n) =>  s = a(1-r^n)/(1-r) => a = s*(1-r)/(1-r^n)
            return dSum * (1 - dRatio) / (1 - Math.Pow(dRatio, dLength));
        }

        public double getAngleForTurn(double angDesiredAngle, double dSpeed, FullPlayerData obj)
        {
            double a = angDesiredAngle * (1.0 + Playertype.InertiaMoment * dSpeed);
            if (a > serverParameters.MaxMoment)
                return serverParameters.MaxMoment;
            else if (a < serverParameters.MinMoment)
                return serverParameters.MinMoment;
            else
                return a;
        }

        public Command predictCommandTurnTowards(FullPlayerData obj, Vector posTo, int iCycles, double dDistBack, bool bMoveBack, double angBodyIn)
        {
            Command soc = new Command(), socFirst = new Command();
            Vector pos = new Vector(obj.X, obj.Y), vel = new Vector(obj.VelX, obj.VelY);
            double angBody = angBodyIn, ang, angNeck, angTo;

            soc.Com = "illegal";
            socFirst.Com = "illegal";
            bool bFirst = true;

            // predict where we will finally stand when our current vel is propogated
            // and then check the orthogonal distance w.r.t. our body direction
            Vector posPred = predictPlayerPosAfterNrCycles(obj, Math.Min(iCycles, 4), 0, pos, vel, false);
            Line line = Line.makeLineFromPositionAndAngle(posPred, angBody);
            double dDist = line.getDistanceWithPoint(posTo);

            // get the angle to this point
            angTo = (posTo - posPred).Theta * 180.0 / Math.PI;
            angTo = normalizeAngle(angTo - angBody);

            // determine whether we want to turn based on orthogonal distance 
            double dRatioTurn;
            if (pos.distance(posTo) > 30.0)
                dRatioTurn = 4.0;
            if (pos.distance(posTo) > 20.0)
                dRatioTurn = 3.0;
            else if (pos.distance(posTo) > 10)
                dRatioTurn = 2.0;
            else
                dRatioTurn = 0.90;

            double angTmp = angTo + ((bMoveBack) ? 180 : 0);
            angTmp = normalizeAngle(angTmp);

            // turn when: 
            //  1. point lies outside our body range (forward and backwards)
            //  2. point lies outside distBack and behind us (forward move) 
            //  3. point lies outside distBack and in front of us backwards move)
            int turn = 0;
            while ((dDist > dRatioTurn * MaximalKickDistance ||
                    (posPred.distance(posTo) > dDistBack &&
                        ((Math.Abs(angTo) > 90 && bMoveBack == false) ||
                          (Math.Abs(angTo) < 90 && bMoveBack == true))))
                   && turn < 5 && Math.Abs(angTmp) > 7.0)
            {

                ang = (posTo - posPred).Theta * 180.0 / Math.PI + ((bMoveBack == true) ? 180 : 0);
                ang = normalizeAngle(ang - angBody);
                soc.Com = "turn";
                soc.powAng = getAngleForTurn(ang, vel.Radius, obj);

                if (bFirst == true)
                    socFirst = (soc);
                bFirst = false;
                predictStateAfterTurn(soc.powAng, pos, vel, ref angBody, obj);
                line = Line.makeLineFromPositionAndAngle(posPred, angBody);
                dDist = line.getDistanceWithPoint(posTo);
                angTo = (posTo - posPred).Theta * 180.0 / Math.PI;
                angTo = normalizeAngle(angTo - angBody);
                turn++;
            }

            // if very close and have to turn a lot, it may be better to move with our
            // back to that point
            if (turn > 1 && iCycles < 4 && posPred.distance(posTo) < dDistBack &&
                bMoveBack == false)
            {
                angBody = obj.BodyDirection;
                pos = new Vector(obj.X, obj.Y);
                vel = new Vector(obj.VelX, obj.VelY);
                ang = (posTo - posPred).Theta * 180.0 / Math.PI + 180;
                ang = normalizeAngle(ang - angBody);
                soc.Com = "turn";
                soc.powAng = getAngleForTurn(ang, vel.Radius, obj);
                predictStateAfterTurn(soc.powAng, pos, vel, ref angBody, obj);
                line = Line.makeLineFromPositionAndAngle(posPred, angBody);
                dDist = line.getDistanceWithPoint(posTo);
                if (dDist < 0.9 * MaximalKickDistance)
                {

                    return soc;
                }
            }

            return socFirst;
        }

        public Command predictCommandTurnTowards(FullPlayerData obj, Vector posTo, int iCycles, double dDistBack, bool bMoveBack, Vector posIn, Vector velIn, double angBodyIn)
        {
            Command soc = new Command(), socFirst = new Command();
            Vector pos = new Vector(posIn.X, posIn.Y), vel = new Vector(velIn.X, velIn.Y);
            double angBody = angBodyIn, ang, angNeck, angTo;

            soc.Com = "illegal";
            socFirst.Com = "illegal";
            bool bFirst = true;

            // predict where we will finally stand when our current vel is propogated
            // and then check the orthogonal distance w.r.t. our body direction
            Vector posPred = predictPlayerPosAfterNrCycles(obj, Math.Min(iCycles, 4), 0, pos, vel, false);
            Line line = Line.makeLineFromPositionAndAngle(posPred, angBody);
            double dDist = line.getDistanceWithPoint(posTo);

            // get the angle to this point
            angTo = (posTo - posPred).Theta * 180.0 / Math.PI;
            angTo = normalizeAngle(angTo - angBody);

            // determine whether we want to turn based on orthogonal distance 
            double dRatioTurn;
            if (pos.distance(posTo) > 30.0)
                dRatioTurn = 4.0;
            if (pos.distance(posTo) > 20.0)
                dRatioTurn = 3.0;
            else if (pos.distance(posTo) > 10)
                dRatioTurn = 2.0;
            else
                dRatioTurn = 0.90;

            double angTmp = angTo + ((bMoveBack) ? 180 : 0);
            angTmp = normalizeAngle(angTmp);

            // turn when: 
            //  1. point lies outside our body range (forward and backwards)
            //  2. point lies outside distBack and behind us (forward move) 
            //  3. point lies outside distBack and in front of us backwards move)
            int turn = 0;
            while ((dDist > dRatioTurn * MaximalKickDistance ||
                    (posPred.distance(posTo) > dDistBack &&
                        ((Math.Abs(angTo) > 90 && bMoveBack == false) ||
                          (Math.Abs(angTo) < 90 && bMoveBack == true))))
                   && turn < 5 && Math.Abs(angTmp) > 7.0)
            {

                ang = (posTo - posPred).Theta * 180.0 / Math.PI + ((bMoveBack == true) ? 180 : 0);
                ang = normalizeAngle(ang - angBody);
                soc.Com = "turn";
                soc.powAng = getAngleForTurn(ang, vel.Radius, obj);

                if (bFirst == true)
                    socFirst = (soc);
                bFirst = false;
                predictStateAfterTurn(soc.powAng, pos, vel, ref angBody, obj);
                line = Line.makeLineFromPositionAndAngle(posPred, angBody);
                dDist = line.getDistanceWithPoint(posTo);
                angTo = (posTo - posPred).Theta * 180.0 / Math.PI;
                angTo = normalizeAngle(angTo - angBody);
                turn++;
            }

            // if very close and have to turn a lot, it may be better to move with our
            // back to that point
            if (turn > 1 && iCycles < 4 && posPred.distance(posTo) < dDistBack &&
                bMoveBack == false)
            {
                angBody = obj.BodyDirection;
                pos = new Vector(obj.X, obj.Y);
                vel = new Vector(obj.VelX, obj.VelY);
                ang = (posTo - posPred).Theta * 180.0 / Math.PI + 180;
                ang = normalizeAngle(ang - angBody);
                soc.Com = "turn";
                soc.powAng = getAngleForTurn(ang, vel.Radius, obj);
                predictStateAfterTurn(soc.powAng, pos, vel, ref angBody, obj);
                line = Line.makeLineFromPositionAndAngle(posPred, angBody);
                dDist = line.getDistanceWithPoint(posTo);
                if (dDist < 0.9 * MaximalKickDistance)
                {

                    return soc;
                }
            }

            return socFirst;
        }

        public Vector predictPlayerPosAfterNrCycles(FullPlayerData o, double dCycles, int iDashPower, Vector posIn, Vector velIn, bool bUpdate)
        {

            Vector vel = ((object)velIn == null) ? new Vector(o.VelX, o.VelY) : new Vector(velIn.X, velIn.Y);
            Vector pos = ((object)posIn == null) ? new Vector(o.X, o.Y) : new Vector(posIn.X, posIn.Y);


            double dDirection = 0.0; // used when no info about global body


            dDirection = o.BodyDirection;

            for (int i = 0; i < (int)dCycles; i++)
                predictStateAfterDash(iDashPower, pos, vel, dDirection, o);


            if ((object)posIn != null && bUpdate)
                posIn.assign(pos);
            if ((object)velIn != null && bUpdate)
                velIn.assign(vel);

            return pos;
        }

        public Vector predictBallPosAfterNrCycles(double dCycles)
        {

            Vector vel = new Vector(0, 0);
            if (trajectorySame > 3 || dCycles < 3)
            {
                vel.assign(ball.VelX, ball.VelY);
            }
            else if (trajectoryChanges > 2)
            {
                for (int i = 0; i < ballVelocities.Count - 1; i++)
                {
                    vel += (ballVelocities[i + 1] - ballVelocities[i]) / (ballVelocities.Count - 1);
                }
            }

            Vector pos = new Vector(ball.X, ball.Y);
            double dDist = getSumGeomSeries(vel.Radius,
                                            serverParameters.Ball_decay,
                                            dCycles);
            pos = pos + Vector.fromPolar((float)dDist, (float)vel.Theta);


            return pos;


        }

        public static double getSumGeomSeries(double dFirst, double dRatio, double dLength)
        {
            // s = a + ar + ar^2 + .. + ar^n-1 and thus sr = ar + ar^2 + .. + ar^n
            // subtract: s - sr = a - ar^n) =>  s = a(1-r^n)/(1-r)
            return dFirst * (1 - Math.Pow(dRatio, dLength)) / (1 - dRatio);
        }

        public struct Command
        {
            public string Com;
            public double powAng;
            public double kickAng;


        }
        public Command getOpenForPassFromInRectangle(Rectangle rect, Vector posFrom)
        {
            Vector bestPoint = leastCongestedPointForPassInRectangle(rect, posFrom);


            double buffer = 1.5;

            if (mySelfp.distance(bestPoint) < buffer)
            {
                return turnBodyToPoint(ballp);
            }
            else
            {
                return moveToPos(bestPoint, 15.0);
            }
        }

        public Command moveToPos(Vector posTo, double angWhenToTurn)
        {
            double dDistBack = 0.0;
            bool bMoveBack = false;
            int iCycles = 1;
            // previously we only turned relative to position in next cycle, now take
            // angle relative to position when you're totally rolled out...
            //  VecPosition posPred   = WM.predictAgentPos( 1, 0 );

            Vector posAgent = new Vector(mySelfp.X, mySelfp.Y);
            Vector posPred = predictFinalAgentPos();

            double angBody = myself.BodyDirection;
            double angTo = (posTo - posPred).Theta * 180.0 / Math.PI;
            angTo = normalizeAngle(angTo - angBody);
            double angBackTo = normalizeAngle(angTo + 180);

            double dDist = posAgent.distance(posTo);
            double temp = -1000;
            if (bMoveBack)
            {
                if (Math.Abs(angBackTo) < angWhenToTurn)
                    return dashToPoint(posTo, iCycles);
                else
                    return turnBackToPoint(posTo);
            }
            else if (Math.Abs(angTo) < angWhenToTurn ||
                     (Math.Abs(angBackTo) < angWhenToTurn && dDist < dDistBack))
                return dashToPoint(posTo, iCycles);
            else
                return directTowards(posTo, angWhenToTurn);

        }

        public Command directTowards(Vector posTurnTo, double angWhenToTurn)
        {
            Vector posAgent = new Vector(myself.X, myself.Y);
            Vector velAgent = new Vector(myself.VelX, myself.VelY);
            double angBodyAgent = myself.BodyDirection;

            Vector posPred = predictFinalAgentPos();
            double angTo = (posTurnTo - posPred).Theta * 180.0 / Math.PI;
            double ang = normalizeAngle(angTo - angBodyAgent);

            int iTurn = 0;
            while (Math.Abs(ang) > angWhenToTurn && iTurn < 5)
            {
                iTurn++;
                predictStateAfterTurn(
                      getAngleForTurn(ang, velAgent.Radius, myself),
                      posAgent,
                      velAgent,
                      ref angBodyAgent, myself);
                ang = normalizeAngle(angTo - angBodyAgent);
            }

            posAgent = new Vector(myself.X, myself.Y);
            velAgent = new Vector(myself.VelX, myself.VelY);
            angBodyAgent = myself.BodyDirection;
            Command com = new Command();
            com.Com = "illegal";
            switch (iTurn)
            {
                case 0: return com;
                case 1:
                case 2: return turnBodyToPoint(posTurnTo, 2);
                default: return dashToPoint(posAgent, 1);  // stop
            }
        }

        public Vector predictFinalAgentPos()
        {

            Vector posAgent = new Vector(myself.X, myself.Y);
            Vector velAgent = new Vector(myself.VelX, myself.VelY);
            double dDistExtra = getSumInfGeomSeries(velAgent.Radius, serverParameters.Player_decay);
            return posAgent + Vector.fromPolar((float)dDistExtra, (float)velAgent.Theta);

        }

        public static double getSumInfGeomSeries(double dFirst, double dRatio)
        {

            // s = a(1-r^n)/(1-r) with n->inf and 0<r<1 => r^n = 0
            return dFirst / (1 - dRatio);
        }

        public Command turnBackToPoint(Vector pos)
        {
            int iCycles = 1;
            Vector posGlobal = predictAgentPos(iCycles, 0);
            double angTurn = (pos - posGlobal).Theta * 180.0 / Math.PI;
            angTurn -= (myself.BodyDirection + 180);
            angTurn = normalizeAngle(angTurn);
            angTurn = getAngleForTurn(angTurn, AgentSpeed, myself);
            Command com = new Command();
            com.Com = "turn";
            com.powAng = angTurn;
            return com;
        }

        public Vector leastCongestedPointForPassInRectangle(RoboCup.Geometry.Rectangle rect, Vector posFrom)
        {
            int x_granularity = 5; // 5 samples by 5 samples
            int y_granularity = 5;

            double x_buffer = 0.15; // 15% border on each side
            double y_buffer = 0.15;

            double x_mesh = serverParameters.Keepaway_Length * (1 - 2 * x_buffer) / (x_granularity - 1);
            double y_mesh = serverParameters.Keepaway_Width * (1 - 2 * y_buffer) / (y_granularity - 1);

            double start_x = -serverParameters.Keepaway_Length / 2 + x_buffer * serverParameters.Keepaway_Length;
            double start_y = -serverParameters.Keepaway_Width / 2 + y_buffer * serverParameters.Keepaway_Width;

            double x = start_x, y = start_y;

            double best_congestion = 1000;
            Vector best_point = new Vector(0, 0), point = new Vector(0, 0);
            double tmp;

            for (int i = 0; i < x_granularity; i++)
            {
                for (int j = 0; j < y_granularity; j++)
                {
                    tmp = congestion(point = new Vector((float)x, (float)y));

                    if (tmp < best_congestion && getNrInSetInCone(opp, 0.3, posFrom, point) == 0)
                    {
                        best_congestion = tmp;
                        best_point = point;
                    }
                    y += y_mesh;
                }
                x += x_mesh;
                y = start_y;
            }

            if (best_congestion == 1000)
            {
                /* take the point out of the rectangle -- meaning no point was valid */
                best_point = new Vector(0, 0);
            }

            return best_point;
        }

        public double congestion(Vector pos)
        {
            bool considerMe = false;
            double congest = 0;
            if (considerMe && pos != mySelfp)
                congest = 1 / mySelfp.distance(pos);

            Vector playerPos = new Vector(0, 0);

            foreach (var p in FullState.Players)
            {
                playerPos.X = p.Value.X;
                playerPos.Y = p.Value.Y;
                if (playerPos != pos)
                    if (p.Value.Team != Team || p.Value.Unum != Unum)
                        /* Don't want to count a player in its own congestion measure */
                        congest += 1 / playerPos.distance(pos);
            }

            return congest;
        }

        public int getNrInSetInCone(Vector[] set, double dWidth, Vector start, Vector end)
        {


            int iNr = 0;

            Line line = Line.makeLineFromTwoPoints(start, end);
            Vector posOnLine = new Vector(0, 0);
            Vector posObj = new Vector(0, 0);

            for (int i = 0; i < set.Length; i++)
            {
                posObj.X = set[i].X;
                posObj.Y = set[i].Y;
                posOnLine = line.getPointOnLineClosestTo(posObj);
                // whether posOnLine lies in cone is checked by three constraints
                // - does it lie in triangle (to infinity)
                // - lies between start and end (and thus not behind me)
                // - does it lie in circle
                if (posOnLine.distance(posObj) < dWidth * posOnLine.distance(start)
                   && line.isInBetween(posOnLine, start, end)
                   && start.distance(posObj) < start.distance(end))
                    iNr++;
            }

            return iNr;
        }

        public int predictNrCyclesToBall(FullPlayerData objFrom, Vector objTo)
        {
            Vector posPrev = new Vector(-1000, -1000);

            if (objTo.distance(new Vector(objFrom.X, objFrom.Y)) > 40)
            {
                return 101;

            }

            // in case of ball with no velocity, calculate cycles to point
            if (BallSpeed < 0.01 || trajectoryChanges > 1)
                return predictNrCyclesToPoint(objFrom, ballp);

            int iCycles = 0;
            int iCyclesToObj = 100;
            Vector posObj = new Vector(objTo.X, objTo.Y);

            // continue calculating number of cycles to position until or we can get
            // earlier at object position, are past maximum allowed number of cycles or
            // the object does not move anymore.
            while (iCycles <= iCyclesToObj && iCycles < 30 &&
                   posObj.distance(posPrev) > 10e-3)
            {
                iCycles = iCycles + 1;
                posPrev = posObj;
                posObj = predictBallPosAfterNrCycles(iCycles);

                if (posObj.distance(new Vector(objFrom.X, objFrom.Y)) / serverParameters.Player_speed_max < iCycles + 1)
                {
                    iCyclesToObj = predictNrCyclesToPoint(objFrom, posObj);
                }
            }

            return iCyclesToObj;
        }


        public void intercept()
        {
            Command soc = interceptClose(), soc2 = new Command();
            soc2.Com = "illegal";
            Vector pos = new Vector(myself.X, myself.Y);


            int cycles = 0;
            soc2 = predictCommandToInterceptBall(myself, soc);
            if (soc2.Com == "illegal")
            {
                soc2 = turnBodyToPoint(pos + Vector.fromPolar(1.0f, 0), 1);
            }

            if (turns > 1)
            {
                if (soc.Com == "illegal" || turns > 2)
                {
                    soc2 = moveToPos(prevBallPos, 7.0);

                }
                else
                {
                    soc2 = soc;
                }
            }

            if (soc2.Com == "dash")
            {
                dash((float)soc2.powAng);
                turns = 0;
            }
            else if (soc2.Com == "turn")
            {
                turn((float)soc2.powAng);
                turns++;
            }

            if (turns < 2)
            {
                prevBallPos.assign(ballp);

            }

        }


        public Command predictCommandToInterceptBallOld(FullPlayerData obj, Command socClose)
        {
            Vector posIntercept = null;
            Vector posIn = null;
            Vector velIn = null;
            Vector temp;

            // declare all needed variables
            Command soc = new Command();
            soc.Com = "illegal";
            Vector pos = new Vector(0, 0), vel = new Vector(0, 0), posPred = new Vector(0, 0), posBall = new Vector(0, 0), posBallTmp = new Vector(0, 0), velBall = new Vector(0, 0), posAgent = new Vector(0, 0);
            double angBody, angNeck = 0;
            int iMinCyclesBall = 100, iFirstBall = 100;
            double dMaxDist = MaximalKickDistance;
            double dBestX = -1000;

            double dMinOldIntercept = 100, dDistanceOfIntercept = 10.0;
            int iOldIntercept = -1000;


            if (body.Time - timeLastIntercepted > 2)
                posOldIntercept.assign(-1000, -1000);

            timeLastIntercepted = body.Time;
            int iCyclesBall = 0;



            // for each new pos of the ball, check whether agent can reach ball 
            // and update the best interception point
            while (iCyclesBall <= 30 &&
                   iCyclesBall <= iFirstBall + 20 &&
                   isInField(posBall, 10) == true)
            {
                // re-initialize all variables
                angBody = obj.BodyDirection;

                pos = ((object)posIn == null) ? new Vector(obj.X, obj.Y) : new Vector(posIn.X, posIn.Y);
                vel = ((object)velIn == null) ? new Vector(obj.VelX, obj.VelY) : new Vector(velIn.X, velIn.Y);
                soc.Com = "illegal";

                // predict the ball position after iCycles and from that its velocity 
                posBallTmp = predictBallPosAfterNrCycles(iCyclesBall);
                if (iCyclesBall == 0)
                    velBall = new Vector(ball.VelX, ball.VelY);
                else
                    velBall = posBallTmp - posBall;
                posBall = posBallTmp;

                // predict the agent position 
                posPred = predictPlayerPosAfterNrCycles(obj, Math.Min(iCyclesBall, 4), 0, null, null, false);
                posAgent = new Vector(obj.X, obj.Y);

                // if too far away, we can never reach it and try next cycle
                if (posPred.distance(posBall) / serverParameters.Player_speed_max
                      > iCyclesBall + dMaxDist || isInField(posBall, 10) == false)
                {
                    iCyclesBall++;
                    continue;
                }

                // predict our position after the same nr of cycles when intercepting 
                for (int i = 0; i < iCyclesBall; i++)
                {

                    soc = (predictCommandToMoveToPos(obj, posBall, iCyclesBall - i,
                            2.5, false, pos, vel, angBody));
                    predictStateAfterCommand(soc, pos, vel, ref angBody, ref angNeck, obj);
                }

                // if in kickable distance, we can reach the ball!
                if (pos.distance(posBall) < dMaxDist)
                {


                    if (iMinCyclesBall == 100) // log first possible interception point
                        iFirstBall = iMinCyclesBall = iCyclesBall;

                    // too get some consistency in the interception point and avoid
                    // too many turns, also keep track of the current possible
                    // interception point.  This is the point close to the old
                    // interception point. Two constraints are that the ball has to
                    // have some speed (else it does not really matter where to
                    // intercept) and the ball must be intercepted safely, that is
                    // the ball is close to the body when intercepting. 
                    if (posBall.distance(posOldIntercept) <
                                         Math.Min(1.0, dMinOldIntercept) &&
                        pos.distance(posBall) < 0.70 * MaximalKickDistance &&
                        velBall.Radius > 0.6)
                    {
                        dBestX = posBall.X;
                        iOldIntercept = iCyclesBall;
                        dDistanceOfIntercept = pos.distance(posBall);
                        dMinOldIntercept = posBall.distance(posOldIntercept);
                    }
                    // determine the safest interception point. This point must be
                    // better than the current intercept, the distance to ball must
                    // be very small after interception and close to the previous
                    // calculated interception point
                    else if (pos.distance(posBall) < dDistanceOfIntercept &&
                             dDistanceOfIntercept > 0.50 * MaximalKickDistance &&
                             (iCyclesBall <= iMinCyclesBall + 3 ||
                               iCyclesBall <= iOldIntercept + 3) &&
                             Math.Abs(posBall.Y) < 32.0 &&
                             Math.Abs(posBall.X) < 50.0)
                    {
                        iMinCyclesBall = iCyclesBall;
                        dDistanceOfIntercept = pos.distance(posBall);

                        if (iOldIntercept == iMinCyclesBall - 1)
                        {

                            iOldIntercept = iMinCyclesBall;
                        }
                    }
                }


                iCyclesBall++;
            }



            // check special situations where we move to special position.
            if (!(iMinCyclesBall > iOldIntercept + 2) &&
                iOldIntercept != -1000)
            {

                iMinCyclesBall = iOldIntercept;
            }
            else
            {

                iMinCyclesBall = iFirstBall;
            }

            posBall = predictBallPosAfterNrCycles(iMinCyclesBall);


            posOldIntercept.assign(posBall.X, posBall.Y);
            posPred = predictPlayerPosAfterNrCycles(obj, Math.Min(iMinCyclesBall, 4), 0, null, null, false);

            Vector temp2 = posBall;
            if ((object)posIntercept != null)
                posIntercept.assign(temp2);

            if (iMinCyclesBall < 3 && socClose.Com != "illegal")
            {

                iMinCyclesBall = 1;
                soc = (socClose);
            }
            else if (posPred.distance(posBall) < 0.5)
            {

                soc.Com = "illegal";
            }
            else
            {
                Vector v = new Vector(0, 0);
                Vector p = new Vector(0, 0);
                double q = -1000;

                soc = predictCommandToMoveToPos(obj, posBall, iMinCyclesBall, 2.5, false, new Vector(obj.X, obj.Y), new Vector(obj.VelX, obj.VelY), obj.BodyDirection);
            }

            return soc;
        }

        public Command predictCommandToInterceptBall(FullPlayerData obj, Command socClose)
        {
            Vector posIntercept = null;
            Vector posIn = null;
            Vector velIn = null;
            Vector temp;

            // declare all needed variables
            Command soc = new Command();
            soc.Com = "illegal";
            Vector pos = new Vector(0, 0), vel = new Vector(0, 0), posPred = new Vector(0, 0), posBall = new Vector(0, 0), posBallTmp = new Vector(0, 0), velBall = new Vector(0, 0), posAgent = new Vector(0, 0);
            double angBody, angNeck = 0;
            int iMinCyclesBall = 100, iFirstBall = 100;
            double dMaxDist = MaximalKickDistance;
            double dBestX = -1000;

            double dMinOldIntercept = 100, dDistanceOfIntercept = 10.0;
            int iOldIntercept = -1000;

            if (body.Time - 1 == timeLastIntercepted && (trajectoryChanges > 2 || trajectorySame > 2 || (trajectorySame == 0 && trajectoryChanges == 0)))
            {
                //  return moveToPos(posOldIntercept, 7);

            }
            if (body.Time - timeLastIntercepted > 2)
                posOldIntercept.assign(-1000, -1000);

            timeLastIntercepted = body.Time;
            int iCyclesBall = 0;



            // for each new pos of the ball, check whether agent can reach ball 
            // and update the best interception point
            while (iCyclesBall <= 30 &&
                   iCyclesBall <= iFirstBall + 20 &&
                   isInField(posBall, 10) == true)
            {
                // re-initialize all variables
                angBody = obj.BodyDirection;

                pos = ((object)posIn == null) ? new Vector(obj.X, obj.Y) : new Vector(posIn.X, posIn.Y);
                vel = ((object)velIn == null) ? new Vector(obj.VelX, obj.VelY) : new Vector(velIn.X, velIn.Y);
                soc.Com = "illegal";

                // predict the ball position after iCycles and from that its velocity 
                posBallTmp = predictBallPosAfterNrCycles(iCyclesBall);
                if (iCyclesBall == 0)
                    velBall = new Vector(ball.VelX, ball.VelY);
                else
                    velBall = posBallTmp - posBall;
                posBall = posBallTmp;

                // predict the agent position 
                posPred = predictPlayerPosAfterNrCycles(obj, Math.Min(iCyclesBall, 4), 0, null, null, false);
                posAgent = new Vector(obj.X, obj.Y);

                // if too far away, we can never reach it and try next cycle
                if (posPred.distance(posBall) / serverParameters.Player_speed_max
                      > iCyclesBall + dMaxDist || isInField(posBall, 10) == false)
                {
                    iCyclesBall++;
                    continue;
                }

                // predict our position after the same nr of cycles when intercepting 
                for (int i = 0; i < iCyclesBall; i++)
                {

                    soc = (predictCommandToMoveToPos(obj, posBall, iCyclesBall - i,
                            2.5, false, pos, vel, angBody));
                    predictStateAfterCommand(soc, pos, vel, ref angBody, ref angNeck, obj);
                }

                // if in kickable distance, we can reach the ball!
                if (pos.distance(posBall) < dMaxDist)
                {


                    if (iMinCyclesBall == 100) // log first possible interception point
                        iFirstBall = iMinCyclesBall = iCyclesBall;

                    // too get some consistency in the interception point and avoid
                    // too many turns, also keep track of the current possible
                    // interception point.  This is the point close to the old
                    // interception point. Two constraints are that the ball has to
                    // have some speed (else it does not really matter where to
                    // intercept) and the ball must be intercepted safely, that is
                    // the ball is close to the body when intercepting. 
                    if (posBall.distance(posOldIntercept) <
                                         Math.Min(1.0, dMinOldIntercept) &&
                        pos.distance(posBall) < 0.70 * MaximalKickDistance &&
                        velBall.Radius > 0.6)
                    {
                        dBestX = posBall.X;
                        iOldIntercept = iCyclesBall;
                        dDistanceOfIntercept = pos.distance(posBall);
                        dMinOldIntercept = posBall.distance(posOldIntercept);
                    }
                    // determine the safest interception point. This point must be
                    // better than the current intercept, the distance to ball must
                    // be very small after interception and close to the previous
                    // calculated interception point
                    else if (pos.distance(posBall) < dDistanceOfIntercept &&
                             dDistanceOfIntercept > 0.50 * MaximalKickDistance &&
                             (iCyclesBall <= iMinCyclesBall + 3 ||
                               iCyclesBall <= iOldIntercept + 3) &&
                             Math.Abs(posBall.Y) < 32.0 &&
                             Math.Abs(posBall.X) < 50.0)
                    {
                        iMinCyclesBall = iCyclesBall;
                        dDistanceOfIntercept = pos.distance(posBall);

                        if (iOldIntercept == iMinCyclesBall - 1)
                        {

                            iOldIntercept = iMinCyclesBall;
                        }
                    }
                }


                iCyclesBall++;
            }



            // check special situations where we move to special position.
            if (!(iMinCyclesBall > iOldIntercept + 2) &&
                iOldIntercept != -1000)
            {

                iMinCyclesBall = iOldIntercept;
            }
            else
            {

                iMinCyclesBall = iFirstBall;
            }

            posBall = predictBallPosAfterNrCycles(iMinCyclesBall);


            posOldIntercept.assign(posBall.X, posBall.Y);
            posPred = predictPlayerPosAfterNrCycles(obj, Math.Min(iMinCyclesBall, 4), 0, null, null, false);

            Vector temp2 = posBall;
            if ((object)posIntercept != null)
                posIntercept.assign(temp2);

            if (iMinCyclesBall < 3 && socClose.Com != "illegal")
            {

                iMinCyclesBall = 1;
                soc = (socClose);
            }
            else if (posPred.distance(posBall) < 0.5)
            {

                soc.Com = "illegal";
            }
            else
            {
                Vector v = new Vector(0, 0);
                Vector p = new Vector(0, 0);
                double q = -1000;

                soc = predictCommandToMoveToPos(obj, posBall, iMinCyclesBall, 2.5, false, new Vector(obj.X, obj.Y), new Vector(obj.VelX, obj.VelY), obj.BodyDirection);
            }

            return soc;
        }

        public bool isInField(Vector pos, double dMargin)
        {
            RoboCup.Geometry.Rectangle rect = new Rectangle(new Vector(0, 0), new Vector(serverParameters.Keepaway_Length, serverParameters.Keepaway_Width));
            return rect.ContainsPoint(pos);
        }

        public Command interceptClose()
        {

            
            Command soc = new Command(), socDash1 = new Command(), socFinal = new Command(), socCollide = new Command(), socTurn = new Command();
            soc.Com = "illegal";
            socDash1.Com = "illegal";
            socFinal.Com = "illegal";
            socCollide.Com = "illegal";
            socTurn.Com = "illegal";
            double dPower, dDist;
            double ang = 0, ang2 = 0;
            Vector s1 = new Vector(0, 0), s2 = new Vector(0, 0);
            bool bReady = false;

            // first determine whether the distance to the ball is not too large
            dDist = 3 * serverParameters.Player_speed_max
                    + (1.0 + serverParameters.Ball_decay) * serverParameters.Ball_speed_max
                    + MaximalKickDistance;
            if (mySelfp.distance(ballp) > dDist)
            {
                bReady = true;

            }

            socCollide = collideWithBall();
            // initialize all variables with information from the worldmodel.
            Vector posAgent = new Vector(mySelfp.X, mySelfp.Y);
            Vector posPred = predictAgentPos(1, 0), posDash1 = new Vector(0, 0);
            Vector posBall = predictBallPosAfterNrCycles(1);
            Vector velMe = new Vector(myself.VelX, myself.VelY);

            double angBody = myself.BodyDirection, angTurn, angNeck = 0;
            double dDesBody = 0.0;


            // get the distance to the closest opponent
            double dDistOpp = 0;
            int objOpp = getClosestInSetTo(opp, mySelfp, ref dDistOpp);
            angTurn = normalizeAngle(dDesBody - myself.BodyDirection);

            // check the distance to the ball when we do not dash (e.g., and only turn)
            posBall = predictBallPosAfterNrCycles(1);
            Vector posPred1 = predictAgentPos(1, 0);
            double dDist1 = posPred1.distance(posBall);
            posBall = predictBallPosAfterNrCycles(2);
            Vector posPred2 = predictAgentPos(2, 0);
            double dDist2 = posPred2.distance(posBall);
            posBall = predictBallPosAfterNrCycles(3);
            Vector posPred3 = predictAgentPos(3, 0);
            double dDist3 = posPred3.distance(posBall);


            double angThreshold = 25;
            bool bOppClose = (dDistOpp < 3.0);

            // make a line from center of body in next cycle with direction of body
            // use next cycle since current velocity is always propogated to position in
            // next cycle.  Make a circle around the ball with a radius equal to the
            // sum of your own body, the ball size and a small buffer. Then calculate
            // the intersection between the line and this circle. These are the (two)
            // points that denote the possible agent locations close to the ball
            // From these two points we take the point where the body direction of the
            // agent makes the smallest angle with the ball (with backward
            // dashing we sometime have to dash "over" the ball to face it up front)
            posAgent = new Vector(mySelfp.X, mySelfp.Y);
            posBall = predictBallPosAfterNrCycles(1);
            angBody = myself.BodyDirection;
            velMe = new Vector(myself.VelX, myself.VelY);

            Line line = Line.makeLineFromPositionAndAngle(posPred1, angBody);
            dDist = MaximalKickDistance / 6;
            int iSol = line.getCircleIntersectionPoints(new Circle(posBall, (float)dDist), s1, s2);
            if (iSol > 0)                                          // if a solution
            {
                if (iSol == 2)                                       // take the best one
                {
                    ang = normalizeAngle((posBall - s1).Theta * 180.0 / Math.PI - angBody);
                    ang2 = normalizeAngle((posBall - s2).Theta * 180.0 / Math.PI - angBody);
                    //     if ( fabs(ang2) < 90)
                    if (s2.X > s1.X) // move as much forward as possible
                        s1 = s2;                                          // and put it in s1
                }

                // try one dash
                // now we have the interception point we try to reach in one cycle. We
                // calculate the needed dash power from the current position to this point,
                // predict were we will stand if we execute this command and check whether
                // we are in the kickable distance
                dPower = getPowerForDash(s1 - posAgent, angBody, velMe, 1.0, 1);
                posDash1 = predictAgentPos(1, (int)dPower);
                if (posDash1.distance(posBall) < 0.95 * MaximalKickDistance)
                {

                    socDash1.Com = "dash"; socDash1.powAng = dPower;
                }
                else
                {
                    dPower = getPowerForDash(s2 - posAgent, angBody, velMe, 1.0, 1);
                    posDash1 = predictAgentPos(1, (int)dPower);
                    if (posDash1.distance(posBall) < 0.95 * MaximalKickDistance)
                    {

                        socDash1.Com = "dash"; socDash1.powAng = dPower;
                    }
                }
            }

            // try one dash by getting close to ball
            // this handles situation where ball cannot be reached within distance
            // SS.getKickableMargin()/6
            if (socDash1.Com == "illegal")
            {
                soc = dashToPoint(posBall, 1);
                predictAgentStateAfterCommand(soc, posDash1, velMe, ref angBody);
                if (posDash1.distance(posBall) < 0.95 * MaximalKickDistance)
                {

                    socDash1 = soc;
                }
            }

            if (bReady != true)
            {
                if (bOppClose && socDash1.Com != "illegal")
                {

                    socFinal = (socDash1);
                }
                else
                {
                    soc = turnBodyToPoint(posPred1 + Vector.fromPolar(1, (float)(dDesBody * Math.PI / 180.0)), 1);
                    predictAgentStateAfterCommand(soc, posPred, velMe, ref angBody);
                    posBall = predictBallPosAfterNrCycles(1);
                    if (posPred.distance(posBall) < 0.8 * MaximalKickDistance)
                    {
                        socTurn = soc; // we can do turn and end up ok, but can maybe improve
                        ang = normalizeAngle(dDesBody - angBody);
                        if (Math.Abs(ang) < angThreshold)
                        {
                            socFinal = soc;

                        }

                    }
                    if (socFinal.Com == "illegal")
                    {
                        ang = normalizeAngle(dDesBody - angBody);
                        predictStateAfterTurn(getAngleForTurn(ang, velMe.Radius, myself), posPred, velMe, ref angBody, myself);
                        posBall = predictBallPosAfterNrCycles(2);
                        if (posPred.distance(posBall) < 0.8 * MaximalKickDistance)
                        {
                            socTurn = soc; // we can do turn and end up ok, but can maybe improve
                            ang = normalizeAngle(dDesBody - angBody);
                            if (Math.Abs(ang) < angThreshold)
                            {

                                socFinal = (soc);
                            }
                        }
                    }
                    if (socFinal.Com == "illegal" && socCollide.Com != "illegal" &&
                        Math.Abs(angTurn) > angThreshold)
                    {

                        posBall = predictBallPosAfterNrCycles(1);
                        socFinal = (socCollide);
                    }
                    if (socFinal.Com == "illegal" && Math.Abs(angTurn) > angThreshold)
                    {
                        posBall = predictBallPosAfterNrCycles(2);
                        soc = dashToPoint(posAgent, 1);
                        predictAgentStateAfterCommand(soc, posPred, velMe, ref angBody);
                        if (posPred.distance(posBall) < 0.8 * MaximalKickDistance)
                        {

                            socFinal = (soc);
                        }
                    }
                    if (socFinal.Com == "illegal" && socTurn.Com != "illegal")
                    {

                        socFinal = (socTurn);
                    }
                    if (socFinal.Com == "illegal" && socDash1.Com != "illegal")
                    {

                        socFinal = (socDash1);
                    }
                }

            }
            // if there are no opponents, we are wrongly directed, and we will be closely
            // to the ball, see whether we can first update our heading
            else if (Math.Abs(angTurn) > angThreshold && !bOppClose &&
                dDist1 < 0.7 * MaximalKickDistance)
            {
                soc = turnBodyToPoint(posPred1 + Vector.fromPolar(1, (float)(dDesBody * Math.PI / 180.0)), 1);

                socFinal = (soc);
            }
            else if ( // fabs( angTurn ) > angThreshold &&
                !bOppClose &&
                dDist2 < 0.7 * MaximalKickDistance)
            {
                soc = turnBodyToPoint(posPred2 + Vector.fromPolar(1, (float)(dDesBody * Math.PI / 180.0)), 2);

                socFinal = (soc);
            }


            else if (socCollide.Com != "illegal" &&
                     Math.Abs(angTurn) > angThreshold)
            {

                socFinal = (socCollide);
            }
            else if (socDash1.Com != "illegal")
            {
                socFinal = (socDash1);
            }

            return socFinal;
        }

        public int getClosestInSetTo(Vector[] set, Vector pos, ref double dDist)
        {
            dDist = 10000;
            int closest = 0;
            for (int i = 0; i < set.Length; i++)
            {
                if (dDist > set[i].distance(pos))
                {
                    dDist = set[i].distance(pos);
                    closest = i + 1;
                }

            }

            return closest;
        }

        
        public double BallSpeed { get { return Math.Sqrt(ball.VelX * ball.VelX + ball.VelY * ball.VelY); } }

        public Command collideWithBall()
        {
            Command soc = new Command();
            soc.Com = "illegal";
            if (mySelfp.distance(ballp) >
                BallSpeed + serverParameters.Player_speed_max)
                return soc;


            Vector posBallPred = predictBallPosAfterNrCycles(1);

            // first try turn
            soc = turnBodyToPoint(mySelfp + Vector.fromPolar(1, 0));
            Vector posAgentPred = predictAgentPosAfterCommand(soc);
            if (posAgentPred.distance(posBallPred) <
                serverParameters.Ball_size + serverParameters.Player_size)
            {

                return soc;
            }

            soc = dashToPoint(posBallPred, 1);
            posAgentPred = predictAgentPosAfterCommand(soc);
            if (posAgentPred.distance(posBallPred) <
                serverParameters.Ball_size + serverParameters.Player_size)
            {

                return soc;
            }

            soc.Com = "illegal";
            return soc;
        }

        public Command dashToPoint(Vector pos, int iCycles)
        {
            double dDashPower = getPowerForDash(
                                           pos - mySelfp,
                                           myself.BodyDirection,
                                           new Vector(myself.VelX, myself.VelY),
                                           1.0,
                                           iCycles);
            Command com = new Command();
            com.Com = "dash";
            com.powAng = dDashPower;
            return com;
        }

        public Vector predictAgentPosAfterCommand(Command com)
        {
            Vector p1 = new Vector(0, 0), p2 = new Vector(0, 0);
            double a1 = 0;
            predictAgentStateAfterCommand(com, p1, p2, ref a1);
            return p1;
        }

        public bool predictAgentStateAfterCommand(Command com, Vector pos, Vector vel, ref double angGlobalBody)
        {
            Vector temp = new Vector(myself.X, myself.Y);
            pos.assign(temp);
            temp = new Vector(myself.VelX, myself.VelY);
            vel.assign(temp);
            angGlobalBody = myself.BodyDirection;
            predictStateAfterCommand(com, pos, vel, ref angGlobalBody, myself);

            return true;
        }

        public bool predictStateAfterCommand(Command com, Vector pos, Vector vel, ref double angGlobalBody, FullPlayerData obj)
        {
            switch (com.Com) // based on kind of command, choose action
            {
                case "dash":
                    predictStateAfterDash(com.powAng, pos, vel, angGlobalBody, obj);
                    break;
                case "turn":
                    predictStateAfterTurn(com.powAng, pos, vel, ref angGlobalBody, obj);
                    break;
                case "illegal":
                    predictStateAfterDash(0.01, pos, vel, angGlobalBody, obj);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public double AgentSpeed { get { return Math.Sqrt(myself.VelX * myself.VelX + myself.VelY * myself.VelY); } }

        public Command turnBodyToPoint(Vector pos)
        {
            int iCycles = 1;
            Vector posGlobal = predictAgentPos(iCycles, 0);
            double angTurn = (pos - posGlobal).Theta * 180.0 / Math.PI;
            angTurn -= (myself.BodyDirection);
            angTurn = normalizeAngle(angTurn);
            angTurn = getAngleForTurn(angTurn, AgentSpeed, myself);

            Command com = new Command();
            com.Com = "turn";
            com.powAng = angTurn;
            return com;
        }

        public Command turnBodyToPoint(Vector pos, int iCycles)
        {

            Vector posGlobal = predictAgentPos(iCycles, 0);
            double angTurn = (pos - posGlobal).Theta * 180.0 / Math.PI;
            angTurn -= (myself.BodyDirection);
            angTurn = normalizeAngle(angTurn);
            angTurn = getAngleForTurn(angTurn, AgentSpeed, myself);

            Command com = new Command();
            com.Com = "turn";
            com.powAng = angTurn;
            return com;
        }

        public Vector predictAgentPos(int iCycles, int iDashPower)
        {

            return predictPlayerPosAfterNrCycles(myself, iCycles, iDashPower, null, null, false);
        }

        public void markMostOpenOpponent(int withBall)
        {
            Vector posFrom = new Vector(0, 0);
            if (withBall >= 0)
            {
                posFrom.assign(opp[withBall - 1].X, opp[withBall - 1].Y);
            }
            else
            {
                posFrom.assign(ballp);
            }


            int minPlayer = withBall;
            int min = teammatesp.Length;
            foreach (var p in opponents)
            {
                Vector point = new Vector(opp[p.Value.Unum - 1].X, opp[p.Value.Unum - 1].Y);
                // if player is on sidelines, skip
                if (Math.Abs(point.Y) == 37 || ballp.distance(point) < 3)
                    continue;
                int num = getNrInSetInCone(teammatesp,
                                0.3, posFrom, point);
                if (num < min)
                {
                    min = num;
                    minPlayer = p.Value.Unum;
                }
            }

            mark(opponents[minPlayer], 4.0);

            return;
        }

        public void mark(FullPlayerData o, double dDist)
        {
            Vector posMark = getMarkingPosition(o, dDist);
            Vector posAgent = new Vector(mySelfp.X, mySelfp.Y);
            Vector posBall = new Vector(ballp.X, ballp.Y);
            //  AngDeg      angBody  = WM->getAgentGlobalBodyAngle();
            Command soc;

            if (posAgent.distance(posMark) < 2.0)
            {
                double angOpp = (new Vector(o.X, o.Y) - posAgent).Theta * 180.0 / Math.PI;
                double angBall = (posBall - posAgent).Theta * 180.0 / Math.PI; ;
                if (isAngInInterval(angBall, angOpp, normalizeAngle(angOpp + 180)))
                    angOpp += 80;
                else
                    angOpp -= 80;
                angOpp = normalizeAngle(angOpp);

                soc = turnBodyToPoint(posAgent + Vector.fromPolar(1.0f, (float)(angOpp * Math.PI / 180.0)));
                if (soc.Com == "turn")
                {
                    turn((float)soc.powAng);
                }
                else if (soc.Com == "dash")
                {

                    dash((float)soc.powAng);
                }
                return;
            }


            soc = moveToPos(posMark, 25, 3.0, false);

            if (soc.Com == "turn")
            {
                turn((float)soc.powAng);
            }
            else if (soc.Com == "dash")
            {

                dash((float)soc.powAng);
            }

            return;
        }

        public Command moveToPos(Vector posTo, double angWhenToTurn, double dDistBack, bool bMoveBack)
        {
            int iCycles = 1;
            // previously we only turned relative to position in next cycle, now take
            // angle relative to position when you're totally rolled out...
            //  VecPosition posPred   = WM.predictAgentPos( 1, 0 );

            Vector posAgent = new Vector(myself.X, myself.Y);
            Vector posPred = predictFinalAgentPos();

            double angBody = myself.BodyDirection;
            double angTo = (posTo - posPred).Theta * 180.0 / Math.PI;
            angTo = normalizeAngle(angTo - angBody);
            double angBackTo = normalizeAngle(angTo + 180);

            double dDist = posAgent.distance(posTo);

            if (bMoveBack)
            {
                if (Math.Abs(angBackTo) < angWhenToTurn)
                    return dashToPoint(posTo, iCycles);
                else
                    return turnBackToPoint(posTo);
            }
            else if (Math.Abs(angTo) < angWhenToTurn ||
                     (Math.Abs(angBackTo) < angWhenToTurn && dDist < dDistBack))
                return dashToPoint(posTo, iCycles);
            else
                return directTowards(posTo, angWhenToTurn);
            //return turnBodyToPoint( posTo );
        }

        public static bool isAngInInterval(double ang, double angMin, double angMax)
        {
            // convert all angles to interval 0..360
            if ((ang + 360) < 360) ang += 360;
            if ((angMin + 360) < 360) angMin += 360;
            if ((angMax + 360) < 360) angMax += 360;

            if (angMin < angMax) // 0 ---false-- angMin ---true-----angMax---false--360
                return angMin < ang && ang < angMax;
            else                  // 0 ---true--- angMax ---false----angMin---true---360
                return !(angMax < ang && ang < angMin);
        }

        public Vector getMarkingPosition(FullPlayerData o, double dDist)
        {
            Vector pos = new Vector(o.X, o.Y);
            // except on back line assume players is moving to goalline
            if (pos.X > -52.5 + 4.0)
                pos -= new Vector(1.0f, 0.0f);

            return getMarkingPosition(pos, dDist);

        }

        public Vector getMarkingPosition(Vector pos, double dDist)
        {
            Vector posBall = new Vector(ballp.X, ballp.Y);
            //edictPosAfterNrCycles( OBJECT_BALL, 3 );

            Vector posAgent = new Vector(myself.X, myself.Y);
            Vector posMark = new Vector(0, 0);
            double ang, angToBall;



            angToBall = (posBall - pos).Theta;
            posMark = pos + Vector.fromPolar((float)dDist, (float)angToBall);

            return posMark;

        }

        public void holdBall(double dDistThr)
        {
            double dDist = 0;
            Vector posAgent = new Vector(myself.X, myself.Y);
            int objOpp = getClosestInSetTo(opp, ballp, ref dDist);
            Vector posOpp = new Vector(opponents[objOpp].X, opponents[objOpp].Y);
            double angOpp = opponents[objOpp].BodyDirection;
            double ang = 0.0;

            if (dDist < 5)
            {
                // get the angle between the object to the agent
                // check whether object is headed to the left or right of this line
                ang = (posAgent - posOpp).Theta * 180.0 / Math.PI;
                int iSign = -((normalizeAngle(angOpp - ang) >= 0 ? 1 : -1));
                ang += iSign * 45 - myself.BodyDirection;
                ang = normalizeAngle(ang);

            }


            if (ballp.distance(posAgent + Vector.fromPolar((float)dDistThr, (float)(ang * Math.PI / 180.0)))
                < 0.3)
            {

                Vector goalPosL = new Vector(-52.5f, 0.0f);
                Vector goalPosR = new Vector(52.5f, 0);

                Command soc = turnBodyToPoint((this.Side == "r" ? goalPosL : goalPosR), 1);
                Vector posBallPred = predictBallPosAfterNrCycles(1);
                Vector posPred = predictAgentPosAfterCommand(soc);
                if (posPred.distance(posBallPred) < 0.85 * MaximalKickDistance)
                {

                    if (soc.Com == "turn")
                    {
                        turn((float)soc.powAng);
                    }
                    else if (soc.Com == "dash")
                    {
                        dash((float)soc.powAng);
                    }

                    return;
                }
            }

            kickBallCloseToBody(ang, dDistThr);
            return;
        }

        public void kickBallCloseToBody(double ang, double dKickRatio)
        {
            double angBody = myself.BodyDirection;
            Vector posAgent = predictAgentPos(1, 0);
            double dDist = serverParameters.Player_size +
                                     serverParameters.Ball_size +
                                     serverParameters.Kickable_margin * dKickRatio;
            double angGlobal = normalizeAngle(angBody + ang);
            Vector posDesBall = posAgent + Vector.fromPolar((float)dDist, (float)(angGlobal * Math.PI / 180.0));
            if (Math.Abs(posDesBall.Y) > 34 ||
                Math.Abs(posDesBall.X) > 52.5)
            {
                Line lineBody = Line.makeLineFromPositionAndAngle(posAgent, angGlobal);
                Line lineSide = new Line(0, 0, 0);
                if (Math.Abs(posDesBall.Y) > 34 / 2.0)
                    lineSide = Line.makeLineFromPositionAndAngle(
                        new Vector(0, (posDesBall.Y >= 0 ? 1 : -1) * 34), 0);
                else
                    lineSide = Line.makeLineFromPositionAndAngle(
                        new Vector(0, (posDesBall.X >= 0 ? 1 : -1) * 52.5f), 90);
                Vector posIntersect = lineSide.getIntersection(lineBody);
                posDesBall = posAgent +
                  Vector.fromPolar((float)(posIntersect.distance(posAgent) - 0.2),
                       (float)(angGlobal * Math.PI / 180.0));
            }

            Vector vecDesired = posDesBall - ballp;
            Vector vecShoot = vecDesired - new Vector(ball.VelX, ball.VelY);
            double dPower = getKickPowerForSpeed(vecShoot.Radius);
            double angActual = vecShoot.Theta * 180.0 / Math.PI - angBody;
            angActual = normalizeAngle(angActual);

            if (dPower > serverParameters.Maxpower && BallSpeed > 0.1)
            {
                freezeBall();
                return;
            }
            else if (dPower > serverParameters.Maxpower)
            {

                dPower = 100;

            }
            kick((float)dPower, (float)angActual);

            return;
        }

        public void freezeBall()
        {
            // determine power needed to kick the ball to compensate for current speed
            // get opposite direction (current direction + 180) relative to body
            // and make the kick command.


            Vector posAgentPred = predictAgentPos(1, 0);

            double dPower = getKickPowerForSpeed(BallSpeed);
            double tAng = ballVel.Theta * 180.0 / Math.PI + 180 - myself.BodyDirection;

            //       kick((float)dPower, (float)tAng);
            //       return;
            if (dPower > serverParameters.Maxpower)
            {

                dPower = (double)serverParameters.Maxpower;
            }
            //   Vector ballVel = new Vector(ball.VelX, ball.VelY);
            double dAngle = ballVel.Theta * 180.0 / Math.PI + 180 - myself.BodyDirection;
            dAngle = normalizeAngle(dAngle);
            Command soc = new Command();
            soc.Com = "kick";
            soc.powAng = dPower;
            soc.kickAng = dAngle;
            Vector posBall = new Vector(0, 0), velBall = new Vector(0, 0);
            predictBallInfoAfterCommand(soc, posBall, velBall);
            if (posBall.distance(posAgentPred) < 0.8 * MaximalKickDistance)
            {
                kick((float)dPower, (float)dAngle);
                return;

            }

            posBall.assign(ballp);
            // kick ball to position inside to compensate when agent is moving
            Vector posTo = posAgentPred +
              Vector.fromPolar((float)Math.Min(0.7 * MaximalKickDistance,
                        posBall.distance(posAgentPred) - 0.1),
                   (float)((posBall - posAgentPred).Theta * Math.PI / 180.0));
            Vector velDes = (posTo - posBall);
            accelerateBallToVelocity(velDes);
            return;
        }

        public void accelerateBallToVelocity(Vector velDes)
        {
            double angBody = myself.BodyDirection;
            Vector velBall = new Vector(ball.VelX, ball.VelY);
            Vector accDes = velDes - velBall;
            double dPower;
            double angActual;

            // if acceleration can be reached, create shooting vector
            if (accDes.Radius <= serverParameters.Ball_accel_max)
            {
                dPower = getKickPowerForSpeed(accDes.Radius);
                angActual = normalizeAngle(accDes.Theta * 180.0 / Math.PI - angBody);
                if (dPower <= serverParameters.Maxpower)
                {
                    kick((float)dPower, (float)angActual);

                    return;
                }
            }

            // else determine vector that is in direction 'velDes' (magnitude is lower)
            dPower = serverParameters.Maxpower;
            double dSpeed = getActualKickPowerRate() * dPower;
            velBall.rotate(-(float)velDes.Theta);
            double tmp = velBall.Y;
            angActual = velDes.Theta * 180.0 / Math.PI - asinDeg(tmp / dSpeed);
            angActual = normalizeAngle(angActual - angBody);

            kick((float)dPower, (float)angActual);
            return;
        }

        public static double asinDeg(double x)
        {
            if (x >= 1)
                return (90.0);
            else if (x <= -1)
                return (-90.0);

            return ((180.0 / Math.PI) * (Math.Asin(x)));
        }

        public void predictBallInfoAfterCommand(Command soc, Vector pos, Vector vel)
        {
            Vector temp;
            Vector posBall = new Vector(ball.X, ball.Y);
            Vector velBall = new Vector(ball.VelX, ball.VelY);

            if (soc.Com == "kick")
            {
                int iAng = (int)soc.kickAng;
                int iPower = (int)soc.powAng;

                // make angle relative to body
                // calculate added acceleration and add it to current velocity
                double ang = normalizeAngle(iAng + myself.BodyDirection);
                velBall += Vector.fromPolar((float)(getActualKickPowerRate() * iPower), (float)(ang * Math.PI / 180.0));
                if (velBall.Radius > serverParameters.Ball_speed_max)
                    velBall.normalize(serverParameters.Ball_speed_max);

            }
            posBall += velBall;
            velBall *= serverParameters.Ball_decay;

            temp = posBall;
            if ((object)pos != null)
                pos.assign(temp);
            temp = velBall;
            if ((object)vel != null)
                vel.assign(temp);
        }

        public double getKickPowerForSpeed(double dDesiredSpeed)
        {
            // acceleration after kick is calculated by power * eff_kick_power_rate
            // so actual kick power is acceleration / eff_kick_power_rate
            return dDesiredSpeed / getActualKickPowerRate();
        }

        public double getActualKickPowerRate()
        {
            // true indicates that relative angle to body should be returned
            double dir_diff = Math.Abs(getRelativeAngle(ballp, true));
            double dist = (mySelfp - ballp).Radius - serverParameters.Player_size - serverParameters.Ball_size;
            return serverParameters.Kick_power_rate *
                     (1 - 0.25 * dir_diff / 180.0 - 0.25 * dist / serverParameters.Kickable_margin);
        }

        public double getRelativeAngle(Vector o, bool bWithBody)
        {

            double dBody = 0.0;

            if (bWithBody == true)
                dBody = myself.BodyDirection - myself.HeadDirection;
            return normalizeAngle((o - mySelfp).Theta * 180.0 / Math.PI - dBody);

        }

        public Command turnBodyToBall()
        {

            return turnBodyToPoint(predictBallPosAfterNrCycles(1), 1);
        }

        #endregion





    }

    public class Line
    {
        // a line is defined by the formula: ay + bx + c = 0
        public double m_a; /*!< This is the a coefficient in the line ay + bx + c = 0 */
        public double m_b; /*!< This is the b coefficient in the line ay + bx + c = 0 */
        public double m_c; /*!< This is the c coefficient in the line ay + bx + c = 0 */

        /*! This constructor creates a line by given the three coefficents of the line.
            A line is specified by the formula ay + bx + c = 0.
            \param dA a coefficients of the line
            \param dB b coefficients of the line
            \param dC c coefficients of the line */
        public Line(double dA, double dB, double dC)
        {
            m_a = dA;
            m_b = dB;
            m_c = dC;
        }

        public int getCircleIntersectionPoints(Circle circle, Vector posSolution1, Vector posSolution2)
        {
            int iSol;
            double dSol1 = 0, dSol2 = 0;
            double h = circle.Center.X;
            double k = circle.Center.Y;

            // line:   x = -c/b (if a = 0)
            // circle: (x-h)^2 + (y-k)^2 = r^2, with h = center.x and k = center.y
            // fill in:(-c/b-h)^2 + y^2 -2ky + k^2 - r^2 = 0
            //         y^2 -2ky + (-c/b-h)^2 + k^2 - r^2 = 0
            // and determine solutions for y using abc-formula
            if (Math.Abs(m_a) < 10e-5)
            {
                iSol = abcFormula(1, -2 * k, ((-m_c / m_b) - h) * ((-m_c / m_b) - h)
                          + k * k - circle.Radius * circle.Radius, ref dSol1, ref dSol2);
                posSolution1.assign((float)(-m_c / m_b), (float)dSol1);
                posSolution2.assign((float)(-m_c / m_b), (float)dSol2);
                return iSol;
            }

            // ay + bx + c = 0 => y = -b/a x - c/a, with da = -b/a and db = -c/a
            // circle: (x-h)^2 + (y-k)^2 = r^2, with h = center.x and k = center.y
            // fill in:x^2 -2hx + h^2 + (da*x-db)^2 -2k(da*x-db) + k^2 - r^2 = 0
            //         x^2 -2hx + h^2 + da^2*x^2 + 2da*db*x + db^2 -2k*da*x -2k*db
            //                                                         + k^2 - r^2 = 0
            //       (1+da^2)*x^2 + 2(da*db-h-k*da)*x + h2 + db^2  -2k*db + k^2 - r^2 = 0
            // and determine solutions for x using abc-formula
            // fill in x in original line equation to get y coordinate
            double da = -m_b / m_a;
            double db = -m_c / m_a;

            double dA = 1 + da * da;
            double dB = 2 * (da * db - h - k * da);
            double dC = h * h + db * db - 2 * k * db + k * k - circle.Radius * circle.Radius;

            iSol = abcFormula(dA, dB, dC, ref dSol1, ref dSol2);

            posSolution1.assign((float)dSol1, (float)(da * dSol1 + db));
            posSolution2.assign((float)dSol2, (float)(da * dSol2 + db));
            return iSol;

        }

        public static int abcFormula(double a, double b, double c, ref double s1, ref double s2)
        {
            double dDiscr = b * b - 4 * a * c;       // discriminant is b^2 - 4*a*c
            if (Math.Abs(dDiscr) < 10e-5)       // if discriminant = 0
            {
                s1 = -b / (2 * a);              //  only one solution
                return 1;
            }
            else if (dDiscr < 0)               // if discriminant < 0
                return 0;                        //  no solutions
            else                               // if discriminant > 0
            {
                dDiscr = Math.Sqrt(dDiscr);           //  two solutions
                s1 = (-b + dDiscr) / (2 * a);
                s2 = (-b - dDiscr) / (2 * a);
                return 2;
            }
        }

        /*! This method returns the intersection point between the current Line and
            the specified line.
            \param line line with which the intersection should be calculated.
            \return VecPosition position that is the intersection point. */
        public Vector getIntersection(Line line)
        {
            Vector pos = new Vector(0, 0);
            double x, y;
            if ((m_a / m_b) == (line.m_a / line.m_b))
                return pos; // lines are parallel, no intersection
            if (m_a == 0)            // bx + c = 0 and a2*y + b2*x + c2 = 0 ==> x = -c/b
            {                          // calculate x using the current line
                x = -m_c / m_b;                // and calculate the y using the second line
                y = line.getYGivenX(x);
            }
            else if (line.m_a == 0)
            {                         // ay + bx + c = 0 and b2*x + c2 = 0 ==> x = -c2/b2
                x = -line.m_c / line.m_b; // calculate x using
                y = getYGivenX(x);       // 2nd line and calculate y using current line
            }
            // ay + bx + c = 0 and a2y + b2*x + c2 = 0
            // y = (-b2/a2)x - c2/a2
            // bx = -a*y - c =>  bx = -a*(-b2/a2)x -a*(-c2/a2) - c ==>
            // ==> a2*bx = a*b2*x + a*c2 - a2*c ==> x = (a*c2 - a2*c)/(a2*b - a*b2)
            // calculate x using the above formula and the y using the current line
            else
            {
                x = (m_a * line.m_c - line.m_a * m_c) /
                                (line.m_a * m_b - m_a * line.m_b);
                y = getYGivenX(x);
            }
            pos.X = (float)x;
            pos.Y = (float)y;
            return pos;
        }



        /*! This method returns the tangent line to a VecPosition. This is the line
            between the specified position and the closest point on the line to this
            position.
            \param pos VecPosition point with which tangent line is calculated.
            \return Line line tangent to this position */
        public Line getTangentLine(Vector pos)
        {
            // ay + bx + c = 0 -> y = (-b/a)x + (-c/a)
            // tangent: y = (a/b)*x + C1 -> by - ax + C2 = 0 => C2 = ax - by
            // with pos.y = y, pos.x = x
            return new Line(m_b, -m_a, m_a * pos.X - m_b * pos.Y);
        }



        /*! This method returns the distance between a specified position and the
            closest point on the given line.
            \param pos position to which distance should be calculated
            \return double indicating the distance to the line. */
        public double getDistanceWithPoint(Vector pos)
        {
            return pos.distance(getPointOnLineClosestTo(pos));
        }

        /*! This method determines whether the projection of a point on the
            current line lies between two other points ('point1' and 'point2')
            that lie on the same line.

            \param pos point of which projection is checked.
            \param point1 first point on line
            \param point2 second point on line
            \return true when projection of 'pos' lies between 'point1' and 'point2'.*/
        public bool isInBetween(Vector pos, Vector point1, Vector point2)
        {
            pos = getPointOnLineClosestTo(pos); // get closest point
            double dDist = point1.distance(point2); // get distance between 2 pos

            // if the distance from both points to the projection is smaller than this
            // dist, the pos lies in between.
            return pos.distance(point1) <= dDist &&
                   pos.distance(point2) <= dDist;
        }

        /*! This method calculates the y coordinate given the x coordinate
            \param x coordinate
            \return y coordinate on this line */
        public double getYGivenX(double x)
        {
            if (m_a == 0)
            {
                return 0;
            }
            // ay + bx + c = 0 ==> ay = -(b*x + c)/a
            return -(m_b * x + m_c) / m_a;
        }

        /*! This method calculates the x coordinate given the x coordinate
            \param y coordinate
            \return x coordinate on this line */
        public double getXGivenY(double y)
        {
            if (m_b == 0)
            {
                return 0;
            }
            // ay + bx + c = 0 ==> bx = -(a*y + c)/a
            return -(m_a * y + m_c) / m_b;
        }

        /*! This method creates a line given two points.
            \param pos1 first point
            \param pos2 second point
            \return line that passes through the two specified points. */
        public static Line makeLineFromTwoPoints(Vector pos1, Vector pos2)
        {
            // 1*y + bx + c = 0 => y = -bx - c
            // with -b the direction coefficient (or slope)
            // and c = - y - bx
            double dA, dB, dC;
            double dTemp = pos2.X - pos1.X; // determine the slope
            if (Math.Abs(dTemp) < 10e-5)
            {
                // ay + bx + c = 0 with vertical slope=> a = 0, b = 1
                dA = 0.0;
                dB = 1.0;
            }
            else
            {
                // y = (-b)x -c with -b the slope of the line
                dA = 1.0;
                dB = -(pos2.Y - pos1.Y) / dTemp;
            }
            // ay + bx + c = 0 ==> c = -a*y - b*x
            dC = -dA * pos2.Y - dB * pos2.X;
            return new Line(dA, dB, dC);
        }


        /*! This method returns the a coefficient from the line ay + bx + c = 0.
            \return a coefficient of the line. */
        public double getACoefficient()
        {
            return m_a;
        }

        /*! This method returns the b coefficient from the line ay + bx + c = 0.
            \return b coefficient of the line. */
        public double getBCoefficient()
        {
            return m_b;
        }

        /*! This method returns the c coefficient from the line ay + bx + c = 0.
            \return c coefficient of the line. */
        public double getCCoefficient()
        {
            return m_c;
        }

        public static Line makeLineFromPositionAndAngle(Vector vec, double angle)
        {
            // calculate point somewhat further in direction 'angle' and make
            // line from these two points.
            return makeLineFromTwoPoints(vec, vec + Vector.fromPolar(1, (float)(angle * Math.PI / 180.0)));
        }

        internal Vector getPointOnLineClosestTo(Vector posAgent)
        {
            Line l2 = getTangentLine(posAgent);  // get tangent line
            return getIntersection(l2);     // and intersection between the two lines
        }
    }
}
