// -*-C#-*-

/***************************************************************************
                                   Stadium.cs
                   The main part of the simulator
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
using System.Text;
using RoboCup.Objects;
using RoboCup.Geometry;
using RoboCup.Environment;
using RoboCup.Parameters;
using RoboCup.Referees;

namespace RoboCup
{
    /// <summary>
    /// Stadium which is where the main simulator is run
    /// </summary>
    public class Stadium
    {

        #region Instance Variable and Properties

        internal List<Player> DrawPlayerList = new List<Player>();

        /// <summary>
        /// The referees that oversee the game
        /// </summary>
        internal List<Referee> Referees = new List<Referee>();


        /// <summary>
        /// The different player types available
        /// </summary>
        internal List<HeteroPlayer> Player_types = new List<HeteroPlayer>();

        /// <summary>
        /// The field where the game takes place
        /// </summary>
        internal Field Field = new Field();

        /// <summary>
        /// The weather on the field (wind)
        /// </summary>
        internal Weather Weather = new Weather();


        PlayMode M_playmode = PlayMode.Null;
        /// <summary>
        /// The current playmode
        /// </summary>
        public PlayMode Playmode
        {
            get { return M_playmode; }
            set
            {
                string[] playmode_strings = Defines.PlaymodeStrings;
                PlayMode pm = value;
                M_playmode = pm;

                foreach (var r in Referees)
                {
                    r.doPlayModeChange(pm);
                }

                if (pm == PlayMode.KickOff_Left
                     || pm == PlayMode.KickIn_Left
                     || pm == PlayMode.FreeKick_Left
                     || pm == PlayMode.IndFreeKick_Left
                     || pm == PlayMode.CornerKick_Left
                     || pm == PlayMode.GoalKick_Left)
                {
                    KickOffSide = "l";
                }
                else if (pm == PlayMode.KickOff_Right
                          || pm == PlayMode.KickIn_Right
                          || pm == PlayMode.FreeKick_Right
                          || pm == PlayMode.IndFreeKick_Right
                          || pm == PlayMode.CornerKick_Right
                          || pm == PlayMode.GoalKick_Right)
                {
                    KickOffSide = "r";
                }
                else if (pm == PlayMode.Drop_Ball)
                {
                    KickOffSide = "n";
                }

                if (pm == PlayMode.PlayOn)
                {
                    M_last_playon_start = Time;
                }

                if (pm != PlayMode.AfterGoal_Left
                     && pm != PlayMode.AfterGoal_Right)
                {
                    sendRefereeAudio(playmode_strings[(int)pm]);
                }
            }
        }

        /// <summary>
        /// The current time in cycles of the server
        /// </summary>
        public int Time;

 
        /// <summary>
        /// The single ball in the case we are playing with only one
        /// </summary>
        public Ball Ball;

        /// <summary>
        /// List of all the balls in play
        /// </summary>
        public List<Ball> Balls = new List<Ball>();



        /// <summary>
        /// List of players
        /// </summary>
        public List<Player> Players = new List<Player>();

        /// <summary>
        /// List of objects that are movable on the field
        /// </summary>
        internal List<MovingObject> Movable_objects= new List<MovingObject>();




        public bool Pause { get; set; }
        public bool Step { get; set; }
        public bool Realtime { get; set; }


        #endregion

        #region Helper Variables to Specific Methods

        readonly Rectangle g_l;
        readonly Rectangle g_r;

        readonly Rectangle pt;
        readonly Rectangle fld;
        public string KickOffSide;
        public int TeamLeftPoint;
        public int TeamRightPoint;
        public Player BallCatcher = null;
        private int M_last_playon_start;
        public int PenaltyTakenRight;
        public int PenaltyScoredRight;
        public int PenaltyTakenLeft;
        public int PenaltyScoredLeft;
        public string PenaltyWinner;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for the Stadium class
        /// </summary>
        public Stadium()
        {

            ServerParam.Initialize(System.Environment.GetCommandLineArgs());
            fld = new Rectangle(new Vector(0.0f, 0.0f),
                        new Vector(ServerParam.PITCH_LENGTH
                                 + ServerParam.PITCH_MARGIN * 2.0f,
                                 ServerParam.PITCH_WIDTH
                                 + ServerParam.PITCH_MARGIN * 2.0f));
            pt = new Rectangle(new Vector(0.0f, 0.0f),
                         new Vector(ServerParam.PITCH_LENGTH
                                  + ServerParam.Instance.Ball_size * 2,
                                  ServerParam.PITCH_WIDTH
                                  + ServerParam.Instance.Ball_size * 2));
            g_r = new Rectangle(new Vector((+ServerParam.PITCH_LENGTH
                                     + ServerParam.GOAL_DEPTH) * 0.5f
                                   + ServerParam.Instance.Ball_size,
                                   0.0f),
                          new Vector(ServerParam.GOAL_DEPTH,
                                   ServerParam.Instance.Goal_width
                                   + ServerParam.Instance.GoalPostRadius));
            g_l = new Rectangle(new Vector((-ServerParam.PITCH_LENGTH
                                 - ServerParam.GOAL_DEPTH) * 0.5f
                               - ServerParam.Instance.Ball_size,
                               0.0f),
                      new Vector(ServerParam.GOAL_DEPTH,
                               ServerParam.Instance.Goal_width
                               + ServerParam.Instance.GoalPostRadius));
            Ball = null;
            Playmode = PlayMode.BeforeKickOff;
            Time = 0;



            // !!! registration order is very important !!!
            // TODO: fix the dependencies between referees.
          //  Referees.Add(new TimeRef(this));
          //  Referees.Add(new BallStuckRef(this));
          //  Referees.Add(new OffsideRef(this));
         //   Referees.Add(new FreeKickRef(this));
         //   Referees.Add(new TouchRef(this));
          //  Referees.Add(new CatchRef(this));
           // Referees.Add(new KeepawayRef(this));
          //  Referees.Add(new PenaltyRef(this));

            

        }

        #endregion

        #region Instance Methods

        internal void AddReferee(Referee referee)
        {
            referee.Stadium = this;
            Referees.Add(referee);
        }
        /// <summary>
        /// Initalizes the stadium and gets it ready to play
        /// </summary>
        /// <returns>True iff intialization successful</returns>
        internal bool init()
        {





            Player_types.Add(new HeteroPlayer(0));

            for (int i = 1; i < PlayerParam.Instance.Player_types; i++)
            {
                Player_types.Add(new HeteroPlayer());

            }

            Weather.Initalize();

            initObjects();

            change_play_mode(PlayMode.BeforeKickOff);



            return true;
        }


        /// <summary>
        /// Creates a new player and adds it to the field
        /// </summary>
        /// <param name="teamname">The teamname of the player</param>
        /// <param name="version">The version of the player</param>
        /// <param name="goalie_flag">Is the player a goalie?</param>
        /// <param name="addr">The remote endpoint of the player</param>
        /// <returns>A reference to the new player</returns>
        public void addPlayer(Player play)
        {
            play.Stadium = this;
            play.Enable = true;
            play.ThisPlayerState = (int)PlayerState.STAND;
            play.SendInitialization();
            play.PlayerTypeId = 0;
            Players.Add(play);
            DrawPlayerList.Add(play);
            Movable_objects.Add(play);


        }


        /// <summary>
        /// Steps all objects a cycle
        /// </summary>
        internal void _step()
        {

            foreach (Player p in Players)
            {
                p.ResetCommandFlags();

            }



            //
            // update objects
            //
            if (Playmode == PlayMode.BeforeKickOff)
            {
                turnMovableObjects();

                foreach (Referee r in Referees) { r.doAnalyse(); }

            }
            else if (Playmode == PlayMode.AfterGoal_Left
                      || Playmode == PlayMode.AfterGoal_Right
                      || Playmode == PlayMode.OffSide_Left
                      || Playmode == PlayMode.OffSide_Right
                      || Playmode == PlayMode.Back_Pass_Left
                      || Playmode == PlayMode.Back_Pass_Right
                      || Playmode == PlayMode.Free_Kick_Fault_Left
                      || Playmode == PlayMode.Free_Kick_Fault_Right
                      || Playmode == PlayMode.CatchFault_Left
                      || Playmode == PlayMode.CatchFault_Right)
            {
               
                incMovableObjects();
          
                foreach (Referee r in Referees) { r.doAnalyse(); }
            }
            else if (Playmode != PlayMode.BeforeKickOff && Playmode != PlayMode.TimeOver)
            {

                foreach (Referee r in Referees) { r.doAnalyse(); }


                incMovableObjects();

                

               // move_caught_ball();

                //for_each( M_referees.begin(), M_referees.end(), std::mem_fun( &Referee::analyse ) );

                ++Time;
            }

            //
            // update stamina
            //
            foreach (Player p in Players)
            {
                if (p.ThisPlayerState == (int)PlayerState.DISABLE) continue;

                p.UpdatePlayerStamina();
                p.UpdateHearCapacities();
            }



            //
            // reset player state
            //
            foreach (Player p in Players)
            {
                p.ResetState();
            }
        }

        /// <summary>
        /// Turns all the movables objects
        /// </summary>
        internal void turnMovableObjects()
        {
            randomShuffleList<MovingObject>(Movable_objects); //rcss::random::UniformRNG::instance() );
            foreach (MovingObject it in Movable_objects)
            {
                it.turnObject();
            }
        }

        /// <summary>
        /// Increments all movable objects
        /// </summary>
        internal void incMovableObjects()
        {
            randomShuffleList<MovingObject>(Movable_objects); //rcss::random::UniformRNG::instance() );
            foreach (MovingObject it in Movable_objects)
            {
                if (it.Enable)
                {
                    it.incrementObjectState();
                }
            }

            collisions();

                if ( BallCatcher != null )
    {
        // keeps the caught ball infront of the player
        Vector rpos = Vector.fromPolar( BallCatcher.Size
                                           + ServerParam.Instance.Ball_size,
                                           BallCatcher.AngleBodyCommitted);
        Ball.moveTo( BallCatcher.Position + rpos, new Vector(0,0), new Vector(0,0) );
    }
        }



        /// <summary>
        /// Sets the main ball's position
        /// </summary>
        /// <param name="kick_off_side">The side that is kicking off</param>
        /// <param name="pos">The set position of the ball</param>
        /// <param name="vel">The set velocity of the ball</param>
        internal void set_ball(Vector pos, Vector vel)
        {
            Ball.moveTo(pos, vel, new Vector(0.0f, 0.0f));

        }

        

        /// <summary>
        /// Sets the main ball's position
        /// </summary>
        /// <param name="kick_off_side">The side that is kicking off</param>
        /// <param name="pos">The set position of the ball</param>
        internal void set_ball(Vector pos)
        {
            Ball.moveTo(pos, new Vector(0.0f,0.0f), new Vector(0.0f, 0.0f));

        }

        /// <summary>
        /// Sets a ball's position
        /// </summary>
        /// <param name="kick_off_side">The side that is kicking off</param>
        /// <param name="pos">The set position of the ball</param>
        /// <param name="ballId">The ball to be set</param>
        internal void set_ball(string kick_off_side, Vector pos, int ballId)
        {
            Balls[ballId].moveTo(pos, new Vector(0.0f, 0.0f), new Vector(0.0f, 0.0f));
       
        }

        /// <summary>
        /// Sets the field to half time
        /// </summary>
        /// <param name="kick_off_side">The new kick off side</param>
        /// <param name="half_time_count">The current half time count</param>
        internal void setHalfTime(int half_time_count)
        {
            //std::cerr << time() << ": setHalfTime  count=" << half_time_count
            //          << std::endl;


            set_ball(new Vector(0.0f, 0.0f));
            change_play_mode(PlayMode.BeforeKickOff);

            if (half_time_count < ServerParam.Instance.Nr_normal_halfs)
            {
                Time = ServerParam.Instance.Half_Time * half_time_count;
            }
            else
            {
                int extra_count = half_time_count - ServerParam.Instance.Nr_normal_halfs;
                Time = ServerParam.Instance.Half_Time * ServerParam.Instance.Nr_normal_halfs
                    + ServerParam.Instance.Extra_Half_time * extra_count;
            }

            // recover only stamina capacity at the start of extra halves
            if (half_time_count == ServerParam.Instance.Nr_normal_halfs)
            {
                foreach (Player p in Players)
                {
                    if (p.ThisPlayerState == (int)PlayerState.DISABLE)
                    {
                        continue;
                    }

                    p.recoverStaminaCapacity();
                }
            }

            Weather.WindVector.X *= -1;
            Weather.WindVector.Y *= -1;
        }

        /// <summary>
        /// Recovers the stamina stats for all players
        /// </summary>
        internal void recoveryPlayers()
        {
            foreach (Player p in Players)
            {
                if (p.ThisPlayerState == (int)PlayerState.DISABLE)
                {
                    continue;
                }

                p.recoverAll();
            }
        }

        /// <summary>
        /// Retrieves information about the ball position (Inside Goal(L/R), Out of Field, or In Field)
        /// </summary>
        /// <returns>The ball position information</returns>
        internal BallPositionInformtion ballPosInfo()
        {


            if (g_l.ContainsPoint(Ball.Position)) return BallPositionInformtion.GoalL;
            if (g_r.ContainsPoint(Ball.Position)) return BallPositionInformtion.GoalR;
            if (!pt.ContainsPoint(Ball.Position)) return BallPositionInformtion.OutOfField;
            return BallPositionInformtion.InField;
        }

        /// <summary>
        /// Places all players in the field
        /// </summary>
        internal void placePlayersInField()
        {


            for (int i = 0; i < Players.Count; ++i)
            {
                if (Players[i].ThisPlayerState == (int)PlayerState.DISABLE) continue;

                if (!fld.ContainsPoint(Players[i].Position))
                {
                    Players[i].MoveTo = (fld.NearestEdgeToPoint(Players[i].Position));
                }
            }
        }

        /// <summary>
        /// Changes the play mode
        /// </summary>
        /// <param name="pm">The new play mode</param>
        internal void change_play_mode(PlayMode pm)
        {


            Playmode = pm;
            foreach (Referee r in Referees)
            {
                r.doPlayModeChange(pm);
            }

            sendRefereeAudio(Enum.GetName(typeof(PlayMode), pm));




        }


        internal void placeBall(PlayMode playMode, string side, Vector pos)
        {
            placeBall(playMode, side, pos, 0);
        }


        /// <summary>
        /// Randomly shuffles a list
        /// </summary>
        /// <typeparam name="T">The type contained in the list to be shuffled</typeparam>
        /// <param name="ls">The list to be shuffled</param>
        internal void randomShuffleList<T>(List<T> ls)
        {
            List<T> temp = new List<T>();

            while (ls.Count != 0)
            {

                int idx = Utilities.CustomRandom.Next(0, ls.Count-1);

                temp.Add(ls[idx]);
                ls.RemoveAt(idx);

            }

            ls.AddRange(temp);


        }

        /// <summary>
        /// Initializes the objects in the stadium
        /// </summary>
        internal void initObjects()
        {
            Ball = new Ball(this);
            Balls.Add(Ball);
            Movable_objects.Add(Ball);
            Ball.moveTo(new Vector(0, 0), new Vector(0, 0), new Vector(0, 0));
            Ball.setConstants(ServerParam.Instance.Ball_size,
                     ServerParam.Instance.Ball_decay,
                     ServerParam.Instance.Ball_rand,
                     ServerParam.Instance.Ball_weight,
                     ServerParam.Instance.Ball_speed_max,
                     ServerParam.Instance.Ball_accel_max);
            

            


        }

        public void addBall()
        {
            Ball ball = new Ball(this);
            Balls.Add(ball);
            Movable_objects.Add(ball);
            ball.moveTo(new Vector(0, 0), new Vector(0, 0), new Vector(0, 0));
            ball.setConstants(ServerParam.Instance.Ball_size,
                     ServerParam.Instance.Ball_decay,
                     ServerParam.Instance.Ball_rand,
                     ServerParam.Instance.Ball_weight,
                     ServerParam.Instance.Ball_speed_max,
                     ServerParam.Instance.Ball_accel_max);
        }



        public void callHalfTime(string kick_off_side, int half_time_count)
        {
            //std::cerr << time() << ": callHalfTime  count=" << half_time_count
            //          << std::endl;

             BallCatcher = null;
            placeBall(PlayMode.FirstHalfOver, kick_off_side, new Vector(0.0f, 0.0f));
            Playmode = (PlayMode.BeforeKickOff);

            if (half_time_count < ServerParam.Instance.Nr_normal_halfs)
            {
                Time = ServerParam.Instance.Half_Time * half_time_count;
            }
            else
            {
                int extra_count = half_time_count - ServerParam.Instance.Nr_normal_halfs;
                Time
                    = ServerParam.Instance.Half_Time * ServerParam.Instance.Nr_normal_halfs
                    + ServerParam.Instance.Extra_Half_time * extra_count;
            }

            // recover only stamina capacity at the start of extra halves
            if (half_time_count == ServerParam.Instance.Nr_normal_halfs)
            {
                foreach (var p in Players)
                {
                    if (!(p).Enable) continue;

                    (p).recoverStaminaCapacity();
                }
            }

            Weather.WindVector.X *= -1;
            Weather.WindVector.Y *= -1;
        }

        /// <summary>
        /// Moves a player to a specific position
        /// </summary>
        /// <param name="side">Side of the player</param>
        /// <param name="unum">Number of the player</param>
        /// <param name="pos">New position</param>
        /// <returns>True iff successfully moved</returns>
        internal bool movePlayer(string team, int unum, Vector pos)
        {
            return movePlayer(team, unum, pos, float.NegativeInfinity, new Vector(float.NegativeInfinity, float.NegativeInfinity));
        }

        /// <summary>
        /// Moves a player to a specific position, angle
        /// </summary>
        /// <param name="side">Side of the player</param>
        /// <param name="unum">Number of the player</param>
        /// <param name="pos">New position</param>
        /// <param name="ang">New angle</param>
        /// <returns>True iff successfully moved</returns>
        internal bool movePlayer(string team, int unum, Vector pos, float angle)
        {
            return movePlayer(team, unum, pos, angle, new Vector(float.NegativeInfinity, float.NegativeInfinity));
        
        }

        /// <summary>
        /// Moves a player to a specific position, velcoity
        /// </summary>
        /// <param name="side">Side of the player</param>
        /// <param name="unum">Number of the player</param>
        /// <param name="pos">New position</param>
        /// <param name="vel">New volocity</param>
        /// <returns>True iff successfully moved</returns>
        internal bool movePlayer(string team, int unum, Vector pos, Vector vel)
        {
            return movePlayer(team, unum, pos, float.NegativeInfinity, vel);
        }

        /// <summary>
        /// Moves a player to a specific position, angle, and velocity
        /// </summary>
        /// <param name="side">Side of the player</param>
        /// <param name="unum">Number of the player</param>
        /// <param name="pos">New position</param>
        /// <param name="ang">New angle</param>
        /// <param name="vel">New velocity</param>
        /// <returns>True iff successfully moved</returns>
        internal bool movePlayer(string team, int unum, Vector pos, float ang, Vector vel)
        {


            Player player = null;

            player = Players.Find(p => p.Team == team && p.Unum == unum);
            
            

            float new_angle = player.AngleBodyCommitted;
            Vector new_vel = player.Velocity;
            Vector new_accel = player.Acceleration;
            if (vel != new Vector(float.NegativeInfinity, float.NegativeInfinity))
            {
                new_vel = vel;
            }
            if (ang != Double.NegativeInfinity)
            {
                new_angle = ang;
            }

            player.Place(pos,
                           new_angle,
                           new_vel,
                           new_accel);
            collisions();
            return true;
        }

        /// <summary>
        /// Receives messages from clients for processing
        /// </summary>
        internal void playerActions()
        {
            randomShuffleList<Player>(Players);

            foreach (var p in Players)
            {
                p.PlayerCommand();
            }
        }


        /// <summary>
        /// Steps the simualtor a cycle
        /// </summary>
        internal void step()
        {


            //
            // step
            //
            _step();
 

 
        }


        /// <summary>
        /// Sends visual messages to everyone
        /// </summary>
        internal void updateState()
        {


            foreach (Player it in Players)
            {
                if (it.ThisPlayerState != (int)PlayerState.DISABLE)
                {

                    it.UpdateState();
                    it.NewCycle();
                }
            }


        }


        /// <summary>
        /// Sends referee audio to all listeners
        /// </summary>
        /// <param name="msg">The message to send</param>
        internal void sendRefereeAudio(string msg)
        {

            // the following should work, but I haven't tested it yet
            //      std::for_each( M_listeners.begin(), M_listeners.end(),
            //                     std::bind2nd( std::mem_fun( &rcss::Listener::sendRefereeAudio ),
            //                                   msg.c_stR ) );

            foreach (Player it in Players)
            {
                it.sendRefereeAudio(msg);
            }

        }


        /// <summary>
        /// Substitutes a player for a new player type
        /// </summary>
        /// <param name="player">The player to be substituted</param>
        /// <param name="player_type_id">The player type id</param>
        internal void substitute(Player player, int player_type_id)
        {
            
            player.Substitute(player_type_id);
             
            broadcastSubstitution(player.Team, player.Unum, player_type_id);
            
        }

        /// <summary>
        /// Handles collision on the field
        /// </summary>
        internal void collisions()
        {
            bool col = false;
            int max_loop = 10;

            do
            {
                col = false;
                Ball.clearCollide();
                foreach (Player it in Players)
                {
                    it.clearCollide();
                }

                // check ball to player
                foreach (Player it in Players)
                {
                    if (it.Enable
                         && !it.Equals(BallCatcher)
                         && Ball.Position.distance((it).Position)
                         < Ball.Size + (it).Size)
                    {
                        col = true;
                        it.CollidedWithBall();
                        foreach (Referee r in Referees) { r.doBallTouched(it); }

                        calcBallCollPos(it);
                    }
                }

                // check player to player
                for (int i = 0; i < Players.Count - 1; ++i)
                {
                    for (int j = i + 1; j < Players.Count; ++j)
                    {
                        if (Players[i].Enable
                             && Players[j].Enable
                             && Players[i].Position.distance(Players[j].Position)
                             < Players[i].Size + Players[j].Size)
                        {
                            col = true;
                            Players[i].CollidedWithPlayer();
                            Players[j].CollidedWithPlayer();
                            calcCollPos(Players[i], Players[j]);
                        }
                    }
                }

                Ball.moveToCollisionPosition();
                foreach (Player it in Players)
                {
                    it.moveToCollisionPosition();
                }

                --max_loop;
            }
            while (col && max_loop > 0);

            //if ( max_loop < 8
            //    std::cerr << M_time << ": collision loop " << max_loop
            //              << std::endl;

            Ball.updateVelocityAfterCollision();
            foreach (Player it in Players)
            {
                it.updateVelocityAfterCollision();
            }

        }

        /// <summary>
        /// Calculates the new position for the abll after a collision
        /// </summary>
        /// <param name="p">The player that collided with the ball</param>
        internal void calcBallCollPos(Player p)
        {
            if (Playmode == PlayMode.PlayOn)
            {
                calcCollPos(Ball, p);
                return;
            }

            Vector b2p;
            if (Ball.Position == p.Position)
            {
                float p_ang = (float)Utilities.CustomRandom.NextDouble(-Math.PI, Math.PI);
                b2p = Vector.fromPolar(Ball.Size + p.Size + Defines.Epsilon, p_ang);
            }
            else
            {
                b2p = p.Position - Ball.Position;
                b2p.normalize(Ball.Size + p.Size + Defines.Epsilon);
            }

            Ball.collide(Ball.Position);
            p.collide(Ball.Position + b2p);
        }

        /// <summary>
        /// Calculates the position after the collsion of two object
        /// </summary>
        /// <param name="a">The first object in the collision</param>
        /// <param name="b">The second object in the collsion</param>
        internal void calcCollPos(MovingObject a, MovingObject b)
        {
            if (a == null || b == null)
                return;

            Vector apos;
            Vector bpos;
            {
                Vector mid = a.Position + b.Position;
                mid /= 2.0f;

                Vector mid2a = a.Position - mid;
                Vector mid2b = b.Position - mid;

                /* pfr 10/25/01
                   This was a really nasty bug. This used to be the condition
                   if ( a->pos == b->pos )
                   If a->pos and b->pos are approximately equal (but
                   not ==, then a->pos - mid and b->pos - mid can both be 0.
                   This means that in the else clause below, we call PVector::r(v)
                   on two zero vectors, which makes them both (v,0).
                   Then, the while statement below is an infinite loop */
                if (a.Position == b.Position
                     || ((a.Position - mid).Radius < Defines.Epsilon
                          && (b.Position - mid).Radius < Defines.Epsilon)
                     )
                {
                    // if the two objects are directly on top on one and other
                    // then they will be separated at a random angle
                    float a_ang = (float)Utilities.CustomRandom.NextDouble(-Math.PI, Math.PI);
                    float b_ang = (float)Utilities.Normalize_angle(a_ang + Math.PI);
                    mid2a = Vector.fromPolar((a.Size + b.Size) / 2.0f + Defines.Epsilon, a_ang);
                    mid2b = Vector.fromPolar((a.Size + b.Size) / 2.0f + Defines.Epsilon, b_ang);
                }
                else
                {
                    mid2a.normalize((a.Size + b.Size) / 2.0f + Defines.Epsilon);
                    mid2b.normalize((a.Size + b.Size) / 2.0f + Defines.Epsilon);
                }

                apos = mid + mid2a;
                bpos = mid + mid2b;

                // 0.01% is added to the movement, as sometimes structural noise
                // means that even though mid2a and mid2b should be
                // a->size + b->size apart, they are ever so slightly
                // less.
                int count = 0;
                while (apos.distance(bpos) < a.Size + b.Size
                        && count < 10)
                {
                    mid2a.normalize((float)(mid2a.Radius * 1.0001));
                    mid2b.normalize((float)(mid2b.Radius * 1.0001));
                    apos = mid + mid2a;
                    bpos = mid + mid2b;
                    ++count;
                }
            }


            /*    cout << "apos = " << a->pos << std::endl; */
            /*    cout << "anewpos = " << (mid + mid2a ) << std::endl; */

            /*    cout << "bpos = " << b->pos << std::endl; */
            /*    cout << "bnewpos = " << (mid + mid2b ) << std::endl; */

            /*    cout << "old distance = " << a->pos.distance ( b->pos ) << std::endl; */
            /*    cout << "new distance = " << apos.distance ( bpos ) << std::endl; */

            a.collide(apos);
            b.collide(bpos);
        }

        /// <summary>
        /// Places a ball
        /// </summary>
        /// <param name="pm">The playmode to set the ball state in</param>
        /// <param name="side">The side to set the ball</param>
        /// <param name="location">The location to set the ball</param>
        internal void placeBall(PlayMode pm, string side, Vector location, int ballId)
        {
            set_ball(side, location, ballId);

            if (Referee.isPenaltyShootOut(Playmode)
                 && (pm == PlayMode.PlayOn || pm == PlayMode.Drop_Ball))
            {
                ; // never change pm to play_on in penalty mode
            }
            else
            {
                change_play_mode(pm);
            }
        }

        /// <summary>
        /// Sets the state of a player
        /// </summary>
        /// <param name="side">The side of the player</param>
        /// <param name="unum">The number of the player</param>
        /// <param name="state">The player state</param>
        internal void setPlayerState(string team, int unum, int state)
        {


            Player player = Players.Find(p => p.Team == team && p.Unum == unum);


            if (player.ThisPlayerState != (int)PlayerState.DISABLE)
            {
                player.AddState(state);
            }
        }

        /// <summary>
        /// Handles when a kick is taken
        /// </summary>
        /// <param name="kicker">The player who kicked the ball</param>
        /// <param name="accel">The acceleration to apply to the abll</param>
        internal void kickTaken(Player kicker, Vector accel)
        {

            BallCatcher = null;
            Ball.accelerateObject(accel);

            foreach (Referee r in Referees) { r.doKickTaken(kicker); }

        }

        /// <summary>
        /// Handles when the ball is caught
        /// </summary>
        /// <param name="catcher">The player who caught the ball</param>
        internal void ballCaught(Player catcher)
        {
            if (!Referee.isPenaltyShootOut(Playmode))
            {
                string msg = "goalie_catch_ball_";
                msg += catcher.Side;
                sendRefereeAudio(msg);
            }


            collisions();
            foreach (Referee r in Referees)
            {
                r.doBallTouched(catcher);
            }

            foreach (Referee r in Referees)
            {
                r.doCaughtBall(catcher);
            }

            if (Playmode == PlayMode.FreeKick_Left
     || Playmode == PlayMode.FreeKick_Right)
            {
                BallCatcher = catcher;
            }

        }

        public void yellowCard(string team, int unum)
        {
            Player p = Players.Find(t => t.Unum == unum && t.Team == team);

            if (p == null
                 || !!p.Enable)
            {
                return;
            }
            sendRefereeAudio("yellow_card_" + p.Side + "_" + p.Unum);

            if (p.YellowCard)
            {

                sendRefereeAudio("red_card_" + p.Side + "_" + p.Unum);
            }

            p.GiveYellowCard();
        }


        public void score(string side)
        {
            if (side == "l") TeamLeftPoint++; else TeamRightPoint++;

        }

        /// <summary>
        /// Handles when a ball catch has failed
        /// </summary>
        internal void ballCatchFailed()
        {
            
            Vector new_ball_vel = Ball.Velocity;
            new_ball_vel *= (float)Utilities.CustomRandom.NextDouble(0.1, 0.8);
            new_ball_vel.rotate((float)Utilities.CustomRandom.NextDouble(-Math.PI/2.0 , Math.PI/2.0));

            Vector accel = new_ball_vel - Ball.Velocity;

            Ball.accelerateObject(accel);
        }



        /// <summary>
        /// Sends player audio to all listeners
        /// </summary>
        /// <param name="player">The player sending audio</param>
        /// <param name="msg">The message to send</param>
        internal void sendPlayerAudio(Player player, string msg)
        {

            foreach (Player it in Players)
            {

                (it).sendPlayerAudio(player, msg);
            }

        }


        /// <summary>
        /// Broadcasts player substitutions to everyone
        /// </summary>
        /// <param name="M_side">The side substituting</param>
        /// <param name="p">The player number being subbed</param>
        /// <param name="type">The new player type</param>
        internal void broadcastSubstitution(string team, int unum, int type)
        {
            string allies = "change_player_type " + unum + " " + type;
            string enemies = "change_player_type " + unum;

            foreach (var p in Players)
            {
                if (p.ThisPlayerState == (int)PlayerState.DISABLE) continue;

                if (p.Team == team)
                {
                    p.sendRefereeAudio(allies);
                }
                else
                {
                    p.sendRefereeAudio(enemies);
                }
            }

        }


        #endregion


        internal void Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            System.Drawing.Graphics g = e.Graphics;
            g.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
           
            float xMod = g.VisibleClipBounds.Width / 100;
            float yMod = g.VisibleClipBounds.Height / 60;
            float playerSize = ServerParam.Instance.Player_size * 2 * 5;
            float ballSize = ServerParam.Instance.Ball_size * 2 * 10;

            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Brushes.White, xMod);
            System.Drawing.Pen pen2 = new System.Drawing.Pen(System.Drawing.Brushes.LightBlue, xMod); //added
            g.DrawLine(pen, xMod*50, 0, xMod*50, yMod*60);
            g.DrawRectangle(pen, new System.Drawing.Rectangle((int)(xMod * 92), (int)(yMod * 23), (int)(xMod * 14), (int)(yMod * 14)));
            g.DrawRectangle(pen, new System.Drawing.Rectangle((int)(xMod * -5), (int)(yMod * 23), (int)(xMod * 14), (int)(yMod * 14)));
            g.DrawRectangle(pen2, new System.Drawing.Rectangle((int)(xMod * 43), (int)(yMod * 23), (int)(xMod * 14), (int)(yMod * 14)));//added
            g.DrawEllipse(pen, new System.Drawing.Rectangle((int)(xMod * 43), (int)(yMod * 23), (int)(xMod * 14), (int)(yMod * 14)));
            RoboCup.Geometry.Vector pos1 = new Vector(0,0);
            RoboCup.Geometry.Vector pos2 = new Vector(0,0);
            System.Drawing.Pen linePen = new System.Drawing.Pen(System.Drawing.Brushes.Black, xMod);
            for (int i = 0; i < DrawPlayerList.Count; i++)
            {
                Player p = DrawPlayerList[i];
                pos1.X = (p.Position.X+50)*xMod;
                pos2.X = (p.Position.X+50)*xMod;
                pos1.Y = (p.Position.Y+30)*yMod;
                pos2.Y = (p.Position.Y+30)*yMod;

                pos1.X += (float)(Math.Cos(p.AngleBodyCommitted)*xMod*playerSize/2);
                pos1.Y += (float)(Math.Sin(p.AngleBodyCommitted)*yMod*playerSize/2);
                pos2.X -= (float)(Math.Cos(p.AngleBodyCommitted)*xMod*playerSize/2);
                pos2.Y -= (float)(Math.Sin(p.AngleBodyCommitted)*yMod*playerSize/2);


                g.FillEllipse(p.Side=="l"?System.Drawing.Brushes.Blue:p.Side=="r"?System.Drawing.Brushes.Red:System.Drawing.Brushes.Yellow, new System.Drawing.Rectangle(new System.Drawing.Point((int)Math.Round((p.Position.X + 50 - playerSize/2)*xMod, (int)MidpointRounding.AwayFromZero), (int)Math.Round((p.Position.Y + 30 - playerSize/2)*yMod, MidpointRounding.AwayFromZero)), new System.Drawing.Size((int)(playerSize*xMod), (int)(playerSize*yMod))));
                g.DrawLine(linePen, pos1.X, pos1.Y, pos2.X, pos2.Y);
            
            }

            foreach (var b in Balls)
            {
                if (b != null)
                {
                    g.FillEllipse(System.Drawing.Brushes.Gray, new System.Drawing.Rectangle(new System.Drawing.Point((int)Math.Round((b.Position.X + 50 - ballSize / 2) * xMod, (int)MidpointRounding.AwayFromZero), (int)Math.Round((b.Position.Y + 30 - ballSize / 2) * yMod, MidpointRounding.AwayFromZero)), new System.Drawing.Size((int)(ballSize * xMod), (int)(ballSize * yMod))));
                }

            }

            foreach (Referee r in Referees)
            {
                r.doPaint(sender, e);
            }
        }

        internal  void tackleTaken(Player player, bool foul)
        {

            foreach (Referee r in this.Referees)
            {
                r.doTackleTaken(player, foul);
            }
        }

        internal void penaltyScore(string side, bool scored)
        {
            if (side == "r")
            {
                PenaltyTakenRight++;
                if (scored) PenaltyScoredRight++;
            }
            else
            {

                PenaltyTakenLeft++;
                if (scored) PenaltyScoredLeft++;
            }
        }

        internal bool episodeCheck()
        {
            bool done = false;

            foreach (var r in Referees)
            {
                done = done || r.doEpisodeEnded();
            }

            return done;
        }

        internal void NewEpisode()
        {
            foreach (var r in Referees)
            {
                r.doNewEpisode();
            }
        }

        internal void kickTaken(Player player, Vector accel, Objects.Ball tBall)
        {
            BallCatcher = null;
            tBall.accelerateObject(accel);

            foreach (Referee r in Referees) { r.doKickTaken(player); }
        }
    }
}
