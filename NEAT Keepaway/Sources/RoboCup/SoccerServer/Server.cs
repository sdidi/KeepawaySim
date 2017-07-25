// -*-C#-*-

/***************************************************************************
                                   Server.cs
                   The drives the simulation of the stadium.
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

namespace RoboCup
{
    /// <summary>
    /// Delegate that triggers on stadium updates
    /// </summary>
    /// <param name="std"></param>
    public delegate void StadiumUpdates(Stadium std);
    /// <summary>
    /// Synchronize Timer derived from RoboCup.Server.Timer base class.
    /// Runs a Timeable object in a synchronized manner.
    /// This means the object will run as fast as possible,
    /// not dependent on real time.
    /// </summary>
    public class Server
    {
        #region Constructors

        /// <summary>
        /// Reference to the stadium being simulated
        /// </summary>
        Stadium simulatedStadium;
        /// <summary>
        /// Indicates whether an episode has completed (Depends on referees used)
        /// </summary>
        public bool EpisodeDone = false;
        /// <summary>
        /// Event that is triggered every time thesimulator cycles
        /// </summary>
        public event StadiumUpdates OnCycle;
        /// <summary>
        /// Server constructor
        /// </summary>
        /// <param name="stadium">Stadium being driver by the server.</param>
        public Server(Stadium stadium)
        {
            
            simulatedStadium = stadium;
            simulatedStadium.init();
            simulatedStadium.Playmode = PlayMode.PlayOn;

        }

        #endregion

        #region Methods

        /// <summary>
        /// Primary method which runs the server in a synchronized (as fast as possible) manner
        /// </summary>
        public void RunCycle()
        {

            while (simulatedStadium.Pause && !simulatedStadium.Step) { }
            simulatedStadium.Step = false;
            if (simulatedStadium.Realtime) { System.DateTime now = System.DateTime.Now; while ((System.DateTime.Now - now).TotalSeconds < 0.1) { } }

            simulatedStadium.step();

            EpisodeDone = simulatedStadium.episodeCheck();
            if (!EpisodeDone)
            {
                simulatedStadium.updateState();

                simulatedStadium.playerActions();
            }
            if (OnCycle != null)
            {
                OnCycle(simulatedStadium);
            }
            
        }

        /// <summary>
        /// Adds a referee to the stadium
        /// </summary>
        /// <param name="official">Referee to be added.</param>
        public void AddReferee(RoboCup.Referees.Referee official)
        {
            simulatedStadium.AddReferee(official);
        }

        /// <summary>
        /// Adds a player to the stadium
        /// </summary>
        /// <param name="player">Player to be added.</param>
        public void AddPlayer(RoboCup.Objects.Player player)
        {
            player.Stadium = simulatedStadium;
            player.Enable = true;
            simulatedStadium.addPlayer(player);

        }

        /// <summary>
        /// Adds another ball to the stadium
        /// </summary>
        public void AddBall()
        {
            simulatedStadium.addBall();
        }

        #endregion

        /// <summary>
        /// Sets the stadium to a new episode
        /// </summary>
        public void NewEpisode()
        {
            EpisodeDone = false;
            simulatedStadium.NewEpisode();
        }
    }
}
