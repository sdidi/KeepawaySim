// -*-C#-*-

/***************************************************************************
                                   Weather.cs
                    Class that represents wind in RoboCup.
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


namespace RoboCup.Environment
{
    
    /// <summary>
    /// Class that represents the Weather on the field (i.e. the wind)
    /// </summary>
    internal class Weather
    {
        #region Instance Variables

        /// <summary>
        /// Vector representing the wind strength/direction
        /// </summary>
        private Vector windVector;

        internal Vector WindVector
        {
            get { return windVector; }
            set { windVector.X = value.X; windVector.Y = value.Y; windVector.Z = value.Z; }
        }

        /// <summary>
        /// Randomness applied to the wind
        /// </summary>
        internal float WindRandomness;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the Weather class
        /// </summary>
        public Weather()
        {
            windVector = new Vector(0, 0);
            WindRandomness = 0.0f;

        }

        #endregion

        #region Instance Methods
        /// <summary>
        /// Intializes a Weather object based on the ServerParameters
        /// </summary>
        internal void Initalize()
        {
            if (ServerParam.Instance.Wind_None)
            {
                WindVector.X = 0;
                WindVector.Y = 0;
                WindVector.Z = 0;
                WindRandomness = 0.0f;
            }
            else if (ServerParam.Instance.Wind_Random)
            {
                WindVector = Vector.fromPolar((float)Utilities.CustomRandom.NextDouble(0, 100), (float)Utilities.CustomRandom.NextDouble(-Math.PI, Math.PI));
                WindRandomness = (float)Utilities.CustomRandom.NextDouble(0, 0.5);
            }
            else
            {
                WindVector = Vector.fromPolar(ServerParam.Instance.Wind_Force,
                                                  (float)(Math.PI / 180.0) * ServerParam.Instance.Wind_Dir);
                WindRandomness = ServerParam.Instance.Wind_Rand;
            }


        }

        #endregion

    }
}
