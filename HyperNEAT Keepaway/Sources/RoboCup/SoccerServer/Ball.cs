// -*-C#-*-

/***************************************************************************
                                   Ball.cs
                    Class that represents a ball in RoboCup.
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


namespace RoboCup.Objects
{
    /// <summary>
    /// Class that represents a ball in RoboCup
    /// </summary>
    public class Ball : MovingObject
    {
        #region Static Variables

        /// <summary>
        /// Keeps track of the number of balls created
        /// </summary>
        static int NextBallId = 0;

        #endregion

        #region Instance Variables

        /// <summary>
        /// The unique integer identifier of the ball
        /// </summary>
        internal int BallId;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Ball.
        /// </summary>
        /// <param name="stadium">Stadium which contains the ball.</param>
        public Ball(Stadium stadium)
            : base(stadium, "b")
        {
            BallId = ++NextBallId;

        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Gets the maximum acceleration.
        /// </summary>
        /// <returns>Returns the maximum accleration.</returns>
        internal override float getMaximumAcceleration()
        {
            return MaximumAcceleration;
        }

        /// <summary>
        /// Gets the maximum speed.
        /// </summary>
        /// <returns>Returns the maximum speed.</returns>
        internal override float getMaximumSpeed()
        {
            return MaximumSpeed;
        }

        #endregion

    }
}
