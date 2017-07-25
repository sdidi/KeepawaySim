// -*-C#-*-

/***************************************************************************
                                   PlayerParam.cs
                    Class that represents Player Parameters for the RoboCup server.
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

namespace RoboCup.Parameters
{
    /// <summary>
    /// This class represents a player paramter file for the RoboCup Soccer server
    /// It is Serializable.
    /// </summary>
    [Serializable]
    public class PlayerParam
    {

        #region Constants and Class Variables

        /// <summary>
        /// Statically accessible instance of the PlayerParam class
        /// </summary>
        public static PlayerParam Instance;

        /// <summary>
        /// The default directory for the configuration file
        /// </summary>
        private const string ConfigDirectory = ".";
        /// <summary>
        /// The default file name for the configuration file
        /// </summary>
        private const string PlayerConfigName = "player.conf";

        /// <summary>
        /// The default number of player types
        /// </summary>
        private const int DefaultNumberPlayerTypes = 18;
        /// <summary>
        /// The default number of substituions allowed
        /// </summary>
        private const int DefaultSubstitutionsMax = 3;
        /// <summary>
        /// The default number of default player types
        /// </summary>
        private const int DefaultPlayerTypeMax = 1;

        /// <summary>
        /// The default speed delta minimum
        /// </summary>
        private const float DefaultPlayerSpeedMaxDeltaMin = 0.0f;
        /// <summary>
        /// The default speed delta maximum
        /// </summary>
        private const float DefaultPlayerSpeedMaxDeltaMax = 0.0f;
        /// <summary>
        /// The default stamina increment delta
        /// </summary>
        private const float DefaultStaminaIncrementMaxDeltaFactor = 0.0f;

        /// <summary>
        /// The default decay delta minimum
        /// </summary>
        private const float DefaultPlayerDecayDeltaMin = -0.1f;
        /// <summary>
        /// The default decay delta maximum
        /// </summary>
        private const float DefaultPlayerDecaryDeltaMax = 0.1f;
        /// <summary>
        /// The default ineertia moment delta factor
        /// </summary>
        private const float DefaultInertiaMomentDeltaFactor = 25.0f;

        /// <summary>
        /// Default dash power rate delta minimum
        /// </summary>
        private const float DefaultDashPowerRateDeltaMin = 0.0f;
        /// <summary>
        /// Default dash power rate delta minimum
        /// </summary>
        private const float DefaultDashPowerRateDeltaMax = 0.0f;
        /// <summary>
        /// Default player size delta factor
        /// </summary>
        private const float DefaultPlayerSizeDeltaFactor = -100.0f;

        /// <summary>
        /// Default kickable margin delta minimum
        /// </summary>
        private const float DefaultKickableMarginDeltaMin = -0.1f;
        /// <summary>
        /// Default kickable margin delta minimum
        /// </summary>
        private const float DefaultKickableMerginDeltaMax = 0.1f;
        /// <summary>
        /// Default kick random delta factor
        /// </summary>
        private const float DefaultKickRandDeltaFactor = 1.0f;

        /// <summary>
        /// Default extra stamina delta minimum
        /// </summary>
        private const float DefaultExtraStaminaDeltaMin = 0.0f;
        /// <summary>
        /// Default extra stamina delta maximum
        /// </summary>
        private const float DefaultExtraStaminaDeltaMax = 50.0f;
        /// <summary>
        /// Default effort maximum delta factor
        /// </summary>
        private const float DefaultEffortMaxDeltaFactor = -0.004f;
        /// <summary>
        /// Default effort minium delta factor
        /// </summary>
        private const float DefaultEffortMinDeltaFactor = -0.004f;

        /// <summary>
        /// Default random number generator seed
        /// </summary>
        private const int DefaultRandomSeed = -1;

        /// <summary>
        /// Default new dash power rate delta minimum
        /// </summary>
        private const float DefaultNewDashPowerRateDeltaMin = -0.0012f;
        /// <summary>
        /// Default new dash power rate delta maximum
        /// </summary>
        private const float DefaultNewDashPowerRateDeltamax = 0.0008f;
        /// <summary>
        /// Default new stamina increment maximum delta factor
        /// </summary>
        private const float DefaultNewStaminaIncMaxDeltaFactor = -6000.0f;


        #endregion

        #region Properties and Instance Variables

        public int Player_types ;

        public int Subs_max ;

        public int Pt_max ;

        public bool Allow_mult_default_type ;

        public float Player_speed_max_delta_min ;

        public float Player_speed_max_delta_max ;

        public float Stamina_inc_max_delta_factor ;

        public float Player_decay_delta_min ;

        public float Player_decay_delta_max ;

        public float Inertia_moment_delta_factor ;

        public float Dash_power_rate_delta_min ;

        public float Dash_power_rate_delta_max ;

        public float Player_size_delta_factor ;

        public float Kickable_margin_delta_min ;

        public float Kickable_margin_delta_max ;

        public float Kick_rand_delta_factor ;

        public float Extra_stamina_delta_min ;

        public float Extra_stamina_delta_max ;

        public float Effort_max_delta_factor ;

        public float Effort_min_delta_factor ;

        public float New_dash_power_rate_delta_min ;

        public float New_dash_power_rate_delta_max ;

        public float New_stamina_inc_max_delta_factor ;

        public int Random_seed ;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for the PlayerParam class
        /// </summary>
        public PlayerParam()
        {
            setDefaults();

        }

        #endregion

        #region Class Methods
        /// <summary>
        /// Initializes the statically held instance of the PlayerParam class.
        /// </summary>
        /// <returns>True iff initialization successful.</returns>
        public static bool Initialize()
        {
            Instance = new PlayerParam();


            string configDirectory = ConfigDirectory;
            string environmentalVariableConfigDirectory = System.Environment.GetEnvironmentVariable("RCSS_CONF_DIR");
            if (environmentalVariableConfigDirectory != null)
            {
                configDirectory = environmentalVariableConfigDirectory;
            }
            System.IO.FileInfo filePath;

            System.IO.DirectoryInfo configPath;
            try
            {

                configPath = new System.IO.DirectoryInfo(Utilities.tildeExpand(configDirectory));
                if (configPath.Exists)
                {

                    filePath = new System.IO.FileInfo(configPath.FullName + System.IO.Path.DirectorySeparatorChar + PlayerConfigName);

                }
                else
                {

                    configPath.Create();
                    filePath = new System.IO.FileInfo(configPath.FullName + System.IO.Path.DirectorySeparatorChar + PlayerConfigName);

                }

            }
            catch (System.Exception excption)
            {
                Console.Error.WriteLine(" Exception caught! " + excption.Message
                          + System.Environment.NewLine + "Could not read config directory '"
                          + Utilities.tildeExpand(configDirectory) + "'"
                          + excption.StackTrace);
                return false;
            }


            if (!Instance.parseCreateConf(filePath))
            {
                Console.Error.WriteLine("could not parse configuration file '"
                          + filePath.FullName);
                return false;
            }


            return true;
        }

        #endregion

        #region Instance Methods
        /// <summary>
        /// Sets the default value for the PlayerParam class
        /// </summary>
        public void setDefaults()
        {
            Player_types = DefaultNumberPlayerTypes;
            Subs_max = DefaultSubstitutionsMax;
            Pt_max = DefaultPlayerTypeMax;

            Allow_mult_default_type = false;

            Player_speed_max_delta_min = DefaultPlayerSpeedMaxDeltaMin;
            Player_speed_max_delta_max = DefaultPlayerSpeedMaxDeltaMax;
            Stamina_inc_max_delta_factor = DefaultStaminaIncrementMaxDeltaFactor;

            Player_decay_delta_min = DefaultPlayerDecayDeltaMin;
            Player_decay_delta_max = DefaultPlayerDecaryDeltaMax;
            Inertia_moment_delta_factor = DefaultInertiaMomentDeltaFactor;

            Dash_power_rate_delta_min = DefaultDashPowerRateDeltaMin;
            Dash_power_rate_delta_max = DefaultDashPowerRateDeltaMax;
            Player_size_delta_factor = DefaultPlayerSizeDeltaFactor;

            Kickable_margin_delta_min = DefaultKickableMarginDeltaMin;
            Kickable_margin_delta_max = DefaultKickableMerginDeltaMax;
            Kick_rand_delta_factor = DefaultKickRandDeltaFactor;

            Extra_stamina_delta_min = DefaultExtraStaminaDeltaMin;
            Extra_stamina_delta_max = DefaultExtraStaminaDeltaMax;
            Effort_max_delta_factor = DefaultEffortMaxDeltaFactor;
            Effort_min_delta_factor = DefaultEffortMinDeltaFactor;

            Random_seed = DefaultRandomSeed;

            New_dash_power_rate_delta_min = DefaultNewDashPowerRateDeltaMin;
            New_dash_power_rate_delta_max = DefaultNewDashPowerRateDeltamax;
            New_stamina_inc_max_delta_factor = DefaultNewStaminaIncMaxDeltaFactor;
        }

        /// <summary>
        /// Parses or creates a configuration file.
        /// </summary>
        /// <param name="configFileName">The information of the file to parse or create.</param>
        /// <returns>True iff succesfully parsed or created config file.</returns>
        private bool parseCreateConf(System.IO.FileInfo configFileName)
        {
            string path = configFileName.FullName;
            System.Xml.Serialization.XmlSerializer serialize = new System.Xml.Serialization.XmlSerializer(typeof(PlayerParam));


            if (!configFileName.Exists)
            {
                System.IO.Stream newConfigFile = new System.IO.FileStream(configFileName.FullName, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write);


                serialize.Serialize(newConfigFile, this);
                newConfigFile.Close();
                return true;
            }
            else
            {
                System.IO.StreamReader oldConfigFile = new System.IO.StreamReader(configFileName.FullName);
                Instance = (PlayerParam)serialize.Deserialize(oldConfigFile);
                oldConfigFile.Close();
                return true;
            }
        }

        #endregion

    }
}
