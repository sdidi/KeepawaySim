// -*-C#-*-

/***************************************************************************
                                   ServerParam.cs
                    Class that represents Server Parameters for the RoboCup server.
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
using System.Collections.Generic;

namespace RoboCup.Parameters
{
    /// <summary>
    /// Class that reads and hold paramters for the RoboCup server.
    /// It is serializable.
    /// </summary>
    [Serializable]
    public class ServerParam
    {

        #region Class Properties and Variables

        /// <summary>
        /// Statically accessible instance of ServerParams
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public static ServerParam Instance;


        /// <summary>
        /// XML serializer for paramter file
        /// </summary>
        private static System.Xml.Serialization.XmlSerializer serialize = new System.Xml.Serialization.XmlSerializer(typeof(ServerParam));

        #endregion

        #region Instance Variables, Properties, Getters, and Setter


        /// <summary>
        /// The random seed used for the RNG for the server.
        /// </summary>
        public int Server_Random_seed;

        /// <summary>
        /// Width of the goal
        /// </summary>
        public float Goal_width;

        /// <summary>
        /// Size of the goal post
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public float GoalPostRadius
        {
            get { return 0.6f; }
        }

        /// <summary>
        /// Intertia moment for objects on the s_half_time_count
        /// </summary>
        public float Inertia_moment;

        float M_player_size;

        /// <summary>
        /// The size of the players.
        /// </summary>
        public float Player_size
        {

            get { return M_player_size; }
            set
            {
                ControlRadiusWidth += M_player_size;
                KickableArea -= M_player_size;
                M_player_size = value;
                KickableArea += M_player_size;
                ControlRadiusWidth -= M_player_size;
            }

        }

        
        /// <summary>
        /// Player decay
        /// </summary>
        public float Player_decay;

        
        /// <summary>
        /// Random factor applied to players
        /// </summary>
        public float Player_rand;

        
        /// <summary>
        /// Player weight
        /// </summary>
        public float Player_weight;

    
        /// <summary>
        /// Maximum player speed
        /// </summary>
        public float Player_speed_max;
        
        
        /// <summary>
        /// Maximum player acceleration
        /// </summary>
        public float Player_accel_max;
    
        
        /// <summary>
        /// The maximum stamina for a player
        /// </summary>
        public float Stamina_max;

        
        /// <summary>
        /// The maximum player stamina increment
        /// </summary>
        public float Stamina_inc_max;


        public float Recover_init;

        
        /// <summary>
        /// Player recover decrement threshold
        /// </summary>
        public float Recover_dec_thr;

        
        /// <summary>
        /// Minimum recover for player
        /// </summary>
        public float Recover_min;
        
        /// <summary>
        /// Player recovery decrement
        /// </summary>
        public float Recover_dec;
        
        /// <summary>
        /// Initial/Maximum effort for player
        /// </summary>
        public float Effort_init;
        
        /// <summary>
        /// Player dash effort decrement threshold
        /// </summary>
        public float Effort_dec_thr;
        
        /// <summary>
        /// Minimum effort for player dash
        /// </summary>
        public float Effort_min;
        
        /// <summary>
        /// Player dash effort decrement
        /// </summary>
        public float Effort_dec;

       
        /// <summary>
        /// Player dash increment threshold
        /// </summary>
        public float Effort_inc_thr;
        
        /// <summary>
        /// Player dash effort increment
        /// </summary>
        public float Effort_inc;

        /// <summary>
        /// Noise added directly to kicks
        /// </summary>
        public float Kick_rand;


        /// <summary>
        /// Random factor for players
        /// </summary>
        public float Prand_factor;
        
        /// <summary>
        /// Random factor for kicking
        /// /// </summary>
        public float Kick_rand_factor;

        float M_ball_size; /* ball size */

        /// <summary>
        /// The size of the ball.
        /// </summary>
        public float Ball_size
        {
            get { return M_ball_size; }
            set
            {
                KickableArea -= M_ball_size;
                M_ball_size = value;
                KickableArea += M_ball_size;
            }
        }

       
        /// <summary>
        /// Ball decay
        /// </summary>
        public float Ball_decay;
        
        /// <summary>
        /// Random factor for ball motion
        /// </summary>
        public float Ball_rand;

        /// <summary>
        /// The weight of the ball
        /// </summary>
        public float Ball_weight;

        /// <summary>
        /// The maximum ball speed
        /// </summary>
        public float Ball_speed_max;

        /// <summary>
        /// The maximum ball acceleration
        /// </summary>
        public float Ball_accel_max;

        /// <summary>
        /// The dash power rate
        /// </summary>
        public float Dash_power_rate;

        /// <summary>
        /// The power rate for kicking
        /// </summary>
        public float Kick_power_rate;

        float M_kickable_margin; /* kickable margin */

        /// <summary>
        /// The marginal area that a ball can be kicked from.
        /// </summary>
        public float Kickable_margin
        {
            get { return M_kickable_margin; }
            set
            {
                KickableArea -= M_kickable_margin;
                M_kickable_margin = value;
                KickableArea += M_kickable_margin;
            }
        }

        float M_control_radius; /* control radius */

        /// <summary>
        /// Control radius for controlling the ball.
        /// </summary>
        public float Control_radius
        {
            get { return M_control_radius; }
            set
            {
                ControlRadiusWidth -= M_control_radius;
                M_control_radius = value;
                ControlRadiusWidth += M_control_radius;
            }
        }

        
        /// <summary>
        /// The width of the control radius.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public float ControlRadiusWidth;

        
        
        /// <summary>
        /// Maximum power
        /// </summary>
        public float Maxpower;
        
        /// <summary>
        /// Minimim power
        /// </summary>
        public float Minpower;
        
        /// <summary>
        /// The maximum object moment
        /// </summary>
        public float MaxMoment;

        
        /// <summary>
        /// The minimum object moment
        /// </summary>
        public float MinMoment;

        
        /// <summary>
        /// The maximum neck moment
        /// </summary>
        public float MaxNeckMoment;

        
        /// <summary>
        /// The minimum neck moment
        /// </summary>
        public float MinNeckMoment;

        /// <summary>
        /// The maxmimum neck angle
        /// </summary>
        public float MaxNeckAngle;

        /// <summary>
        /// The minimum neck angle
        /// </summary>
        public float MinNeckAngle;

        /// <summary>
        /// Visible angle for a player
        /// </summary>
        public float Visible_Angle;
        
        
        /// <summary>
        /// Visible distance for a player
        /// </summary>
        public float Visible_Distance;

        /// <summary>
        /// Direction of the wind
        /// </summary>
        public float Wind_Dir;

        /// <summary>
        /// Force of the wind
        /// </summary>
        public float Wind_Force;

        /// <summary>
        /// Angle of the wind
        /// </summary>
        public float Wind_Angle;


        /// <summary>
        /// Range for random wind force
        /// </summary>
        public float Wind_Rand;

        /// <summary>
        /// No wind?
        /// </summary>
        public bool Wind_None;

        /// <summary>
        /// Is the wind random?
        /// </summary>
        public bool Wind_Random;

        
        /// <summary>
        /// Kickable area around a ball
        /// </summary>
        public float KickableArea;

        
        /// <summary>
        /// Catchable area length for a goalie
        /// </summary>
        public float Catchable_area_l;
        
        /// <summary>
        /// Catchable area width for a goalie
        /// </summary>
        public float Catchable_area_w;

        /// <summary>
        /// Probability of goalie catching
        /// </summary>
        public float Catch_probability;
        
        /// <summary>
        /// Maximum goalie moves allowed after a catch
        /// </summary>
        public int Goalie_max_moves;
        
        
        /// <summary>
        /// Do we play keepaway?
        /// </summary>
        public bool Keepaway;
        
        /// <summary>
        /// Length of the keepaway region
        /// </summary>
        public float Keepaway_Length;
        
        /// <summary>
        /// Width of the keepaway region
        /// </summary>
        public float Keepaway_Width;
        
        /// <summary>
        /// Corner Kick margin
        /// </summary>
        public float Ckick_margin;
        
        
        /// <summary>
        /// Offside active area size
        /// </summary>
        public float Offside_Active_Area_Size;
    
        
        /// <summary>
        /// Time in a half (in seconds)
        /// </summary>
        public int Half_Time;

        
        
        /// <summary>
        /// Number of normal halves
        /// </summary>
        public int Nr_normal_halfs;

        
        /// <summary>
        /// Number of extra halves
        /// </summary>
        public int Nr_extra_halfs;


        
        /// <summary>
        /// Do we have penality shoot outs after extra halves?
        /// </summary>
        public bool Penalty_Shoot_Outs;

        
        
        /// <summary>
        /// Number of penalty kicks
        /// </summary>
        public int Pen_Nr_Kicks;

        
        /// <summary>
        /// Maximum number of penalty kicks
        /// </summary>
        public int Pen_Max_Extra_Kicks;

        
        /// <summary>
        /// Distance to place ball from goal for penalties
        /// </summary>
        public float Pen_Dist_X;
        
        /// <summary>
        /// Do we randomly select a winner on draw?
        /// </summary>
        public bool Pen_Random_Winner;

        /// <summary>
        /// Do we allow multiple kicks on ball from an attacker?
        /// </summary>
        public bool Pen_Allow_Mult_Kicks;

        /// <summary>
        /// Maximum distance from goal for goalie?
        /// </summary>
        public float Pen_Max_Goalie_Dist_X;

        
        /// <summary>
        /// The simulator step interval in millseconds
        /// </summary>
        public int Simulator_Step;

        

        /// <summary>
        /// Cycles for goalie catch ban
        /// </summary>
        public int Catch_Ban_Cycle;

        
        /// <summary>
        /// Do we use to the offside rule?
        /// </summary>
        public bool Use_Offside;

        /// <summary>
        /// Do we allow offside kicks?
        /// </summary>
        public bool Forbid_Kick_off_Offside;

        /// <summary>
        /// Margin for offside kicks
        /// </summary>
        public float Offside_Kick_Margin;

        /// <summary>
        /// Distance to cut off audio
        /// </summary>
        public float Audio_Cut_Dist;

        /// <summary>
        /// The quantiziation step for other objects
        /// </summary>
        public float Quantize_Step;

        /// <summary>
        /// The quantiziation step for landmarks
        /// </summary>
        public float Quantize_step_l;
        
        /// <summary>
        /// Do we send the full state for the left team?
        /// </summary>
        public bool Fullstate;
        
        
        /// <summary>
        /// Maximum size of a say message
        /// </summary>
        public int Say_Msg_Size;

        
        /// <summary>
        /// Maximum player hear capacity
        /// </summary>
        public int Hear_Max;

        /// <summary>
        /// Increment for player hear capacity
        /// </summary>
        public int Hear_Inc;

        /// <summary>
        /// Decay for player hear capacity
        /// </summary>
        public int Hear_Decay;

        /// <summary>
        /// Distance for tackling
        /// </summary>
        public float Tackle_Dist;

        /// <summary>
        /// Distance for back tackles
        /// </summary>
        public float Tackle_Back_Dist;

        /// <summary>
        /// Width of allowable area for tackling
        /// </summary>
        public float Tackle_Width;

        /// <summary>
        /// Exponent for tackling
        /// </summary>
        public float Tackle_Exponent;

        /// <summary>
        /// The number of cycles for tackling
        /// </summary>
        public int Tackle_Cycles;

        
        /// <summary>
        /// The tackle power rate
        /// </summary>
        public float Tackle_Power_Rate;


        /// <summary>
        /// Enforce Free kick faults?
        /// </summary>
        public bool Free_Kick_Faults;

        
        /// <summary>
        /// Allow back passes?
        /// </summary>
        public bool Back_Passes;

        /// <summary>
        /// Force proper goal kicks?
        /// </summary>
        public bool Proper_Goal_Kicks;

        
        /// <summary>
        /// The velocity of the ball when it is stopped
        /// </summary>
        public float Stopped_Ball_Vel;

        
        /// <summary>
        /// The maximum number of kicks for goal kicks
        /// </summary>
        public int Max_Goal_Kicks;


        /// <summary>
        /// Are we in auto mode?
        /// </summary>
        public bool Auto_Mode;

       
        
        /// <summary>
        /// Threshold for determining whether the ball is stuck
        /// </summary>
        public float Ball_Stuck_Area;
        

        
        /// <summary>
        /// Maximum power for tackles
        /// </summary>
        public float Max_Tackle_Power;
        
        /// <summary>
        /// The maxmim power of back tackles
        /// </summary>
        public float Max_Back_Tackle_Power;

        /// <summary>
        /// The minimum value of the maximum speed of players
        /// </summary>
        public float Player_Speed_Max_Min;

        
        /// <summary>
        /// Extra stamina given to each agent (minimum stamina an agent will always have access to)
        /// </summary>
        public float Extra_Stamina;


        /// <summary>
        /// Length of time for extra halves in seconds
        /// </summary>
        public int Extra_Half_time;

        /// <summary>
        /// The stamina capacity (how much total stamina for an agent before no more to refresh stamina)
        /// </summary>
        public float Stamina_Capacity;
        
        /// <summary>
        /// The maximum dash angle
        /// </summary>
        public float Max_Dash_Angle;

        
        /// <summary>
        /// The minimum dash angle
        /// </summary>
        public float Min_Dash_Angle;

        /// <summary>
        /// The step for permissible angles for dashing
        /// </summary>
        public float Dash_Angle_Step;

        /// <summary>
        /// The rate adjustment for dashing sideways
        /// </summary>
        public float Side_Dash_Rate;
        
        /// <summary>
        /// The rate adjustment for dashing backward
        /// </summary>
        public float Back_Dash_Rate;

        /// <summary>
        /// Maximum dash power for a player
        /// </summary>
        public float Max_Dash_Power;

        /// <summary>
        /// Minimum dash power for a player
        /// </summary>
        public float Min_Dash_Power;

        

        /// <summary>
        /// Do we hide non-friendly teams?  (i.e. any team not your own is considered as part of opponent team?)
        /// </summary>
        public bool Hide_teams;

       
        /// <summary>
        /// Minimum catch probability
        /// </summary>
        public float Min_catch_probablity;

        /// <summary>
        /// Reliable catch area
        /// </summary>
        public float Reliable_catch_area_l;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for ServerParam class.
        /// </summary>
        public ServerParam()
        {
            setDefaults();
        }

        #endregion

        #region Class Methods

        /// <summary>
        /// Initalizes the server paramters (including Player Parameters, CVSSaver Parameters, and command lines).
        /// </summary>
        /// <param name="args">Arguments from the command line.</param>
        /// <returns>Returns true after successful intialization.</returns>
        public static bool Initialize(string[] args)
        {

            Instance = new ServerParam();
            

            string configDirectory = CONF_DIR;

            string environmentalVariableConfDirectory = System.Environment.GetEnvironmentVariable("RCSS_CONF_DIR");

            if (environmentalVariableConfDirectory != null)
            {

                configDirectory = environmentalVariableConfDirectory;
            }

            System.IO.DirectoryInfo configPath;

            try
            {

                configPath = new System.IO.DirectoryInfo(Utilities.tildeExpand(configDirectory));

                if (!configPath.Exists)
                {

                    configPath.Create();
                    if (!configPath.Exists)
                    {
                        Console.Error.WriteLine("Could not read or create the config directory '"
                                                + configPath.FullName + "'");

                        return false;
                    }
                }

            }
            catch (System.Exception exception)
            {

                Console.Error.WriteLine("Exception caught! " + exception.Message
                                         + System.Environment.NewLine + "Could not read or create config directory '"
                                         + Utilities.tildeExpand(configDirectory) + "' in " + exception.TargetSite);
                return false;

            }

            System.IO.FileInfo filePath = new System.IO.FileInfo(configPath.FullName + System.IO.Path.DirectorySeparatorChar + SERVER_CONF);


            if (!Instance.parseCreateConf(filePath))
            {
                Console.Error.WriteLine("could not create or parse configuration file '"
                                        + filePath.ToString() + "'");
                return false;
            }




            if (!PlayerParam.Initialize())
            {
                return false;
            }



            return true;
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Either reads in or creates a config file.
        /// </summary>
        /// <param name="configFile">Info about the file to be opened/created.</param>
        /// <returns>Returns true if it succesfully reads or creates the config file.</returns>
        private bool parseCreateConf(System.IO.FileInfo configFile)
        {
            string path = configFile.FullName;


            if (!configFile.Exists)
            {
                System.IO.Stream newConfigFile = new System.IO.FileStream(configFile.FullName,System.IO.FileMode.CreateNew, System.IO.FileAccess.Write);


                serialize.Serialize(newConfigFile, this);
                newConfigFile.Close();
                return true;
            }
            else
            {
                System.IO.StreamReader oldConfigFile = new System.IO.StreamReader(configFile.FullName);
                Instance = (ServerParam)serialize.Deserialize(oldConfigFile);
                oldConfigFile.Close();
                return true;
            }
        }

        /// <summary>
        /// Sets the default values for all the parameters.
        /// </summary>
        private void setDefaults()
        {

            /* set default parameter */
            Goal_width = GOAL_WIDTH;

            Inertia_moment = IMPARAM;
            Player_size = PLAYER_SIZE;
            Player_decay = PLAYER_DECAY;
            Player_rand = PLAYER_RAND;
            Player_weight = PLAYER_WEIGHT;
            Player_speed_max = PLAYER_SPEED_MAX;
            Player_accel_max = PLAYER_ACCEL_MAX;

            Stamina_max = STAMINA_MAX;
            Stamina_inc_max = STAMINA_INC_MAX;
            Recover_init = 1.0f;
            Recover_dec_thr = RECOVERY_DEC_THR;
            Recover_min = RECOVERY_MIN;
            Recover_dec = RECOVERY_DEC;
            Effort_init = 1.0f;
            Effort_dec_thr = EFFORT_DEC_THR;
            Effort_min = EFFORT_MIN;
            Effort_dec = EFFORT_DEC;
            Effort_inc_thr = EFFORT_INC_THR;
            Effort_inc = EFFORT_INC;
            // pfr 8/14/00: for RC2000 evaluation
            Kick_rand = KICK_RAND;
            Prand_factor = PRAND_FACTOR_L;
            
            Kick_rand_factor = KICK_RAND_FACTOR_L;
            
            M_ball_size = BALL_SIZE;
            Ball_decay = BALL_DECAY;
            Ball_rand = BALL_RAND;
            Ball_weight = BALL_WEIGHT;
            Ball_speed_max = BALL_SPEED_MAX;
            // th 6.3.00
            Ball_accel_max = BALL_ACCEL_MAX;
            //
            Dash_power_rate = DASHPOWERRATE;
            Kick_power_rate = KICKPOWERRATE;
            M_kickable_margin = KICKABLE_MARGIN;
            M_control_radius = CONTROL_RADIUS;

            Maxpower = MAXPOWER;
            Minpower = MINPOWER;
            MaxMoment = MAXMOMENT;
            MinMoment = MINMOMENT;
            MaxNeckMoment = MAX_NECK_MOMENT;
            MinNeckMoment = MIN_NECK_MOMENT;
            MaxNeckAngle = MAX_NECK_ANGLE;
            MinNeckAngle = MIN_NECK_ANGLE;

            Visible_Angle = VISIBLE_ANGLE;
            Visible_Distance = VISIBLE_DISTANCE;

            Wind_Dir = WIND_DIR;
            Wind_Force = WIND_FORCE;
            Wind_Angle = 0.0f;
            Wind_Rand = WIND_RAND;

            Catchable_area_l = GOALIE_CATCHABLE_AREA_LENGTH;
            Catchable_area_w = GOALIE_CATCHABLE_AREA_WIDTH;
            Catch_probability = GOALIE_CATCHABLE_POSSIBILITY;
            Goalie_max_moves = GOALIE_MAX_MOVES;

            Keepaway = false;
            Keepaway_Length = KEEPAWAY_LENGTH;
            Keepaway_Width = KEEPAWAY_WIDTH;

            Ckick_margin = CORNER_KICK_MARGIN;
            Offside_Active_Area_Size = OFFSIDE_ACTIVE_AREA_SIZE;

            Wind_None = false;
            Wind_Random = false;

            Nr_normal_halfs = NR_NORMAL_HALFS;
            Nr_extra_halfs = NR_EXTRA_HALFS;
            Penalty_Shoot_Outs = PENALTY_SHOOT_OUTS;

            Pen_Nr_Kicks = PEN_NR_KICKS;
            Pen_Max_Extra_Kicks = PEN_MAX_EXTRA_KICKS;
            Pen_Dist_X = PEN_DIST_X;
            Pen_Random_Winner = PEN_RANDOM_WINNER;
            Pen_Allow_Mult_Kicks = PEN_ALLOW_MULT_KICKS;
            Pen_Max_Goalie_Dist_X = PEN_MAX_GOALIE_DIST_X;
            

            Simulator_Step = SIMULATOR_STEP_INTERVAL_MSEC;
           

            Catch_Ban_Cycle = GOALIE_CATCH_BAN_CYCLE;
            

            Use_Offside = true;
            Forbid_Kick_off_Offside = true;
            Offside_Kick_Margin = OFFSIDE_KICK_MARGIN;

            Audio_Cut_Dist = AUDIO_CUT_OFF_DIST;

            Quantize_Step = DIST_QSTEP;
            Quantize_step_l = LAND_QSTEP;

            Fullstate = false;

            Say_Msg_Size = (int)SAY_MSG_SIZE;
            Hear_Max = (int)HEAR_MAX;
            Hear_Inc = (int)HEAR_INC;
            Hear_Decay = (int)HEAR_DECAY;

            Tackle_Dist = TACKLE_DIST;
            Tackle_Back_Dist = TACKLE_BACK_DIST;
            Tackle_Width = TACKLE_WIDTH;
            Tackle_Exponent = TACKLE_EXPONENT;
            Tackle_Cycles = (int)TACKLE_CYCLES;
            Tackle_Power_Rate = TACKLE_POWER_RATE;

            Free_Kick_Faults = FREE_KICK_FAULTS;
            Back_Passes = BACK_PASSES;

            Proper_Goal_Kicks = PROPER_GOAL_KICKS;
            Stopped_Ball_Vel = STOPPED_BALL_VEL;
            Max_Goal_Kicks = MAX_GOAL_KICKS;


            Auto_Mode = S_AUTO_MODE;
            

            // 11.0.0
            Ball_Stuck_Area = BALL_STUCK_AREA;


            // 12.0.0
            Max_Tackle_Power = MAX_TACKLE_POWER;
            Max_Back_Tackle_Power = MAX_BACK_TACKLE_POWER;
            Player_Speed_Max_Min = PLAYER_SPEED_MAX_MIN;
            Extra_Stamina = EXTRA_STAMINA;


            // 13.0.0
            Stamina_Capacity = STAMINA_CAPACITY;
            Max_Dash_Angle = MAX_DASH_ANGLE;
            Min_Dash_Angle = MIN_DASH_ANGLE;
            Dash_Angle_Step = DASH_ANGLE_STEP;
            Side_Dash_Rate = SIDE_DASH_RATE;
            Back_Dash_Rate = BACK_DASH_RATE;
            Max_Dash_Power = MAX_DASH_POWER;
            Min_Dash_Power = MIN_DASH_POWER;


            Half_Time = HALF_TIME;
            Extra_Half_time = EXTRA_HALF_TIME;
            Hide_teams = HIDE_TEAMS;
            
            Server_Random_seed = RANDOM_SEED;

            Reliable_catch_area_l = GOALIE_CATCHABLE_AREA_LENGTH;
            Min_catch_probablity = 1.0f;

            KickableArea = M_player_size + M_kickable_margin + M_ball_size;
            ControlRadiusWidth = M_control_radius - M_player_size;

        }

        #endregion

        #region Constants and Default Values
        // default values

        // Version 101 (C#, .NET transition)
        private const bool LEGACY = false;
        private const int MAX_TEAMS = 2;
        private const int MAX_PLAYERS = 11;
        private const int RANDOM_SEED = -1;
        private const int DEFAULT_NUM_BALLS = 1;
        private const bool HIDE_TEAMS = false;

        private const int DEFAULT_PORT_NUMBER = 6000;
        private const int COACH_PORT_NUMBER = 6001;
        private const int OLCOACH_PORT_NUMBER = 6002;

        private const int SIMULATOR_STEP_INTERVAL_MSEC = 100; /* milli-sec */
        private const int UDP_RECV_STEP_INTERVAL_MSEC = 10; /* milli-sec */
        private const int UDP_SEND_STEP_INTERVAL_MSEC = 150; /* milli-sec */
        private const int SENSE_BODY_INTERVAL_MSEC = 100; /* milli-sec */
        private const int SEND_VISUALINFO_INTERVAL_MSEC = 100; /* milli-sec */

        private const int HALF_TIME = 300;
        private const int DROP_TIME = 200;


        public const float PITCH_LENGTH = 105.0f;
        public const float PITCH_WIDTH = 68.0f;
        public const float PITCH_MARGIN = 5.0f;
        public const float CENTER_CIRCLE_R = 9.15f;
        public const float PENALTY_AREA_LENGTH = 16.5f;
        public const float PENALTY_AREA_WIDTH = 40.32f;
        public const float GOAL_AREA_LENGTH = 5.5f;
        public const float GOAL_AREA_WIDTH = 18.32f;
        public const float GOAL_WIDTH = 14.02f;
        public const float GOAL_DEPTH = 2.44f;
        public const float PENALTY_SPOT_DIST = 11.0f;
        public const float CORNER_ARC_R = 1.0f;
        public const float KICK_OFF_CLEAR_DISTANCE = CENTER_CIRCLE_R;

        public const float CORNER_KICK_MARGIN = 1.0f;

        private const float KEEPAWAY_LENGTH = 20.0f;
        private const float KEEPAWAY_WIDTH = 20.0f;

        private const float BALL_SIZE = 0.085f;
        private const float BALL_DECAY = 0.94f;
        private const float BALL_RAND = 0.05f;
        private const float BALL_WEIGHT = 0.2f;
        private const float BALL_T_VEL = 0.001f;
        private const float BALL_SPEED_MAX = 3.0f;
        // th 6.3.00
        private const float BALL_ACCEL_MAX = 2.7f;

        private const float PLAYER_SIZE = 0.3f;
        private const float PLAYER_WIDGET_SIZE = 1.0f;
        private const float PLAYER_DECAY = 0.4f;
        private const float PLAYER_RAND = 0.1f;
        private const float PLAYER_WEIGHT = 60.0f;
        private const float PLAYER_SPEED_MAX = 1.05f;
        // th 6.3.00
        private const float PLAYER_ACCEL_MAX = 1.0f;
        //
        private const float IMPARAM = 5.0f; /* Inertia-Moment Parameter */

        private const float STAMINA_MAX = 8000.0f;
        private const float STAMINA_INC_MAX = 45.0f;
        private const float RECOVERY_DEC_THR = 0.3f;
        private const float RECOVERY_DEC = 0.002f;
        private const float RECOVERY_MIN = 0.5f;
        private const float EFFORT_DEC_THR = 0.3f;
        private const float EFFORT_DEC = 0.005f;
        private const float EFFORT_MIN = 0.6f;
        private const float EFFORT_INC_THR = 0.6f;
        private const float EFFORT_INC = 0.01f;

        private const float KICK_RAND = 0.1f;
        private const float PRAND_FACTOR_L = 1.0f;
        private const float PRAND_FACTOR_R = 1.0f;
        private const float KICK_RAND_FACTOR_L = 1.0f;
        private const float KICK_RAND_FACTOR_R = 1.0f;

        private const float GOALIE_CATCHABLE_POSSIBILITY = 1.0f;
        private const float GOALIE_CATCHABLE_AREA_LENGTH = 1.2f;
        private const float GOALIE_CATCHABLE_AREA_WIDTH = 1.0f;
        private const int GOALIE_CATCH_BAN_CYCLE = 5;
        private const int GOALIE_MAX_MOVES = 2;

        private const float VISIBLE_ANGLE = 90.0f;
        private const float VISIBLE_DISTANCE = 3.0f;
        private const float AUDIO_CUT_OFF_DIST = 50.0f;

        private const float DASHPOWERRATE = 0.06f;
        private const float KICKPOWERRATE = 0.027f;
        private const float MAXPOWER = 100.0f;
        private const float MINPOWER = -100.0f;

        private const float KICKABLE_MARGIN = 0.7f;
        private const float CONTROL_RADIUS = 2.0f;

        private const float DIST_QSTEP = 0.1f;
        private const float LAND_QSTEP = 0.01f;
        private const float DIR_QSTEP = 0.1f;

        private const float MAXMOMENT = 180;
        private const float MINMOMENT = -180;

        private const float MAX_NECK_MOMENT = 180;
        private const float MIN_NECK_MOMENT = -180;

        private const float MAX_NECK_ANGLE = 90;
        private const float MIN_NECK_ANGLE = -90;

        private const int DEF_SAY_COACH_MSG_SIZE = 128;
        private const int DEF_SAY_COACH_CNT_MAX = 128;

        private const float WIND_DIR = 0.0f;
        private const float WIND_FORCE = 0.0f;
        private const float WIND_RAND = 0.0f;
        public const float WIND_WEIGHT = 10000.0f;

        private const float OFFSIDE_ACTIVE_AREA_SIZE = 2.5f;
        private const float OFFSIDE_KICK_MARGIN = 9.15f;

        private const string LANDMARK_FILE = "rcssserver-landmark.xml";
        private const string CONF_DIR = ".";
        private const string SERVER_CONF = "server.conf";
        private const string OLD_SERVER_CONF = "rcssserver-server.conf";


        private const int KAWAY_START = -1;


        private const uint SAY_MSG_SIZE = 10;
        private const uint HEAR_MAX = 1;
        private const uint HEAR_INC = 1;
        private const uint HEAR_DECAY = 1;

        private const float TACKLE_DIST = 2.0f;
        private const float TACKLE_BACK_DIST = 0.0f;
        private const float TACKLE_WIDTH = 1.25f;
        private const float TACKLE_EXPONENT = 6.0f;
        private const uint TACKLE_CYCLES = 10;
        private const float TACKLE_POWER_RATE = 0.027f;

        private const int NR_NORMAL_HALFS = 2;
        private const int NR_EXTRA_HALFS = 2;
        private const bool PENALTY_SHOOT_OUTS = true;

        private const int PEN_BEFORE_SETUP_WAIT = 10;
        private const int PEN_SETUP_WAIT = 70;
        private const int PEN_READY_WAIT = 10;
        private const int PEN_TAKEN_WAIT = 150;
        private const int PEN_NR_KICKS = 5;
        private const int PEN_MAX_EXTRA_KICKS = 5;
        private const bool PEN_RANDOM_WINNER = false;
        private const float PEN_DIST_X = 42.5f;
        private const float PEN_MAX_GOALIE_DIST_X = 14;
        private const bool PEN_ALLOW_MULT_KICKS = true;
        private const bool PEN_COACH_MOVES_PLAYERS = true;



        private const bool FREE_KICK_FAULTS = true;
        private const bool BACK_PASSES = true;

        private const bool PROPER_GOAL_KICKS = false;
        private const float STOPPED_BALL_VEL = 0.01f;
        private const int MAX_GOAL_KICKS = 3;


        private const bool S_AUTO_MODE = false;
 

        // 11.0.0
        private const float BALL_STUCK_AREA = 3.0f;
        // 12.0.0
        private const float MAX_TACKLE_POWER = 100.0f;
        private const float MAX_BACK_TACKLE_POWER = 0.0f;
        private const float PLAYER_SPEED_MAX_MIN = 0.75f;
        private const float EXTRA_STAMINA = 50.0f;
        private const int SYNCH_SEE_OFFSET = 0;
        // 12.1.3
        private const int EXTRA_HALF_TIME = 100;
        // 13.0.0
        private const float STAMINA_CAPACITY = 148600.0f;
        private const float MAX_DASH_ANGLE = 180.0f;
        private const float MIN_DASH_ANGLE = -180.0f;
        private const float DASH_ANGLE_STEP = 90.0f;
        private const float SIDE_DASH_RATE = 0.25f;
        private const float BACK_DASH_RATE = 0.5f;
        private const float MAX_DASH_POWER = 100.0f;
        private const float MIN_DASH_POWER = -100.0f;

        #endregion

    }
}
