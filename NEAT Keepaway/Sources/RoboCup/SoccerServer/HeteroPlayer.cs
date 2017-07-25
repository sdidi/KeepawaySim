// -*-C#-*-

/***************************************************************************
                                   HetroPlayer.cs
                   Class to generate and hold HeteroPlayer settings.
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
using RoboCup.Parameters;

namespace RoboCup.Parameters
{
    /// <summary>
    /// Class to generate and hold HeteroPlayer settings.
    /// Is Serializable.
    /// </summary>
    [Serializable]
    public class HeteroPlayer
    {

        #region Properties and Instance Variables
        /// <summary>
        /// The Id of this HeteroPlayer type
        /// </summary>
        public float Id ;

        /// <summary>
        /// This player type's maximum speed.
        /// </summary>
        public float PlayerSpeedMax ;

        /// <summary>
        /// This player type's maximum stamina increment
        /// </summary>
        public float StaminaIncrementMax ;

        /// <summary>
        /// This player type's player decay.
        /// </summary>
        public float PlayerDecay ;

        /// <summary>
        /// This player type's intertia moment.
        /// </summary>
        public float InertiaMoment ;

        /// <summary>
        /// This player type's dash power rate.
        /// </summary>
        public float DashPowerRate ;

        /// <summary>
        /// This player type's size.
        /// </summary>
        public float PlayerSize ;

        /// <summary>
        /// This player type's kickable margin.
        /// </summary>
        public float KickableMargin ;

        /// <summary>
        /// This player type's kick noise.
        /// </summary>
        public float KickRandomness ;

        /// <summary>
        /// This player type's extra stamina.
        /// </summary>
        public float ExtraStamina ;

        /// <summary>
        /// This player type's maximum effort
        /// </summary>
        public float EffortMax ;

        /// <summary>
        /// This player type's minimum effort.
        /// </summary>
        public float EffortMin ;

        #endregion

        #region Class Variables
        /// <summary>
        /// Shows whether the random generator for the HeteroPlayer generator been seeded. 
        /// </summary>
        private static bool IsRandomSeeded = false;

        /// <summary>
        /// Random number generator to create randomized HeteroPlayers
        /// </summary>
        private static Utilities.Random random;

        /// <summary>
        /// The seed used to generate the HeteroPlayers.
        /// </summary>
        public static int RandomSeed { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor to create a default HeteroPlayer with the static settings defined in the ServerParam file.
        /// </summary>
        /// <param name="dummy">Dummy input to indicate that we should generate the base HeteroPlayer.</param>
        public HeteroPlayer(object dummy)
        {
            PlayerSpeedMax = ServerParam.Instance.Player_speed_max;
            StaminaIncrementMax = ServerParam.Instance.Stamina_inc_max;

            PlayerDecay = ServerParam.Instance.Player_decay;
            InertiaMoment = ServerParam.Instance.Inertia_moment;

            DashPowerRate = ServerParam.Instance.Dash_power_rate;
            PlayerSize = ServerParam.Instance.Player_size;

            KickableMargin = ServerParam.Instance.Kickable_margin;
            KickRandomness = ServerParam.Instance.Kick_rand;

            ExtraStamina = ServerParam.Instance.Extra_Stamina;
            EffortMax = ServerParam.Instance.Effort_init;
            EffortMin = ServerParam.Instance.Effort_min;
        }

        /// <summary>
        /// Constructor which generates a randomized HeteroPlayer within the PlayerParam and ServerParam settings.
        /// </summary>
        public HeteroPlayer()
        {
            const int maximumAttempts = 1000;

            int attemptNumber = 0;
            while (++attemptNumber <= maximumAttempts)
            {
                //
                float temporaryDelta = delta(PlayerParam.Instance.Player_speed_max_delta_min,
                                                        PlayerParam.Instance.Player_speed_max_delta_max);
                PlayerSpeedMax = ServerParam.Instance.Player_speed_max + temporaryDelta;
                if (PlayerSpeedMax <= 0.0) continue;

                StaminaIncrementMax = ServerParam.Instance.Stamina_inc_max
                    + temporaryDelta * PlayerParam.Instance.Stamina_inc_max_delta_factor;
                if (StaminaIncrementMax <= 0.0) continue;
                //
                temporaryDelta = delta(PlayerParam.Instance.Player_decay_delta_min,
                                                 PlayerParam.Instance.Player_decay_delta_max);
                PlayerDecay = ServerParam.Instance.Player_decay + temporaryDelta;
                if (PlayerDecay <= 0.0) continue;
                InertiaMoment = ServerParam.Instance.Inertia_moment
                    + temporaryDelta * PlayerParam.Instance.Inertia_moment_delta_factor;
                if (InertiaMoment < 0.0) continue;
                //
                temporaryDelta = delta(PlayerParam.Instance.Dash_power_rate_delta_min,
                                                 PlayerParam.Instance.Dash_power_rate_delta_max);
                DashPowerRate = ServerParam.Instance.Dash_power_rate + temporaryDelta;
                if (DashPowerRate <= 0.0) continue;
                PlayerSize = ServerParam.Instance.Player_size
                    + temporaryDelta * PlayerParam.Instance.Player_size_delta_factor;
                if (PlayerSize <= 0.0) continue;

                // trade-off stamina_inc_max with dash_power_rate
                temporaryDelta = delta(PlayerParam.Instance.New_dash_power_rate_delta_min,
                                                 PlayerParam.Instance.New_dash_power_rate_delta_max);
                DashPowerRate = ServerParam.Instance.Dash_power_rate + temporaryDelta;
                if (DashPowerRate <= 0.0) continue;
                StaminaIncrementMax = ServerParam.Instance.Stamina_inc_max
                    + temporaryDelta * PlayerParam.Instance.New_stamina_inc_max_delta_factor;
                if (StaminaIncrementMax <= 0.0) continue;
                //
                temporaryDelta = delta(PlayerParam.Instance.Kickable_margin_delta_min,
                                                 PlayerParam.Instance.Kickable_margin_delta_max);
                KickableMargin = ServerParam.Instance.Kickable_margin + temporaryDelta;
                if (KickableMargin <= 0.0) continue;
                KickRandomness = ServerParam.Instance.Kick_rand
                    + temporaryDelta * PlayerParam.Instance.Kick_rand_delta_factor;
                if (KickRandomness < 0.0) continue;
                //
                temporaryDelta = delta(PlayerParam.Instance.Extra_stamina_delta_min,
                                                 PlayerParam.Instance.Extra_stamina_delta_max);
                ExtraStamina = ServerParam.Instance.Extra_Stamina + temporaryDelta;
                if (ExtraStamina < 0.0) continue;
                EffortMax = ServerParam.Instance.Effort_init
                    + temporaryDelta * PlayerParam.Instance.Effort_max_delta_factor;
                EffortMin = ServerParam.Instance.Effort_min
                    + temporaryDelta * PlayerParam.Instance.Effort_min_delta_factor;
                if (EffortMax <= 0.0) continue;
                if (EffortMin <= 0.0) continue;

                float real_speed_max
                    = (ServerParam.Instance.Maxpower
                        * DashPowerRate
                        * EffortMax)
                    / (1.0f - PlayerDecay);

                if (ServerParam.Instance.Player_Speed_Max_Min - Defines.Epsilon < real_speed_max
                     && real_speed_max < PlayerSpeedMax + Defines.Epsilon)
                {
                    break;
                }
            }


            if (attemptNumber > maximumAttempts)
            {
 
                PlayerSpeedMax = ServerParam.Instance.Player_speed_max;
                StaminaIncrementMax = ServerParam.Instance.Stamina_inc_max;

                PlayerDecay = ServerParam.Instance.Player_decay;
                InertiaMoment = ServerParam.Instance.Inertia_moment;

                DashPowerRate = ServerParam.Instance.Dash_power_rate;
                PlayerSize = ServerParam.Instance.Player_size;

                KickableMargin = ServerParam.Instance.Kickable_margin;
                KickRandomness = ServerParam.Instance.Kick_rand;

                ExtraStamina = ServerParam.Instance.Extra_Stamina;
                EffortMax = ServerParam.Instance.Effort_init;
                EffortMin = ServerParam.Instance.Effort_min;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generates a random delta between min and max.
        /// </summary>
        /// <param name="minimumDelta">The minimum value of the delta.</param>
        /// <param name="maximumDelta">The maximum value of the delta.</param>
        /// <returns>A random float between min and max.</returns>
        private float delta(float minimumDelta, float maximumDelta)
        {

            if (!IsRandomSeeded)
            {
                if (PlayerParam.Instance.Random_seed >= 0)
                {
                    RandomSeed = PlayerParam.Instance.Random_seed;
                    //srandom( PlayerParam::instance().randomSeed() );
                    random = new Utilities.Random((uint)PlayerParam.Instance.Random_seed);
                }
                else
                {
                    RandomSeed = (int)DateTime.Now.Ticks;
                    random = new Utilities.Random((uint)RandomSeed);
                }
                Console.WriteLine("Using given Hetero Player Seed: " + RandomSeed);

                IsRandomSeeded = true;
            }

            if (minimumDelta == maximumDelta)
            {
                return minimumDelta;
            }

            float minimumValue = minimumDelta;
            float maximumValue = maximumDelta;

            if (minimumValue > maximumValue)
            {
                minimumValue = maximumDelta;
                maximumValue = minimumDelta;
            }

            return (float)random.NextDouble(minimumValue, maximumValue);
        }

        #endregion
    }
}
