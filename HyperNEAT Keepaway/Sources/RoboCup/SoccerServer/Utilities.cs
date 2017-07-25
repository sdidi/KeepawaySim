// -*-C#-*-

/***************************************************************************
                                   Utilities.cs
                   Contains utility functions and classes.
                             -------------------
    begin                : JUL-2009
    credit               : Translated from / Based on the RoboCup Soccer Server
                           by The RoboCup Soccer Server  Maintenance Group.
                           (Implementation done by Phillip Verbancsics of the
                            Evolutionary Complexity Research Group)
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
using RoboCup.Geometry;
using RoboCup.Parameters;

namespace RoboCup
{
    /// <summary>
    /// Class to hold definitions across the library.
    /// </summary>
    static class Defines
    {


        public const float Epsilon = 1.0e-10f;

        public static string[] PlaymodeStrings = {"",                   
            "before_kick_off",                  
            "time_over",                        
            "play_on",                          
            "kick_off_l",                       
            "kick_off_r",                       
            "kick_in_l",                        
            "kick_in_r",                        
            "free_kick_l",                      
            "free_kick_r",                      
            "corner_kick_l",                    
            "corner_kick_r",                    
            "goal_kick_l",                      
            "goal_kick_r",                      
            "goal_l",                           
            "goal_r",                           
            "drop_ball",                        
            "offside_l",                        
            "offside_r",                        
            "penalty_kick_l",                   
            "penalty_kick_r",                   
            "first_half_over",                  
            "pause",                            
            "human_judge",                      
            "foul_charge_l",                    
            "foul_charge_r",                    
            "foul_push_l",                      
            "foul_push_r",                      
            "foul_multiple_attack_l",           
            "foul_multiple_attack_r",           
            "foul_ballout_l",                   
            "foul_ballout_r",                   
            "back_pass_l",                      
            "back_pass_r",                      
            "free_kick_fault_l",                
            "free_kick_fault_r",                
            "catch_fault_l",                    
            "catch_fault_r",                    
            "indirect_free_kick_l",             
            "indirect_free_kick_r",             
            "penalty_setup_l",                  
            "penalty_setup_r",                  
            "penalty_ready_l",                  
            "penalty_ready_r",                  
            "penalty_taken_l",                  
            "penalty_taken_r",                  
            "penalty_miss_l",                   
            "penalty_miss_r",                   
            "penalty_score_l",                  
            "penalty_score_r"                   
            };


    }

    /// <summary>
    /// Enumeration of player states a player may have
    /// </summary>
    internal enum PlayerState
    {
        DISABLE = 0x00000000,
        STAND = 0x00000001,
        KICK = 0x00000002,
        KICK_FAULT = 0x00000004,
        GOALIE = 0x00000008,
        CATCH = 0x00000010,
        CATCH_FAULT = 0x00000020,
        BALL_TO_PLAYER = 0x00000040,
        PLAYER_TO_BALL = 0x00000080,
        DISCARD = 0x00000100,
        LOST = 0x00000200, 
        BALL_COLLIDE = 0x00000400,
        PLAYER_COLLIDE = 0x00000800, 
        TACKLE = 0x00001000,
        TACKLE_FAULT = 0x00002000,
        BACK_PASS = 0x00004000,
        FREE_KICK_FAULT = 0x00008000,
        POST_COLLIDE = 0x00010000, 
    };

    /// <summary>
    /// Enumeration of play modes in the server
    /// </summary>
    public enum PlayMode
    {
        Null,
        BeforeKickOff,
        TimeOver,
        PlayOn,
        KickOff_Left,
        KickOff_Right,
        KickIn_Left,
        KickIn_Right,
        FreeKick_Left,
        FreeKick_Right,
        CornerKick_Left,
        CornerKick_Right,
        GoalKick_Left,
        GoalKick_Right,
        AfterGoal_Left,
        AfterGoal_Right,
        Drop_Ball,
        OffSide_Left,
        OffSide_Right,
        PK_Left,
        PK_Right,
        FirstHalfOver,
        Pause,
        Human,
        Foul_Charge_Left,
        Foul_Charge_Right,
        Foul_Push_Left,
        Foul_Push_Right,
        Foul_MultipleAttacker_Left,
        Foul_MultipleAttacker_Right,
        Foul_BallOut_Left,
        Foul_BallOut_Right,
        Back_Pass_Left,
        Back_Pass_Right,
        Free_Kick_Fault_Left,
        Free_Kick_Fault_Right,
        CatchFault_Left,
        CatchFault_Right,
        IndFreeKick_Left,
        IndFreeKick_Right,
        PenaltySetup_Left,
        PenaltySetup_Right,
        PenaltyReady_Left,
        PenaltyReady_Right,
        PenaltyTaken_Left,
        PenaltyTaken_Right,
        PenaltyMiss_Left,
        PenaltyMiss_Right,
        PenaltyScore_Left,
        PenaltyScore_Right,
        MAX
    };

    /// <summary>
    /// Enumeration of ball position information
    /// </summary>
    enum BallPositionInformtion
    {
        Null,
        InField,
        GoalL,
        GoalR,
        OutOfField,
        MAX
    };

    /// <summary>
    /// Class that holds several utility methods.
    /// </summary>
    static class Utilities
    {

        #region Random

 /* C# Version Copyright (C) 2001-2004 Akihilo Kramot (Takel).  */
/* C# porting from a C-program for MT19937, originaly coded by */
/* Takuji Nishimura, considering the suggestions by            */
/* Topher Cooper and Marc Rieffel in July-Aug. 1997.           */
/* This library is free software under the Artistic license:   */
/*                                                             */
/* You can find the original C-program at                      */
/*     http://www.math.keio.ac.jp/~matumoto/mt.html            */

        public class Random       
        {
            /* Period parameters */
            private const int N = 624;
            private const int M = 397;
            private const uint MATRIX_A = 0x9908b0df; /* constant vector a */
            private const uint UPPER_MASK = 0x80000000; /* most significant w-r bits */
            private const uint LOWER_MASK = 0x7fffffff; /* least significant r bits */

            /* Tempering parameters */
            private const uint TEMPERING_MASK_B = 0x9d2c5680;
            private const uint TEMPERING_MASK_C = 0xefc60000;

            private static uint TEMPERING_SHIFT_U(uint y) { return (y >> 11); }
            private static uint TEMPERING_SHIFT_S(uint y) { return (y << 7); }
            private static uint TEMPERING_SHIFT_T(uint y) { return (y << 15); }
            private static uint TEMPERING_SHIFT_L(uint y) { return (y >> 18); }

            private uint[] mt = new uint[N]; /* the array for the state vector  */

            private short mti;

            private static uint[] mag01 = { 0x0, MATRIX_A };

            /* initializing the array with a NONZERO seed */
            public Random(uint seed)
            {
                /* setting initial seeds to mt[N] using         */
                /* the generator Line 25 of Table 1 in          */
                /* [KNUTH 1981, The Art of Computer Programming */
                /*    Vol. 2 (2nd Ed.), pp102]                  */
                mt[0] = seed & 0xffffffffU;
                for (mti = 1; mti < N; ++mti)
                {
                    mt[mti] = (69069 * mt[mti - 1]) & 0xffffffffU;
                }
            }
            public Random()
                : this(4357) /* a default initial seed is used   */
            {
            }

            protected uint GenerateUInt()
            {
                uint y;

                /* mag01[x] = x * MATRIX_A  for x=0,1 */
                if (mti >= N) /* generate N words at one time */
                {
                    short kk = 0;

                    for (; kk < N - M; ++kk)
                    {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
                    }

                    for (; kk < N - 1; ++kk)
                    {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1];
                    }

                    y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                    mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];

                    mti = 0;
                }

                y = mt[mti++];
                y ^= TEMPERING_SHIFT_U(y);
                y ^= TEMPERING_SHIFT_S(y) & TEMPERING_MASK_B;
                y ^= TEMPERING_SHIFT_T(y) & TEMPERING_MASK_C;
                y ^= TEMPERING_SHIFT_L(y);

                return y;
            }

            public virtual uint NextUInt()
            {
                return this.GenerateUInt();
            }

            public virtual uint NextUInt(uint maxValue)
            {
                return (uint)(this.GenerateUInt() / ((float)uint.MaxValue / maxValue));
            }

            public virtual uint NextUInt(uint minValue, uint maxValue) /* throws ArgumentOutOfRangeException */
            {
                if (minValue >= maxValue)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return (uint)(this.GenerateUInt() / ((float)uint.MaxValue / (maxValue - minValue)) + minValue);
            }

            public int Next()
            {
                return this.Next(int.MaxValue);
            }

            public int Next(int maxValue) /* throws ArgumentOutOfRangeException */
            {
                if (maxValue <= 1)
                {
                    if (maxValue < 0)
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    return 0;
                }

                return (int)(this.NextDouble() * maxValue);
            }

            public int Next(int minValue, int maxValue)
            {
                if (maxValue < minValue)
                {
                    throw new ArgumentOutOfRangeException();
                }
                else if (maxValue == minValue)
                {
                    return minValue;
                }
                else
                {
                    return this.Next(maxValue - minValue) + minValue;
                }
            }

            public void NextBytes(byte[] buffer) /* throws ArgumentNullException*/
            {
                int bufLen = buffer.Length;

                if (buffer == null)
                {
                    throw new ArgumentNullException();
                }

                for (int idx = 0; idx < bufLen; ++idx)
                {
                    buffer[idx] = (byte)this.Next(256);
                }
            }

            public double NextDouble()
            {
                return (float)this.GenerateUInt() / ((ulong)uint.MaxValue + 1);
            }

            public double NextDouble(double min, double max)
            {
                if(min > max){
                    double tmp = min;
                    min = max;
                    max = tmp;
                }
                return min + NextDouble()*(max - min);
            }

            double z2 = double.NaN; 

            public double NextGuassian(double dir, double sigma)
            {
                if (!double.IsNaN(z2))
                {
                    double temp = dir + sigma * z2;
                    z2 = double.NaN;
                    return temp;
                }

                double u1 = NextDouble();
                double u2 = NextDouble();

                double z1 = Math.Sqrt(-2 * Math.Log(u1 + Double.Epsilon)) * Math.Sin(2 * Math.PI * u2);
                z2 = Math.Sqrt(-2 * Math.Log(u1 + Double.Epsilon)) * Math.Cos(2 * Math.PI * u2);

                return dir + z1 * sigma;


            }
        }

        #endregion

        /// <summary>
        /// Seed for the ranomd number generator
        /// </summary>
        public static int Seed { get; set; }
        private static Random random = null;
        /// <summary>
        /// Custome Randomum Number Generator Instance for server use
        /// </summary>
        public static Random CustomRandom
        {
            get
            {
                if (random == null)
                {

                    if (ServerParam.Instance.Server_Random_seed > -1)
                    {

                        random = new Random((uint)ServerParam.Instance.Server_Random_seed);
                        Seed = ServerParam.Instance.Server_Random_seed;
                    }
                    else
                    {

                        Seed = (int)DateTime.Now.Ticks;
                        random = new Random((uint)Seed);

                    }
                }

                return random;
            }
        }

        /// <summary>
        /// Quantizes a value to a particular interval
        /// </summary>
        /// <param name="value">The value to be quantized.</param>
        /// <param name="interval">The quantization factor.</param>
        /// <returns>Returns a number that is now quantized to a certain interval.</returns>
        public static double Quantize(double value, double interval)
        {
            return Math.Round(value / interval,MidpointRounding.AwayFromZero) * interval;
        }

        /// <summary>
        /// Finds the least common multiple between two numbers.
        /// </summary>
        /// <param name="number1">The first value.</param>
        /// <param name="number2">The secon value</param>
        /// <returns>Returns the least common multiple of the two values.</returns>
        public static int LeastCommonMultiple(int number1, int number2)
        {

            int tmp = 0, idx = 0, larger = Math.Max(number1, number2);
            do
            {
                ++idx;
                tmp = larger * idx;
            }
            while (tmp % number1 != 0 || tmp % number2 != 0);
            return tmp;

        }

        /// <summary>
        /// Finds if the line between two points intersects with a circle
        /// </summary>
        /// <param name="beginPoint">The beginning point of the line.</param>
        /// <param name="endPoint">The end point of the line.</param>
        /// <param name="circle">The circle to intersect with.</param>
        /// <param name="intersectionPoint">The intersection point.</param>
        /// <returns>Returns true iff they intersect, and sets intersection point to that point.</returns>
        public static bool intersect(Vector beginPoint, Vector endPoint, Circle circle, Vector intersectionPoint)
        {
            if (beginPoint == endPoint)
                return false;

            if ((beginPoint - endPoint).Radius < (beginPoint - circle.Center).Radius - circle.Radius)
               
                return false;

            if (circle.Center == new Vector(0,0))
            {
                float deltaX = endPoint.X - beginPoint.X;
                float deltaY = endPoint.Y - beginPoint.Y;
      
                float deltaRadius = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
           
                float D = beginPoint.X * endPoint.Y - endPoint.X * beginPoint.Y;
                float descriminant = circle.Radius * circle.Radius * deltaRadius * deltaRadius - D * D;
              
                if (descriminant <= 0.0)
                {
        
                    return false;
                }
                else
                {
                    descriminant = (float)Math.Sqrt(descriminant);
   

                    float x1 = (D * deltaY + deltaX * descriminant) / (deltaRadius * deltaRadius);
                    float x2 = (D * deltaY - deltaX * descriminant) / (deltaRadius * deltaRadius);
                    float y1 = (-D * deltaX + Math.Abs(deltaY) * descriminant) / (deltaRadius * deltaRadius);
                    float y2 = (-D * deltaX - Math.Abs(deltaY) * descriminant) / (deltaRadius * deltaRadius);
                    Vector firstPoint, secondPoint;
                    if (deltaY < 0)
                    {
                        firstPoint = new Vector(x2, y1);
                        secondPoint = new Vector(x1, y2);
                    }
                    else
                    {
                        firstPoint = new Vector(x1, y1);
                        secondPoint = new Vector(x2, y2);
                    }

                    if (!firstPoint.between(beginPoint, endPoint)
                         && !secondPoint.between(beginPoint, endPoint))
                    {

                        return false;
                    }

                    if (!firstPoint.between(beginPoint, endPoint))
                    {
                        intersectionPoint = secondPoint;
                        secondPoint = firstPoint;
                    }
                    else if (!secondPoint.between(beginPoint, endPoint))
                    {
                        intersectionPoint = firstPoint;
                    }
                    else
                    {
                        if ((beginPoint - firstPoint).Radius < (beginPoint - secondPoint).Radius)
                        {
                            intersectionPoint = firstPoint;
                        }
                        else
                        {
                            intersectionPoint = secondPoint;
                            secondPoint = firstPoint;
                        }
                    }

                    if (intersectionPoint == beginPoint
                         && !secondPoint.between(beginPoint, endPoint))
                    {
                        return false;
                    }
                    return true;
                }
            }
            else
            {
                if (intersect(beginPoint - circle.Center, endPoint - circle.Center,
                                new Circle(new Vector(0,0), circle.Radius), intersectionPoint))
                {
                    intersectionPoint += circle.Center;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Normalizes an angle to be in the range [-pi,pi]
        /// </summary>
        /// <param name="angle">The angle to be normalized.</param>
        /// <returns>Returns the angle in the range [-pi,pi].</returns>
        public static double Normalize_angle(double angle)
        {
            if (Math.Abs(angle) > 2 * Math.PI)
            {
                angle = angle % (Math.PI * 2);
            }

            if (angle < -Math.PI) angle += (Math.PI * 2);
            if (angle > Math.PI) angle -= (Math.PI * 2);

            return angle;
        }

        /// <summary>
        /// Finds a circle represented the nearest post to a position
        /// </summary>
        /// <param name="position">The position the post should be near.</param>
        /// <param name="objectSize">The size of the object at that positon.</param>
        /// <returns>Returns the circle that would represent a collision with the post.</returns>
        public static Circle nearestPost(Vector position, float objectSize)
        {
            Vector nearestPost;

            if (position.Y > 0)
            {
                if (position.X > 0)
                {
                    nearestPost = new Vector(ServerParam.PITCH_LENGTH * 0.5f
                                             - ServerParam.Instance.GoalPostRadius,
                                             ServerParam.Instance.Goal_width * 0.5f
                                             + ServerParam.Instance.GoalPostRadius);
                }
                else
                {
                    nearestPost = new Vector(-ServerParam.PITCH_LENGTH * 0.5f
                                             + ServerParam.Instance.GoalPostRadius,
                                             ServerParam.Instance.Goal_width * 0.5f
                                             + ServerParam.Instance.GoalPostRadius);
                }
            }
            else
            {
                if (position.X > 0)
                {
                    nearestPost = new Vector(ServerParam.PITCH_LENGTH * 0.5f
                                             - ServerParam.Instance.GoalPostRadius,
                                             -ServerParam.Instance.Goal_width * 0.5f
                                             - ServerParam.Instance.GoalPostRadius);
                }
                else
                {
                    nearestPost = new Vector(-ServerParam.PITCH_LENGTH * 0.5f
                                             + ServerParam.Instance.GoalPostRadius,
                                             -ServerParam.Instance.Goal_width * 0.5f
                                             - ServerParam.Instance.GoalPostRadius);
                }
            }

            return new Circle(nearestPost, ServerParam.Instance.GoalPostRadius + objectSize);
        }

        /// <summary>
        /// Bounds a double value between a minimum and maximum
        /// </summary>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="maximum">The maximum value.</param>
        /// <returns>Returns either the current value, or the maximum or minimum of it goes beyond those bounds.</returns>
        public static float bound(float minimum, float currentValue, float maximum)
        {
            return Math.Min(Math.Max(minimum, currentValue), maximum - Defines.Epsilon);
        }

        /// <summary>
        /// Gets a random double between two values.
        /// </summary>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        /// <returns>Returns a random value in the range [minimum, maximum]</returns>
        public static double drand(double minimum, double maximum)
        {
            return CustomRandom.NextDouble(minimum, maximum);
        }

        /// <summary>
        /// Expands a tilde for path names.
        /// </summary>
        /// <param name="tildePath">The name of the path that may include a tilde.</param>
        /// <returns>Returns the path with the tilde expanded to the correct directory.</returns>
        public static string tildeExpand(string tildePath)
        {

            string newPath = tildePath;
            if (newPath.Length == 0
                 || newPath[0] != '~')
            {
                return newPath;
            }

            if (newPath.Length == 1
                 || newPath[1] == '/')
            {

                string userDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                return userDirectory + newPath.Substring(1);

            }

            return newPath;


        }
    }
}
