// -*-C#-*-

/***************************************************************************
                                   PlayerData.cs
                   Contains class that represents visual/state information.
                             -------------------
    begin                : JUL-2009
    credit               : Implementation done by Phillip Verbancsics of the
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
using System.Text;

namespace RoboCup
{
    /// <summary>
    /// Class that holds a player's body (sense) information
    /// </summary>
    public class BodyData
    {
        /// <summary>
        /// The time of the sense
        /// </summary>
        public int Time;
        /// <summary>
        /// Do we have high quality vision?
        /// </summary>
        public bool HighQuality;
        /// <summary>
        /// The view width.
        /// </summary>
        public string ViewWidth;
        /// <summary>
        /// The player's remaining stamina.
        /// </summary>
        public float Stamina;
        /// <summary>
        /// The player's current level of effort.
        /// </summary>
        public float Effort;
        /// <summary>
        /// The player's remaining stamina capacity.
        /// </summary>
        public float StaminaCapacity;
        /// <summary>
        /// The magnitude of the player's velocity.
        /// </summary>
        public float VelocityMagnitude;
        /// <summary>
        /// The heading of the player's velocity.
        /// </summary>
        public float VelocityHeading;
        /// <summary>
        /// The angle of the player's neck.
        /// </summary>
        public float NeckAngle;
        /// <summary>
        /// The angle of the player's body
        /// </summary>
        public float BodyAngle;
        /// <summary>
        /// Number of times player has kicked.
        /// </summary>
        public int KickCount;
        /// <summary>
        /// Number of times player has dashed.
        /// </summary>
        public int DashCount;
        /// <summary>
        /// Number of times player has turned.
        /// </summary>
        public int TurnCount;
        /// <summary>
        /// Number of times player has said something.
        /// </summary>
        public int SayCount;
        /// <summary>
        /// Number of times player has turned neck.
        /// </summary>
        public int TurnNeckCount;
        /// <summary>
        /// Number of times player has attempted to catch.
        /// </summary>
        public int CatchCount;
        /// <summary>
        /// Number of times player has moved.
        /// </summary>
        public int MoveCount;
        /// <summary>
        /// Number of times player has changed view.
        /// </summary>
        public int ChangeViewCount;
        /// <summary>
        /// The Team the player is currently focused on.
        /// </summary>
        public string FocusTeam;
        /// <summary>
        /// The uniform number of the player's focus.
        /// </summary>
        public int FocusUnum;
        /// <summary>
        /// The number of times the player has focused.
        /// </summary>
        public int FocusCount;
        /// <summary>
        /// Cycles of tackling
        /// </summary>
        public int TackleCycles;
        /// <summary>
        /// Number of times player has kicked.
        /// </summary>
        public int TackleCount;
        /// <summary>
        /// Has the player collided with ball?
        /// </summary>
        public bool BallCollide;
        /// <summary>
        /// Has the player collided with player?
        /// </summary>
        public bool PlayerCollide;
        /// <summary>
        /// Has the player collided with post?
        /// </summary>
        public bool PostCollide;

    }
    
    /// <summary>
    /// Class that holds visual data for non-full state visuals.
    /// </summary>
    public class VisualData
    {
        /// <summary>
        /// The time of the visual data.
        /// </summary>
        public int Time;
        /// <summary>
        /// The list of players who are visible.
        /// </summary>
        public List<VisualPlayer> players = new List<VisualPlayer>();
        /// <summary>
        /// The list of visible balls.
        /// </summary>
        public List<VisualObject> balls = new List<VisualObject>();
        /// <summary>
        /// The list of visible flags.
        /// </summary>
        public List<VisualObject> flags = new List<VisualObject>();
        /// <summary>
        /// The list of visible lines.
        /// </summary>
        public List<VisualObject> lines = new List<VisualObject>();
    }

    /// <summary>
    /// Visual information for a player for non-full state
    /// </summary>
    public class VisualPlayer
    {
        /// <summary>
        /// Name of the object (p for player)
        /// </summary>
        public string Name;
        /// <summary>
        /// The teamname of the player
        /// </summary>
        public string Teamname;
        /// <summary>
        /// The uniform Id (if it is visible)
        /// </summary>
        public int? Id;
        /// <summary>
        /// The direction of the player (if able to be determined)
        /// </summary>
        public float? Direction;
        /// <summary>
        /// The distance of the player (if able to be determined)
        /// </summary>
        public float? Distance;
        /// <summary>
        /// The change in distance (if able to be determined)
        /// </summary>
        public float? DistChange;
        /// <summary>
        /// The change in direction (if able to be determined)
        /// </summary>
        public float? DirChange;
        /// <summary>
        /// The player's body direction (if able to be determined)
        /// </summary>
        public float? BodyDir;
        /// <summary>
        /// The player's neck angle (if able to be determined)
        /// </summary>
        public float? HeadDir;
        /// <summary>
        /// Tells if the player is tackling 
        /// </summary>
        public bool? Tackling;
        /// <summary>
        /// Tells if the player is kicking
        /// </summary>
        public bool? Kicking;
        /// <summary>
        /// Tells if the player is a goalie
        /// </summary>
        public bool? Goalie;

    }

    /// <summary>
    /// Visual information for a non-full state object
    /// </summary>
    public class VisualObject
    {
        /// <summary>
        /// Name of the object
        /// </summary>
        public string Name;
        /// <summary>
        /// Object ID (if visible)
        /// </summary>
        public int? Id;
        /// <summary>
        /// Object direction (if visible)
        /// </summary>
        public float? Direction;
        /// <summary>
        /// Object distance (if visible)
        /// </summary>
        public float? Distance;
        /// <summary>
        /// Object distance change (if visible)
        /// </summary>
        public float? DistChange;
        /// <summary>
        /// Object direction change (if visible)
        /// </summary>
        public float? DirChange;
    }

    /// <summary>
    /// Class that holds full state information
    /// </summary>
    public class FullStateData
    {
        /// <summary>
        /// The time of the full state information
        /// </summary>
        public int Time;
        /// <summary>
        /// The current playmode
        /// </summary>
        public PlayMode Playmode;
        /// <summary>
        /// Collection of players
        /// </summary>
        public Dictionary<int, FullPlayerData> Players = new Dictionary<int,FullPlayerData>();
        /// <summary>
        /// Collection of balls
        /// </summary>
        public Dictionary<int,FullBallData> Balls = new Dictionary<int,FullBallData>();
    }

    /// <summary>
    /// Represents the full state player information
    /// </summary>
    public class FullPlayerData
    {
        /// <summary>
        /// The player's team
        /// </summary>
        public string Team;
        /// <summary>
        /// The player's uniform number
        /// </summary>
        public int Unum;
        /// <summary>
        /// Indicates if the player is a goalie
        /// </summary>
        public bool Goalie;
        /// <summary>
        /// The X location
        /// </summary>
        public float X;
        /// <summary>
        /// The Y location
        /// </summary>
        public float Y;
        /// <summary>
        /// Velocity in X direction
        /// </summary>
        public float VelX;
        /// <summary>
        /// Velocity in Y direction
        /// </summary>
        public float VelY;
        /// <summary>
        /// The player's body angle
        /// </summary>
        public float BodyDirection;
        /// <summary>
        /// The player's neck angle
        /// </summary>
        public float HeadDirection;
        /// <summary>
        /// The player's stamina
        /// </summary>
        public float Stamina;
        /// <summary>
        /// The player's effort
        /// </summary>
        public float Effort;
        /// <summary>
        /// The player's recovery
        /// </summary>
        public float Recovery;
        /// <summary>
        /// The player's stamina
        /// </summary>
        public float StaminaCapacity;
        /// <summary>
        /// Indicates if kicking
        /// </summary>
        public bool Kicking;
        /// <summary>
        /// Indicates if tackling
        /// </summary>
        public bool Tackling;

    }

    /// <summary>
    /// Class that holds full-state ball data
    /// </summary>
    public class FullBallData
    {
        /// <summary>
        /// The unique ID of the ball
        /// </summary>
        public int BallId;
        /// <summary>
        /// The X location
        /// </summary>
        public float X;
        /// <summary>
        /// The Y location
        /// </summary>
        public float Y;
        /// <summary>
        /// Velocity in X direction
        /// </summary>
        public float VelX;
        /// <summary>
        /// Velocity in Y direction
        /// </summary>
        public float VelY;

    }
}
