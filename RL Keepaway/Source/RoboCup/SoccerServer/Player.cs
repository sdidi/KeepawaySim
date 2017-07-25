// -*-C#-*-

/***************************************************************************
                                   Player.cs
                   Class that represents a Player in the RoboCup server.
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
using RoboCup.Geometry;
using RoboCup.Parameters;
using RoboCup.Referees;

namespace RoboCup.Objects
{

    /// <summary>
    /// Class that represents an audio message a player sends/receives
    /// </summary>
    public class AudioMessage
    {
        /// <summary>
        /// The source of the message.
        /// </summary>
        public string source;
        /// <summary>
        /// The time the message was sent.
        /// </summary>
        public int time;
        /// <summary>
        /// The text of the message.
        /// </summary>
        public string msg;

        public AudioMessage()
        {
            source = "";
            time = 0;
            msg = "";
        }
    }

    /// <summary>
    /// Class that represents a player.
    /// </summary>
    public class Player : MovingObject
    {
        #region Utility Methods

        /// <summary>
        /// Normalizes dash power by bounding it
        /// </summary>
        /// <param name="dashPower">The dash power</param>
        /// <returns>Returns Max(Min(dashPower, maxpower), minpower)</returns>
        static float NormalizeDashPower(float dashPower)
        {
            return Utilities.bound(ServerParam.Instance.Min_Dash_Power, dashPower, ServerParam.Instance.Max_Dash_Power);
        }

        /// <summary>
        /// Noramlizes dash angle by bounding it
        /// </summary>
        /// <param name="dashAngle">The dash angle</param>
        /// <returns>Returns Max(Min(dashAngle, maxangle), minangle)</returns>
        static float NormalizeDashAngle(float dashAngle)
        {
            return Utilities.bound(ServerParam.Instance.Min_Dash_Angle, dashAngle, ServerParam.Instance.Max_Dash_Angle);
        }

        /// <summary>
        /// Noramlizes kick power by bounding it
        /// </summary>
        /// <param name="kickPower">The kick power</param>
        /// <returns>Returns Max(Min(kickPower, maxpower), minpower)</returns>
        static float NormalizeKickPower(float kickPower)
        {
            return Utilities.bound(ServerParam.Instance.Minpower, kickPower, ServerParam.Instance.Maxpower);
        }

        /// <summary>
        /// Normalizes tackle power by bounding it
        /// </summary>
        /// <param name="tacklePower">The kick power</param>
        /// <returns>Returns Max(Min(tacklePower, maxpower), minpower)</returns>
        static float NormalizeTacklePower(float tacklePower)
        {
            return Utilities.bound(-ServerParam.Instance.Max_Back_Tackle_Power, tacklePower, ServerParam.Instance.Max_Tackle_Power);
        }

        /// <summary>
        /// Normalizes moment by bounding it and converting it to radians
        /// </summary>
        /// <param name="moment">The moment</param>
        /// <returns>Returns Max(Min(moment, maxmoment), minmoment)</returns>
        static float NormalizeMoment(float moment)
        {
            return (float)(Math.PI / 180.0 * Utilities.bound(ServerParam.Instance.MinMoment, moment, ServerParam.Instance.MaxMoment));
        }

        /// <summary>
        /// Normalizes neck moment by bounding it and converting it to radians
        /// </summary>
        /// <param name="neckMoment">The moment</param>
        /// <returns>Returns Max(Min(neckMoment, maxmoment), minmoment)</returns>
        static float NormalizeNeckMoment(float neckMoment)
        {
            return (float)(Math.PI / 180.0 * Utilities.bound(ServerParam.Instance.MinNeckMoment, neckMoment, ServerParam.Instance.MaxNeckMoment));
        }

        /// <summary>
        /// Normalizes neck angle by bounding it
        /// </summary>
        /// <param name="neckAngle">The moment</param>
        /// <returns>Returns Max(Min(neckAngle, maxangle), minangle)</returns>
        static float NormalizeNeckAngle(float neckAngle)
        {
            return Utilities.bound((float)(Math.PI / 180.0 * ServerParam.Instance.MinNeckAngle), neckAngle, (float)(Math.PI / 180.0 * ServerParam.Instance.MaxNeckAngle));
        }

        #endregion

        #region Instance Variables

        /// <summary>
        /// The play mode of the stadium.
        /// </summary>
        public PlayMode Playmode = PlayMode.BeforeKickOff;
        /// <summary>
        /// Holds information when full state is given to players.
        /// </summary>
        public FullStateData FullState = new FullStateData();
        /// <summary>
        /// Holds the visual information when players must see (not full state).
        /// </summary>
        public VisualData Visuals = new VisualData();
        /// <summary>
        /// Holds the paramters for the server.
        /// </summary>
        public ServerParam serverParameters;
        /// <summary>
        /// Holds the parameters for the players.
        /// </summary>
        public PlayerParam playerParameters;
        /// <summary>
        /// List of the hetero player types.
        /// </summary>
        public List<HeteroPlayer> heteroPlayerTypes = new List<HeteroPlayer>();

        /// <summary>
        /// Indicates if the player has been initialized.
        /// </summary>
        internal bool HasInitBeenSent = false;

        /// <summary>
        /// Queue of referee messsages
        /// </summary>
        public Queue<AudioMessage> RefereeMessages = new Queue<AudioMessage>();

        /// <summary>
        /// Queue of player messages
        /// </summary>
        public Queue<AudioMessage> PlayerMessages = new Queue<AudioMessage>();

        /// <summary>
        /// The player's team
        /// </summary>
        public string Team;

        /// <summary>
        /// The player's side
        /// </summary>
        public string Side;

        /// <summary>
        /// The player's uniform number
        /// </summary>
        public int Unum;

        /// <summary>
        /// The player's stamina
        /// </summary>
        internal float Stamina;

        /// <summary>
        /// The player's recovery
        /// </summary>
        internal float Recovery;

        /// <summary>
        /// The player's effort
        /// </summary>
        internal float Effort;

        /// <summary>
        /// The player's stamina capacity
        /// </summary>
        internal float StaminaCapacity;

        /// <summary>
        /// The player's consumed stamina
        /// </summary>
        internal float ConsumedStamina;

        /// <summary>
        /// The player's visible angle
        /// </summary>
        internal float VisibleAngle;

        /// <summary>
        /// The player's view width
        /// </summary>
        internal string ViewWidth;

        /// <summary>
        /// The player's default angle
        /// </summary>
        internal float DefaultViewingAngle;
        /// <summary>
        /// The player's visible distance
        /// </summary>
        internal float VisibleDistance;
        /// <summary>
        /// The player's visible distance squared
        /// </summary>
        internal float VisibleDistanceSquared;

        /// <summary>
        /// The player's inertia moment
        /// </summary>
        internal float InertiaMoment;

        /// <summary>
        /// Uniform number far length
        /// </summary>
        internal float UnumFarLength;

        /// <summary>
        /// Uniform number too far length
        /// </summary>
        internal float UnumTooFarLength;

        /// <summary>
        /// Team far length
        /// </summary>
        internal float TeamFarLength;

        /// <summary>
        /// Team too far length
        /// </summary>
        internal float TeamTooFarLength;

        /// <summary>
        /// Temporary angle of the body
        /// </summary>
        internal float AngleBody;

        /// <summary>
        /// Committed angle of the body
        /// </summary>
        public float AngleBodyCommitted;

        /// <summary>
        /// Temporary angle of the neck
        /// </summary>
        internal float AngleNeck;

        /// <summary>
        /// Committed angle of the neck
        /// </summary>
        internal float AngleNeckCommitted;

        /// <summary>
        /// Randomness applied to kick
        /// </summary>
        internal float KickRandomness;

        /// <summary>
        /// High quality vision?
        /// </summary>
        internal bool IsVisionHighQuality;

        /// <summary>
        /// Player state
        /// </summary>
        internal Int32 ThisPlayerState;

        /// <summary>
        /// Collided with ball?
        /// </summary>
        internal bool HasCollidedWithBall;

        /// <summary>
        /// Collided with player?
        /// </summary>
        internal bool HasCollidedWithPlayer;

        /// <summary>
        /// Collided with post?
        /// </summary>
        internal bool HasCollidedWithPost;

        /// <summary>
        /// Command done?
        /// </summary>
        internal bool IsPrimaryCommandPerformedThisCycle;


        /// <summary>
        /// Turn Neck Done?
        /// </summary>
        internal bool HasNeckBeenTurned;

        /// <summary>
        /// The number of times attention has shifted
        /// </summary>
        private int AttentionToCount;

        /// <summary>
        /// Capacity to hear from teammates
        /// </summary>
        internal int HearCapacityFromTeammate;

        /// <summary>
        /// Capacity to hear from opponents
        /// </summary>
        internal int HearCapacityFromOpponent;

        /// <summary>
        /// Is goalie?
        /// </summary>
        internal bool IsGoalie;

        /// <summary>
        /// Goalie catch ban
        /// </summary>
        internal int CyclesGoalieCatchBan;

        /// <summary>
        /// Number of goalie moves since catch
        /// </summary>
        internal int GoalieMovesSinceCatch;

        /// <summary>
        /// Number of cycles kicking
        /// </summary>
        internal int KickCycles;

        /// <summary>
        /// Number of kicks
        /// </summary>
        internal int KickCount;

        /// <summary>
        /// Number of dashes
        /// </summary>
        internal int DashCount;

        /// <summary>
        /// Number of turns
        /// </summary>
        internal int TurnCount;

        /// <summary>
        /// Number of catches
        /// </summary>
        internal int CatchCount;

        /// <summary>
        /// Number of moves
        /// </summary>
        internal int MoveCount;

        /// <summary>
        /// Number of neck turns
        /// </summary>
        internal int TurnNeckCount;

        /// <summary>
        /// Number of view changes
        /// </summary>
        internal int ChangeViewCount;

        /// <summary>
        /// Number of says
        /// </summary>
        internal int SayCount;

        /// <summary>
        /// Tackle cycles
        /// </summary>
        internal int TackleCycles;

        /// <summary>
        /// Number of tackles
        /// </summary>
        internal int TackleCount;

        /// <summary>
        /// Is offside?
        /// </summary>
        internal bool IsOffsides;

        /// <summary>
        /// The position a player is found offsides.
        /// </summary>
        Vector offsidesPosition = new Vector(0, 0);

        /// <summary>
        /// The position it is offside at
        /// </summary>
        internal Vector OffsidePos
        {
            get { return offsidesPosition; }
        }

        /// <summary>
        /// The hetero player type of this player
        /// </summary>
        public HeteroPlayer Playertype;

        /// <summary>
        /// The id of the hetero type of the player.
        /// </summary>
        int playerTypeId;

        /// <summary>
        /// ID of the hetero player type
        /// </summary>
        internal int PlayerTypeId
        {
            get { return playerTypeId; }
            set
            {
                HeteroPlayer type = Stadium.Player_types[value];

                Playertype = type;
                MaximumSpeed = type.PlayerSpeedMax;
                InertiaMoment = type.InertiaMoment;
                Decay = type.PlayerDecay;
                Size = type.PlayerSize;
                KickRandomness = type.KickRandomness;


                KickRandomness *= ServerParam.Instance.Kick_rand_factor;

                playerTypeId = value;
            }
        }

        /// <summary>
        /// Is tackling?
        /// </summary>
        internal bool Tackling
        {
            get { return TackleCycles > 0; }
        }

        /// <summary>
        /// Is kicking?
        /// </summary>
        internal bool Kicking
        {
            get { return KickCycles >= 0; }

        }

        /// <summary>
        /// Landmark Quantize step
        /// </summary>
        internal float LandmarkDistanceQuantizeStep
        {
            get { return Parameters.ServerParam.Instance.Quantize_step_l; }
        }

        /// <summary>
        /// Quantize step distance
        /// </summary>
        internal float DistanceQuantizeStep
        {
            get { return Parameters.ServerParam.Instance.Quantize_Step; }

        }

        /// <summary>
        /// Quantize step direction
        /// </summary>
        internal float DirectionQuantizeStep
        {
            get { return 0.1f; }
        }

        /// <summary>
        /// Target of this player's focuse
        /// </summary>
        internal Player FocusTarget;

        /// <summary>
        /// Number of times focused
        /// </summary>
        internal int FocusCount;

        /// <summary>
        /// Holds the sense data for a player (information about a player's body).
        /// </summary>
        public BodyData body = new BodyData();

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for the Player class
        /// </summary>
        /// <param name="stadium">stadium the player is in</param>
        /// <param name="team">Team the player is a member of</param>
        /// <param name="number">The player number of the player</param>
        /// <param name="side">The side of the player.</param>
        public Player(Stadium stadium, string team, int number, string side)
            : base(stadium, "p")
        {


            Team = team;
            Side = side;
            Unum = number;
            Stamina = ServerParam.Instance.Stamina_max;
            Recovery = ServerParam.Instance.Recover_init;
            StaminaCapacity = ServerParam.Instance.Stamina_Capacity;
            ConsumedStamina = 0.0f;
            VisibleAngle = (float)(ServerParam.Instance.Visible_Angle * Math.PI / 180.0);
            ViewWidth = "NORMAL";
            DefaultViewingAngle = (float)(ServerParam.Instance.Visible_Angle * Math.PI / 180.0);
            VisibleDistance = ServerParam.Instance.Visible_Distance;
            VisibleDistanceSquared = VisibleDistance * VisibleDistance;
            UnumFarLength = 20.0f;
            UnumTooFarLength = 40.0f;
            TeamFarLength = 40.0f;
            TeamTooFarLength = 60.0f;
            AngleBody = 0.0f;
            AngleBodyCommitted = 0.0f;
            AngleNeck = 0.0f;
            AngleNeckCommitted = 0.0f;
            AttentionToCount = 0;

            IsVisionHighQuality = true;
            ThisPlayerState = (int)PlayerState.DISABLE;
            HasCollidedWithBall = false;
            HasCollidedWithPlayer = false;
            HasCollidedWithPost = false;
            IsPrimaryCommandPerformedThisCycle = false;
            HasNeckBeenTurned = false;

            HearCapacityFromOpponent = ServerParam.Instance.Hear_Max;
            HearCapacityFromTeammate = ServerParam.Instance.Hear_Max;
            IsGoalie = false;
            CyclesGoalieCatchBan = (0);
            GoalieMovesSinceCatch = (0);
            KickCycles = (0);
            KickCount = (0);
            DashCount = (0);
            TurnCount = (0);
            CatchCount = (0);
            MoveCount = (0);
            TurnNeckCount = (0);
            ChangeViewCount = (0);
            SayCount = (0);


            TackleCycles = (0);
            TackleCount = (0);

            IsOffsides = false;

            Enable = false;

            Weight = ServerParam.Instance.Player_weight;
            MaximumSpeed = ServerParam.Instance.Player_speed_max;
            MaximumAcceleration = ServerParam.Instance.Player_accel_max;

            Position = new Vector(Unum * 3 * ((side == "r") ? 1 : -1), ServerParam.PITCH_WIDTH / 2.0f - 3.0f);

            // pfr 8/14/00: for RC2000 evaluation
            KickRandomness = ServerParam.Instance.Kick_rand;

            PlayerTypeId = 0;


            Effort = Playertype.EffortMax;

        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Adds a state flag to the player
        /// </summary>
        /// <param name="additionalState">State flag to add</param>
        internal void AddState(Int32 additionalState)
        {
            ThisPlayerState |= additionalState;
        }

        /// <summary>
        /// Sends the body message to the player
        /// </summary>
        internal void UpdateBody()
        {
            body.Time = Stadium.Time;
            body.BallCollide = HasCollidedWithBall;
            body.CatchCount = CatchCount;
            body.ChangeViewCount = ChangeViewCount;
            body.DashCount = DashCount;
            body.Effort = Effort;
            body.FocusCount = FocusCount;
            if (FocusTarget != null)
            {
                body.FocusTeam = FocusTarget.Team;
                body.FocusUnum = FocusTarget.Unum;
            }
            body.BodyAngle = AngleBodyCommitted;
            body.HighQuality = IsVisionHighQuality;
            body.KickCount = KickCount;
            body.MoveCount = MoveCount;
            body.NeckAngle = (float)((180.0 / Math.PI) * AngleNeckCommitted);
            body.PlayerCollide = HasCollidedWithPlayer;
            body.PostCollide = HasCollidedWithPost;
            body.SayCount = SayCount;
            body.Stamina = Stamina;
            body.StaminaCapacity = StaminaCapacity;
            body.TackleCount = TackleCount;
            body.TackleCycles = TackleCycles;
            body.TurnCount = TurnCount;
            body.TurnNeckCount = TurnNeckCount;
            body.VelocityHeading = (float)((180.0 / Math.PI) * Utilities.Normalize_angle(Velocity.Theta - AngleBodyCommitted - AngleNeckCommitted));
            body.VelocityMagnitude = (float)Utilities.Quantize(Velocity.Radius, 0.01);
            body.ViewWidth = ViewWidth;

        }

        /// <summary>
        /// Enables a players
        /// </summary>
        internal void SetEnable()
        {
            ThisPlayerState = (int)PlayerState.STAND;
            Enable = true;
            if (IsGoalie)
            {
                ThisPlayerState |= (int)PlayerState.GOALIE;
            }
        }

        /// <summary>
        /// Determines the angle of an object from the body
        /// </summary>
        /// <param name="targetObject"></param>
        /// <returns>Returns the angle from the player's body to the target object.</returns>
        internal float AngleFromBody(BasicObject targetObject)
        {
            return (float)Utilities.Normalize_angle((float)(targetObject.Position - this.Position).Theta - AngleBodyCommitted);
        }

        /// <summary>
        /// Set player as collided with ball
        /// </summary>
        internal void CollidedWithBall()
        {
            AddState((int)PlayerState.BALL_TO_PLAYER | (int)PlayerState.BALL_COLLIDE);
            HasCollidedWithBall = true;
        }

        /// <summary>
        /// Set player as collided with another player
        /// </summary>
        internal void CollidedWithPlayer()
        {
            AddState((int)PlayerState.PLAYER_COLLIDE);
            HasCollidedWithPlayer = true;
        }

        /// <summary>
        /// Initializes an instance of a player
        /// </summary>
        /// <param name="isGoalie">Is the player a goalie?</param>
        /// <returns>True iff successfully initalized</returns>
        public bool Initalize(bool isGoalie)
        {

            IsGoalie = isGoalie;

            Enable = true;
            ThisPlayerState = (int)PlayerState.STAND;

            if (isGoalie) ThisPlayerState |= (int)PlayerState.GOALIE;

            CyclesGoalieCatchBan = 0;
            GoalieMovesSinceCatch = 0;


            float myRandomness = ServerParam.Instance.Player_rand;

            myRandomness *= ServerParam.Instance.Prand_factor;
            KickRandomness *= ServerParam.Instance.Kick_rand_factor;


            Random = myRandomness;

            return true;
        }

        /// <summary>
        /// Substitutes a new player for the current player (Switches the hetero player type).
        /// </summary>
        /// <param name="newPlayerType">The new hetero player type.</param>
        public void Substitute(int newPlayerType)
        {
            PlayerTypeId = newPlayerType;
            Stamina = ServerParam.Instance.Stamina_max;
            Recovery = 1.0f;
            Effort = Playertype.EffortMax;
            StaminaCapacity = ServerParam.Instance.Stamina_Capacity;
            ConsumedStamina = 0.0f;
            HearCapacityFromOpponent = HearCapacityFromTeammate = ServerParam.Instance.Hear_Max;
        }

        /// <summary>
        /// Resets the player state to default by masking the appropriate flags
        /// </summary>
        internal void ResetState()
        {
            if (TackleCycles > 0)
            {
                ThisPlayerState &= (int)(PlayerState.STAND | PlayerState.GOALIE | PlayerState.DISCARD | PlayerState.TACKLE | PlayerState.TACKLE_FAULT);
            }
            else
            {
                ThisPlayerState &= (int)(PlayerState.STAND | PlayerState.GOALIE | PlayerState.DISCARD);
            }
        }

        /// <summary>
        /// Disables and the player
        /// </summary>
        public void Disable()
        {
            Enable = false;
            ThisPlayerState = (int)PlayerState.DISABLE;
            currentPosition.X = -(Unum * 3 * ((Side == "l") ? 1 : -1));
            currentPosition.Y = -ServerParam.PITCH_WIDTH / 2.0f - 3.0f;
            objectVelocity.X = 0.0f;
            objectVelocity.Y = 0.0f;
            objectAcceleration.X = 0.0f;
            objectAcceleration.Y = 0.0f;

        }

        private int lastAction = 0;
        /// <summary>
        /// Main action loop where player logic is implemented.
        /// </summary>
        /// <returns>True iff valid command is parsed</returns>
        public virtual void PlayerCommand()
        {

            if (lastAction == 0)
            {
                turn(90.0f);
                lastAction = 1;
                return;
            }
            else
            {
                lastAction = 0;
                dash(100, 0.0f);
                return;
            }
            turnNeck(45.0f);
            say("Hello");
            attentionTo(true, Team, Unum + 1);

            kick(100.0f, 0.0f);

            tackle(0.0f);

            goalieCatch(0.0f);

            move(0.0f, 0.0f);


            changeView("NORMAL", "HIGH");


            ear(true, Team, "COMPLETE");



        }

        /// <summary>
        /// Does the player have a yellow card?
        /// </summary>
        public bool YellowCard = false;

        /// <summary>
        /// Gives the player a yellow card, or red card if currently yellow carded.
        /// </summary>
        public void GiveYellowCard()
        {
            if (YellowCard)
            {
                GiveRedCard();
            }
            else
            {


                ++TimesCarded;
            }
        }

        /// <summary>
        /// Gives the player a red card.
        /// </summary>
        public void GiveRedCard()
        {
            Enable = false;

            TimesCarded = 2;
        }

        /// <summary>
        /// Dashes straight ahead
        /// </summary>
        /// <param name="dashPower">Power to dash with</param>
        public void dash(float dashPower)
        {
            dash(dashPower, 0.0f);
            //      Console.WriteLine("Dash: " + power + " " + (180.0 / Math.PI * AngleBodyCommitted));

        }

        /// <summary>
        /// Dashes with power in direction dir
        /// </summary>
        /// <param name="dashPower">Power of the dash</param>
        /// <param name="dashDirection">Direction of the dash</param>
        public void dash(float dashPower, float dashDirection)
        {
            if (!IsPrimaryCommandPerformedThisCycle)
            {
                ServerParam serverParamters = ServerParam.Instance;

                dashPower = NormalizeDashPower(dashPower);
                dashDirection = NormalizeDashAngle(dashDirection);

                if (serverParamters.Dash_Angle_Step < Defines.Epsilon)
                {
                    // players can dash in any direction.
                }
                else
                {
                    // The dash direction is discretized by server.dash_angle_step
                    dashDirection = (float)(serverParamters.Dash_Angle_Step * Math.Round(dashDirection / serverParamters.Dash_Angle_Step));
                }

                bool dashBackwards = dashPower < 0.0;

                float powerNeeded = (dashBackwards
                                      ? dashPower * -2.0f
                                      : dashPower);
                powerNeeded = Math.Min(powerNeeded, Stamina + Playertype.ExtraStamina);
                Stamina = Math.Max(0.0f, Stamina - powerNeeded);

                dashPower = (dashBackwards
                          ? powerNeeded / -2.0f
                          : powerNeeded);

                float directionRatio = (float)(Math.Abs(dashDirection) > 90.0
                                    ? serverParamters.Back_Dash_Rate - ((serverParamters.Back_Dash_Rate - serverParamters.Side_Dash_Rate)
                                                               * (1.0 - (Math.Abs(dashDirection) - 90.0) / 90.0))
                                    : serverParamters.Side_Dash_Rate + ((1.0 - serverParamters.Side_Dash_Rate)
                                                               * (1.0 - Math.Abs(dashDirection) / 90.0))
                                    );
                directionRatio = Utilities.bound(0.0f, directionRatio, 1.0f);

                float actualDashPower = Math.Abs(Effort
                                                         * dashPower
                                                         * directionRatio
                                                         * Playertype.DashPowerRate);

                if (dashBackwards)
                {
                    dashDirection += 180.0f;
                }

                accelerateObject(Vector.fromPolar(actualDashPower,
                                          (float)Utilities.Normalize_angle(AngleBodyCommitted + Math.PI / 180.0 * (dashDirection))));

                ++DashCount;
                IsPrimaryCommandPerformedThisCycle = true;
            }
        }

        /// <summary>
        /// Turns the player by a given angular moment
        /// </summary>
        /// <param name="turnAngle">The angle to turn</param>
        public void turn(float turnAngle)
        {
            if (!IsPrimaryCommandPerformedThisCycle)
            {
                AngleBody = (float)Utilities.Normalize_angle(AngleBodyCommitted
                                                + (1.0 + Utilities.drand(-Random, Random))
                                                * NormalizeMoment(turnAngle)
                                                / (1.0 + InertiaMoment * Velocity.Radius));
                ++TurnCount;
                IsPrimaryCommandPerformedThisCycle = true;
            }

            //   Console.WriteLine("Turn: " + moment + " " + (180.0/Math.PI*AngleBodyCommitted));

        }

        /// <summary>
        /// Turns the neck by an angular moment
        /// </summary>
        /// <param name="neckTurnAngle">Angle to turn the neck by</param>
        public void turnNeck(float neckTurnAngle)
        {
            if (!HasNeckBeenTurned)
            {
                AngleNeck = NormalizeNeckAngle(AngleNeckCommitted
                                                   + NormalizeNeckMoment(neckTurnAngle));
                ++TurnNeckCount;
                HasNeckBeenTurned = true;
            }
        }

        /// <summary>
        /// Kicks a particular ball in a given direction with a given power.
        /// </summary>
        /// <param name="kickPower">The power of the kick.</param>
        /// <param name="kickDirection">The direction of the kick.</param>
        /// <param name="ballId">The id of the ball to be kicked.</param>
        public void kick(float kickPower, float kickDirection, int ballId)
        {
            Ball targetBall = Stadium.Balls.Find(b => b.BallId == ballId);

            if (!IsPrimaryCommandPerformedThisCycle && targetBall != null)
            {
                IsPrimaryCommandPerformedThisCycle = true;
                KickCycles = 1;

                kickPower = NormalizeKickPower(kickPower);
                kickDirection = NormalizeMoment(kickDirection);

                Vector temporaryVector;
                float directionDifference;
                float distanceToBall;

                ThisPlayerState |= (int)PlayerState.KICK;

                if (Stadium.Playmode == PlayMode.BeforeKickOff ||
                     Stadium.Playmode == PlayMode.AfterGoal_Right ||
                     Stadium.Playmode == PlayMode.AfterGoal_Left ||
                     Stadium.Playmode == PlayMode.OffSide_Right ||
                     Stadium.Playmode == PlayMode.OffSide_Left ||
                     Stadium.Playmode == PlayMode.Back_Pass_Right ||
                     Stadium.Playmode == PlayMode.Back_Pass_Left ||
                     Stadium.Playmode == PlayMode.Free_Kick_Fault_Right ||
                     Stadium.Playmode == PlayMode.Free_Kick_Fault_Left ||
                     Stadium.Playmode == PlayMode.CatchFault_Right ||
                     Stadium.Playmode == PlayMode.CatchFault_Left ||
                     Stadium.Playmode == PlayMode.TimeOver)
                {
                    ThisPlayerState |= (int)PlayerState.KICK_FAULT;
                    return;
                }

                if (Position.distance(targetBall.Position)
                     > (Playertype.PlayerSize
                         + targetBall.Size + Playertype.KickableMargin))
                {
                    ThisPlayerState |= (int)PlayerState.KICK_FAULT;
                    return;
                }

                directionDifference = Math.Abs(AngleFromBody(targetBall));
                temporaryVector = targetBall.Position - this.Position;
                distanceToBall = (float)(temporaryVector.Radius - Playertype.PlayerSize
                              - ServerParam.Instance.Ball_size);

                float actualKickPower = kickPower * ServerParam.Instance.Kick_power_rate
                    * (float)(1.0 - 0.25 * directionDifference / Math.PI - 0.25 * distanceToBall / Playertype.KickableMargin);

                Vector acceleration = Vector.fromPolar(actualKickPower,
                                                    kickDirection + AngleBodyCommitted);

                float positionRatio
                    = 0.5f
                    + (float)(0.25 * (directionDifference / Math.PI + distanceToBall / Playertype.KickableMargin));

                float speedRatio
                    = 0.5f
                    + (float)(0.5 * (targetBall.Velocity.Radius
                              / (ServerParam.Instance.Ball_speed_max
                                  * ServerParam.Instance.Ball_decay)));

                float maximumRandomness
                    = KickRandomness
                    * (kickPower / ServerParam.Instance.Maxpower)
                    * (positionRatio + speedRatio);
                Vector kickNoise = Vector.fromPolar((float)Utilities.drand(0.0, maximumRandomness),
                                                         (float)Utilities.drand(-Math.PI, Math.PI));
                acceleration += kickNoise;

                Stadium.kickTaken(this, acceleration, targetBall);

                ++KickCount;
            }
        }

        /// <summary>
        /// Kicks the ball with a certain power in a certain direction
        /// </summary>
        /// <param name="kickPower">Power to kick the ball with</param>
        /// <param name="kickDirection">Direction to kick the ball in</param>
        public void kick(float kickPower, float kickDirection)
        {
            kick(kickPower, kickDirection, 1);
        }

        /// <summary>
        /// Attempts the catch of a ball by goalie
        /// </summary>
        /// <param name="catchDirection">Direction of the catch</param>
        public void goalieCatch(float catchDirection)
        {
            if (!IsPrimaryCommandPerformedThisCycle)
            {
                IsPrimaryCommandPerformedThisCycle = true;
                ThisPlayerState |= (int)PlayerState.CATCH;


                if (!this.IsGoalie
                    || CyclesGoalieCatchBan > 0
                     || (Stadium.Playmode != PlayMode.PlayOn
                          && !Referee.isPenaltyShootOut(Stadium.Playmode))
                     )
                {
                    ThisPlayerState |= (int)PlayerState.CATCH_FAULT;
                    return;
                }


                float catchableAreaLength = ServerParam.Instance.Catchable_area_l;
                float reliableCatchableAreaLength = ServerParam.Instance.Reliable_catch_area_l;
                Rectangle catchableArea = new Rectangle(new Vector(catchableAreaLength * 0.5f, 0.0f),
                                       new Vector(catchableAreaLength,
                                                ServerParam.Instance.Catchable_area_w));
                Rectangle reliableCatchableArea = new Rectangle(new Vector(reliableCatchableAreaLength * 0.5f, 0.0f),
                                                new Vector(reliableCatchableAreaLength,
                                                         ServerParam.Instance.Catchable_area_w));
                Vector relativeBallPosition = Stadium.Ball.Position - this.Position;
                relativeBallPosition.rotate(-(AngleBodyCommitted + NormalizeMoment(catchDirection)));

                if (!catchableArea.ContainsPoint(relativeBallPosition)
                     || Utilities.drand(0, 1) >= ServerParam.Instance.Catch_probability)
                {
                    ThisPlayerState |= (int)PlayerState.CATCH_FAULT;
                    return;
                }

                CyclesGoalieCatchBan = ServerParam.Instance.Catch_Ban_Cycle;

                bool success = true;

                if (!reliableCatchableArea.ContainsPoint(relativeBallPosition))
                {
                    float diagonal = (float)Math.Sqrt(Math.Pow(catchableAreaLength, 2.0)
                                                 + Math.Pow(ServerParam.Instance.Catchable_area_w * 0.5, 2.0));
                    float reliableDiagonal = (float)Math.Sqrt(Math.Pow(reliableCatchableAreaLength, 2.0)
                                                          + Math.Pow(ServerParam.Instance.Catchable_area_w * 0.5, 2.0));
                    float ballDistance = (float)relativeBallPosition.Radius;

                    float alpha = 0.75f;

                    float maximumFailureProbability = 1.0f - ServerParam.Instance.Min_catch_probablity;
                    float speedRatio = maximumFailureProbability * alpha
                        * (float)(Stadium.Ball.Velocity.Radius
                           / (ServerParam.Instance.Ball_speed_max
                               * ServerParam.Instance.Ball_decay));
                    float distanceRatio = maximumFailureProbability * (1.0f - alpha)
                        * (float)((ballDistance - reliableDiagonal) / (diagonal - reliableDiagonal));

                    float failureProbability = Utilities.bound(0.0f,
                                                    speedRatio + distanceRatio,
                                                    1.0f);


                    if (BournoulliDistribution(failureProbability))
                    {
                        success = false;
                    }
                }

                if (success)
                {
                    Vector newPosition = Stadium.Ball.Position - this.Position;
                    float magnitude = (float)newPosition.Radius;
                    magnitude -= ServerParam.Instance.Ball_size + Playertype.PlayerSize;
                    newPosition.normalize(magnitude);
                    currentPosition += newPosition;
                    AngleBody = (float)newPosition.Theta;
                    objectVelocity = new Vector(0, 0);
                    GoalieMovesSinceCatch = 0;
                    Stadium.ballCaught(this);
                }
                ++CatchCount;
            }
        }

        /// <summary>
        /// Generates a true with Bournoulli distribution
        /// </summary>
        /// <param name="probability">Probability of a true</param>
        /// <returns>True/False according to Bournoulli distribution</returns>
        private bool BournoulliDistribution(float probability)
        {
            return Utilities.CustomRandom.NextDouble() < probability;
        }

        /// <summary>
        /// Says a message from the player
        /// </summary>
        /// <param name="message">The message to be said</param>
        public void say(string message)
        {
            if (message.Length > ServerParam.Instance.Say_Msg_Size)
            {
                return;
            }

            Stadium.sendPlayerAudio(this, message);
            ++SayCount;
        }

        /// <summary>
        /// Moves the player to a specific x, y coordinate
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void move(float x, float y)
        {
            if (!IsPrimaryCommandPerformedThisCycle)
            {
                if (Stadium.Playmode == PlayMode.BeforeKickOff ||
                     Stadium.Playmode == PlayMode.AfterGoal_Left ||
                     Stadium.Playmode == PlayMode.AfterGoal_Right
                    //|| Stadium.Playmode == PlayMode.PM_PenaltySetup_Left
                    //|| Stadium.Playmode == PlayMode.PM_PenaltySetup_Right
                     )
                {
                    currentPosition.X = x;
                    currentPosition.Y = y;
                    Stadium.collisions();
                }
                else if ((Stadium.Playmode == PlayMode.FreeKick_Left
                            || Stadium.Playmode == PlayMode.FreeKick_Right)
                          && false)//Stadium.Ball_catcher.Equals(this))
                {
                    if (ServerParam.Instance.Goalie_max_moves < 0
                         || GoalieMovesSinceCatch < ServerParam.Instance.Goalie_max_moves)
                    {
                        currentPosition.X = x;
                        currentPosition.Y = y;
                        ++GoalieMovesSinceCatch;
                    }

                }
                else
                {
                    return;
                }

                IsPrimaryCommandPerformedThisCycle = true;
                ++MoveCount;
            }
        }

        /// <summary>
        /// Changes the view of the player
        /// </summary>
        /// <param name="newViewWidth">The new width of the view</param>
        /// <param name="newViewQuality">The new quality of the view</param>
        public void changeView(string newViewWidth, string newViewQuality)
        {
            if (newViewQuality != "HIGH")
            {
                return;
            }

            if (newViewWidth == "NARROW")
            {

                VisibleAngle = DefaultViewingAngle / 2.0f;

            }
            else if (newViewWidth == "NORMAL")
            {

                VisibleAngle = DefaultViewingAngle;

            }
            else if (newViewWidth == "WIDE")
            {

                VisibleAngle = DefaultViewingAngle * 2.0f;

            }
            else
            {
                return;
            }

            ViewWidth = newViewWidth;

            if (newViewQuality == "HIGH")
            {
                IsVisionHighQuality = true;
            }
            else if (newViewQuality == "LOW")
            {

                IsVisionHighQuality = false;

            }
            else
            {
                return;
            }

            ++ChangeViewCount;
        }

        /// <summary>
        /// Changes the view width
        /// </summary>
        /// <param name="newViewWidth">The new view width</param>
        public void changeView(string newViewWidth)
        {
            if (newViewWidth == "NARROW")
            {

                VisibleAngle = DefaultViewingAngle / 2.0f;

            }
            else if (newViewWidth == "NORMAL")
            {

                VisibleAngle = DefaultViewingAngle;

            }
            else if (newViewWidth == "WIDE")
            {

                VisibleAngle = DefaultViewingAngle * 2.0f;

            }
            else
            {
                return;
            }

            ViewWidth = newViewWidth;
            IsVisionHighQuality = true;

            ++ChangeViewCount;
        }

        /// <summary>
        /// Player Command attentionto
        /// </summary>
        /// <param name="isAttentionOn">Turn attention on/off</param>
        /// <param name="attentionToTeamname">Team attenion name</param>
        /// <param name="attentionToUniformNumber">the player number to pay attention to</param>
        public void attentionTo(bool isAttentionOn, string attentionToTeamname, int attentionToUniformNumber)
        {
            if (isAttentionOn == false)
            {
                // turn attention to off
                focusOff();
                ++AttentionToCount;
            }
            else
            {

                string at_team = attentionToTeamname;

                foreach (Player player in Stadium.Players)
                {
                    if ((player.Team == attentionToTeamname && player.Unum == attentionToUniformNumber) || (attentionToTeamname == "unknown" && player.Id == attentionToUniformNumber))
                    {
                        focusOn(player);
                        AttentionToCount++;
                        break;
                    }
                }


            }
        }

        /// <summary>
        /// Tackles with a power/angle
        /// </summary>
        /// <param name="tacklePower">The power with which to tackle</param>
        public void tackle(float tacklePower)
        {
            bool foul = false;
            if (!IsPrimaryCommandPerformedThisCycle
                 && !Tackling)
            {
                IsPrimaryCommandPerformedThisCycle = true;
                TackleCycles = ServerParam.Instance.Tackle_Cycles;
                ++TackleCount;

                Vector ballRelativePosition = Stadium.Ball.Position - Position;
                ballRelativePosition.rotate(-AngleBodyCommitted);

                float tackleDistance = (ballRelativePosition.X > 0.0
                                       ? ServerParam.Instance.Tackle_Dist
                                       : ServerParam.Instance.Tackle_Back_Dist);

                if (Math.Abs(tackleDistance) <= 1.0e-5)
                {
                    ThisPlayerState |= (int)PlayerState.TACKLE_FAULT;
                    return;
                }

                float failureProbability = (float)(Math.Pow(Math.Abs(ballRelativePosition.X) / tackleDistance,
                                          ServerParam.Instance.Tackle_Exponent)
                                + Math.Pow(Math.Abs(ballRelativePosition.Y)
                                            / ServerParam.Instance.Tackle_Width,
                                            ServerParam.Instance.Tackle_Exponent));

                if (failureProbability < 1.0)
                {

                    if (BournoulliDistribution(failureProbability))
                    {
                        ThisPlayerState |= (int)PlayerState.TACKLE;

                        if (Stadium.Playmode == PlayMode.BeforeKickOff ||
                             Stadium.Playmode == PlayMode.AfterGoal_Right ||
                             Stadium.Playmode == PlayMode.AfterGoal_Left ||
                             Stadium.Playmode == PlayMode.OffSide_Right ||
                             Stadium.Playmode == PlayMode.OffSide_Left ||
                             Stadium.Playmode == PlayMode.Back_Pass_Right ||
                             Stadium.Playmode == PlayMode.Back_Pass_Left ||
                             Stadium.Playmode == PlayMode.Free_Kick_Fault_Right ||
                             Stadium.Playmode == PlayMode.Free_Kick_Fault_Left ||
                             Stadium.Playmode == PlayMode.TimeOver)
                        {
                            return;
                        }

                        float powerRatio = 1.0f;
                        Vector acceleration = new Vector(0.0f, 0.0f);

                        float angle = NormalizeMoment(tacklePower);
                        float effectiveTacklePower
                            = (float)(ServerParam.Instance.Max_Back_Tackle_Power
                                + ((ServerParam.Instance.Max_Tackle_Power
                                      - ServerParam.Instance.Max_Back_Tackle_Power)
                                    * (1.0 - (Math.Abs(angle) / Math.PI))
                                    )
                                )
                            * ServerParam.Instance.Tackle_Power_Rate;

                        effectiveTacklePower *= 1.0f - 0.5f * (float)(Math.Abs(ballRelativePosition.Theta) / Math.PI);

                        acceleration = Vector.fromPolar(effectiveTacklePower,
                                                    angle + AngleBodyCommitted);

                        float positionRatio = 0.5f + 0.5f * (1.0f - failureProbability);

                        float speedRatio
                            = 0.5f
                            + 0.5f * (float)(Stadium.Ball.Velocity.Radius
                                      / (ServerParam.Instance.Ball_speed_max
                                          * ServerParam.Instance.Ball_decay));

                        float maximumRandomness = KickRandomness
                            * powerRatio
                            * (positionRatio + speedRatio);
                        Vector kickNoise = Vector.fromPolar((float)Utilities.drand(0.0, maximumRandomness),
                                                                 (float)Utilities.drand(-Math.PI, Math.PI));
                        acceleration += kickNoise;

                        Stadium.kickTaken(this, acceleration);
                        Stadium.tackleTaken(this, foul);
                    }
                    else
                    {
                        ThisPlayerState |= (int)(PlayerState.TACKLE | PlayerState.TACKLE_FAULT);
                    }
                }
                else
                {
                    ThisPlayerState |= (int)PlayerState.TACKLE_FAULT;
                }
            }
        }

        /// <summary>
        /// Player Command ear
        /// </summary>
        /// <param name="isEarOn">Turn on or off</param>
        /// <param name="targetTeamName">Team name</param>
        /// <param name="newMode">The new ear mode</param>
        public void ear(bool isEarOn, string targetTeamName, string newMode)
        {
            bool partial = true;
            bool complete = true;
            if (newMode == "PARTIAL")
            {
                complete = false;
            }
            else if (newMode == "COMPLETE")
            {
                partial = false;
            }


            setEar(isEarOn, targetTeamName, complete, partial);
        }

        /// <summary>
        /// Gets the play mode from the stadium.
        /// </summary>
        /// <returns>The current playmode.</returns>
        protected PlayMode GetPlayMode()
        {
            return Stadium.Playmode;
        }

        /// <summary>
        /// Updates the state of the player (see and sense).
        /// </summary>
        internal void UpdateState()
        {

            UpdateBody();

            if (ServerParam.Instance.Fullstate)
            {
                updateFullstate();
            }
            else
            {
                updateVisual();
            }
        }

        /// <summary>
        /// Sends initialization message to player
        /// </summary>
        internal void SendInitialization()
        {

            serverParameters = ServerParam.Instance;
            playerParameters = PlayerParam.Instance;

            foreach (var hp in Stadium.Player_types)
            {
                heteroPlayerTypes.Add(hp);
            }

            HasInitBeenSent = true;

        }

        /// <summary>
        /// Sends visual message to player
        /// </summary>
        internal void updateVisual()
        {
            Visuals.Time = Stadium.Time;
            SetFlags();
            SetLines();
            SetBalls();
            SetPlayers();

        }

        /// <summary>
        /// Sets the visual information for Lines.
        /// </summary>
        private void SetLines()
        {
            double normalToLine;
            double playerToLineDistance;
            double startOfLine;
            double endOfLine;
            bool isLineVertical;
            BasicObject line;
            VisualObject temporaryVisual = new VisualObject();
            temporaryVisual.DirChange = null;
            temporaryVisual.Direction = null;
            temporaryVisual.Distance = null;
            temporaryVisual.DistChange = null;
            temporaryVisual.Id = null;
            temporaryVisual.Name = "";

            line = Stadium.Field.lineLeft;

            normalToLine = Math.PI;
            if (this.currentPosition.X < line.Position.X)
            {
                normalToLine = 0.0;
            }

            playerToLineDistance = line.Position.X - this.currentPosition.X;
            startOfLine = -Parameters.ServerParam.PITCH_WIDTH * 0.5;
            endOfLine = Parameters.ServerParam.PITCH_WIDTH * 0.5;
            isLineVertical = true;

            double lineAngle = CalculateLineRadianDirection(normalToLine);

            if (Math.Abs(lineAngle) <= this.VisibleAngle * 0.5)
            {

                double lineIntersection = playerToLineDistance * Math.Tan(lineAngle);

                if (isLineVertical)
                {
                    lineIntersection = this.currentPosition.Y - lineIntersection;
                }
                else
                {
                    lineIntersection = this.currentPosition.X + lineIntersection;
                }

                if (lineIntersection > startOfLine || lineIntersection < endOfLine)
                {

                    temporaryVisual.Name = line.Name;
                    temporaryVisual.Distance = (float?)Utilities.Quantize(Math.Exp(Utilities.Quantize(Math.Log(Math.Abs(playerToLineDistance / Math.Cos(lineAngle)) + Defines.Epsilon), this.LandmarkDistanceQuantizeStep)), 0.1);
                    temporaryVisual.Direction = (int)Math.Round(180.0 / Math.PI * lineAngle, MidpointRounding.AwayFromZero);

                    Visuals.lines.Add(temporaryVisual);
                }
            }
            temporaryVisual = new VisualObject();
            temporaryVisual.DirChange = null;
            temporaryVisual.Direction = null;
            temporaryVisual.Distance = null;
            temporaryVisual.DistChange = null;
            temporaryVisual.Id = null;
            temporaryVisual.Name = "";

            line = Stadium.Field.lineRight;

            normalToLine = 0;
            if (this.currentPosition.X > line.Position.X)
            {
                normalToLine = Math.PI;
            }

            playerToLineDistance = line.Position.X - this.currentPosition.X;
            startOfLine = -Parameters.ServerParam.PITCH_WIDTH * 0.5;
            endOfLine = Parameters.ServerParam.PITCH_WIDTH * 0.5;
            isLineVertical = true;

            lineAngle = CalculateLineRadianDirection(normalToLine);

            if (Math.Abs(lineAngle) <= this.VisibleAngle * 0.5)
            {

                double line_intersect = playerToLineDistance * Math.Tan(lineAngle);

                if (isLineVertical)
                {
                    line_intersect = this.currentPosition.Y - line_intersect;
                }
                else
                {
                    line_intersect = this.currentPosition.X + line_intersect;
                }

                if (line_intersect > startOfLine || line_intersect < endOfLine)
                {

                    temporaryVisual.Name = line.Name;
                    temporaryVisual.Distance = (float?)Utilities.Quantize(Math.Exp(Utilities.Quantize(Math.Log(Math.Abs(playerToLineDistance / Math.Cos(lineAngle)) + Defines.Epsilon), this.LandmarkDistanceQuantizeStep)), 0.1);
                    temporaryVisual.Direction = (int)Math.Round(180.0 / Math.PI * lineAngle, MidpointRounding.AwayFromZero);
                    Visuals.lines.Add(temporaryVisual);
                }
            }

            temporaryVisual = new VisualObject();
            temporaryVisual.DirChange = null;
            temporaryVisual.Direction = null;
            temporaryVisual.Distance = null;
            temporaryVisual.DistChange = null;
            temporaryVisual.Id = null;
            temporaryVisual.Name = "";
            line = Stadium.Field.lineTop;

            normalToLine = Math.PI * -0.5;
            if (this.currentPosition.Y < line.Position.X)
            {
                normalToLine *= -1;
            }

            playerToLineDistance = line.Position.X - this.currentPosition.Y;
            startOfLine = -Parameters.ServerParam.PITCH_LENGTH * 0.5;
            endOfLine = Parameters.ServerParam.PITCH_LENGTH * 0.5;
            isLineVertical = false;

            lineAngle = CalculateLineRadianDirection(normalToLine);

            if (Math.Abs(lineAngle) <= this.VisibleAngle * 0.5)
            {

                double line_intersect = playerToLineDistance * Math.Tan(lineAngle);

                if (isLineVertical)
                {
                    line_intersect = this.currentPosition.Y - line_intersect;
                }
                else
                {
                    line_intersect = this.currentPosition.X + line_intersect;
                }

                if (line_intersect > startOfLine || line_intersect < endOfLine)
                {

                    temporaryVisual.Name = line.Name;
                    temporaryVisual.Distance = (float?)Utilities.Quantize(Math.Exp(Utilities.Quantize(Math.Log(Math.Abs(playerToLineDistance / Math.Sin(lineAngle)) + Defines.Epsilon), this.LandmarkDistanceQuantizeStep)), 0.1);
                    temporaryVisual.Direction = (int)Math.Round(180.0 / Math.PI * lineAngle, MidpointRounding.AwayFromZero);
                    Visuals.lines.Add(temporaryVisual);
                }
            }
            temporaryVisual = new VisualObject();
            temporaryVisual.DirChange = null;
            temporaryVisual.Direction = null;
            temporaryVisual.Distance = null;
            temporaryVisual.DistChange = null;
            temporaryVisual.Id = null;
            temporaryVisual.Name = "";
            line = Stadium.Field.lineBottom;

            normalToLine = Math.PI * 0.5;
            if (this.currentPosition.Y > line.Position.X)
            {
                normalToLine *= -1;
            }

            playerToLineDistance = line.Position.X - this.currentPosition.Y;
            startOfLine = -Parameters.ServerParam.PITCH_LENGTH * 0.5;
            endOfLine = Parameters.ServerParam.PITCH_LENGTH * 0.5;
            isLineVertical = false;

            lineAngle = CalculateLineRadianDirection(normalToLine);

            if (Math.Abs(lineAngle) <= this.VisibleAngle * 0.5)
            {

                double line_intersect = playerToLineDistance * Math.Tan(lineAngle);

                if (isLineVertical)
                {
                    line_intersect = this.currentPosition.Y - line_intersect;
                }
                else
                {
                    line_intersect = this.currentPosition.X + line_intersect;
                }

                if (line_intersect > startOfLine || line_intersect < endOfLine)
                {

                    temporaryVisual.Name = line.Name;
                    temporaryVisual.Distance = (float?)Utilities.Quantize(Math.Exp(Utilities.Quantize(Math.Log(Math.Abs(playerToLineDistance / Math.Sin(lineAngle)) + Defines.Epsilon), this.LandmarkDistanceQuantizeStep)), 0.1);
                    temporaryVisual.Direction = (int)Math.Round(180.0 / Math.PI * lineAngle, MidpointRounding.AwayFromZero);
                    Visuals.lines.Add(temporaryVisual);
                }
            }
        }

        /// <summary>
        /// Calculates the direction to a line in radians
        /// </summary>
        /// <param name="normalToLine">The normal of the line</param>
        /// <returns>The normalized angle to the line in radians</returns>
        public double CalculateLineRadianDirection(double normalToLine)
        {
            return Utilities.Normalize_angle(normalToLine
                                    - this.AngleBodyCommitted
                                    - this.AngleNeckCommitted);
        }

        /// <summary>
        /// Sets the visual information for the flags.
        /// </summary>
        private void SetFlags()
        {
            double angle;
            double unquantizedDistance;
            double quantizedDistance;
            double probability;
            double distanceChange, directionChange;
            VisualObject temporaryFlag = new VisualObject();
            Visuals.flags.Clear();


            foreach (var flag in Stadium.Field.landmarks)
            {
                temporaryFlag = new VisualObject();
                temporaryFlag.DirChange = null;
                temporaryFlag.Direction = null;
                temporaryFlag.Distance = null;
                temporaryFlag.DistChange = null;
                temporaryFlag.Id = null;
                temporaryFlag.Name = "";


                angle = Utilities.Normalize_angle(this.AngleFromBody(flag) - this.AngleNeckCommitted);
                unquantizedDistance = this.currentPosition.distanceSquared(flag.Position);


                if (Math.Abs(angle) < this.VisibleAngle * 0.5)
                {
                    unquantizedDistance = Math.Sqrt(unquantizedDistance);
                    quantizedDistance = Utilities.Quantize(Math.Exp(Utilities.Quantize(Math.Log(unquantizedDistance + Defines.Epsilon), LandmarkDistanceQuantizeStep)), 0.1);
                    probability = (quantizedDistance - this.UnumFarLength) / (this.UnumTooFarLength - this.UnumFarLength);


                    if (probability >= 1.0 || Utilities.CustomRandom.NextDouble() < probability)
                    {

                        temporaryFlag.Name = flag.Name;
                        temporaryFlag.Distance = (float?)quantizedDistance;
                        temporaryFlag.Direction = (int)Math.Round(180.0 / Math.PI * angle, MidpointRounding.AwayFromZero);


                        Visuals.flags.Add(temporaryFlag);


                    }
                    else
                    {
                        if (unquantizedDistance != 0.0)
                        {
                            Vector vtmp = -this.objectVelocity;
                            Vector etmp = flag.Position - this.Position;
                            etmp = etmp / (float)unquantizedDistance;

                            distanceChange = vtmp.X * etmp.X + vtmp.Y * etmp.Y;
                            //         dir_chg = RAD2DEG * ( vtmp.y * etmp.x
                            //                               - vtmp.x * etmp.y ) / un_quant_dist;
                            directionChange = vtmp.Y * etmp.X - vtmp.X * etmp.Y;
                            directionChange /= unquantizedDistance;
                            directionChange *= 180.0 / Math.PI;

                            directionChange = (directionChange == 0.0
                                        ? 0.0
                                        : Utilities.Quantize(directionChange, this.DirectionQuantizeStep));
                            distanceChange = (quantizedDistance
                                         * Utilities.Quantize(distanceChange
                                                     / unquantizedDistance, 0.02));
                        }
                        else
                        {
                            directionChange = 0.0;
                            distanceChange = 0.0;
                        }
                        temporaryFlag.Name = flag.Name;
                        temporaryFlag.Distance = (float?)quantizedDistance;
                        temporaryFlag.Direction = (int)Math.Round(180.0 / Math.PI * angle, MidpointRounding.AwayFromZero);
                        temporaryFlag.DirChange = (float?)directionChange;
                        temporaryFlag.DistChange = (float?)distanceChange;
                        Visuals.flags.Add(temporaryFlag);


                    }

                }
                else if (unquantizedDistance < this.VisibleDistanceSquared)
                {

                    temporaryFlag.Name = flag.Name.ToUpper();

                    Visuals.flags.Add(temporaryFlag);

                }
            }
        }

        /// <summary>
        /// Sets the visual information for balls.
        /// </summary>
        private void SetBalls()
        {
            double angle;
            double unquantizedDistance;
            double quantizedDistance;
            double probability;
            double distanceChange, directionChange;
            VisualObject temporaryBall = new VisualObject();
            Visuals.balls.Clear();

            foreach (var ball in Stadium.Balls)
            {
                temporaryBall = new VisualObject();
                temporaryBall.DirChange = null;
                temporaryBall.Direction = null;
                temporaryBall.Distance = null;
                temporaryBall.DistChange = null;
                temporaryBall.Id = null;
                temporaryBall.Name = "";

                angle = Utilities.Normalize_angle(this.AngleFromBody(ball) - this.AngleNeckCommitted);
                unquantizedDistance = this.currentPosition.distanceSquared(ball.Position);


                if (Math.Abs(angle) < this.VisibleAngle * 0.5)
                {
                    unquantizedDistance = Math.Sqrt(unquantizedDistance);
                    quantizedDistance = Utilities.Quantize(Math.Exp(Utilities.Quantize(Math.Log(unquantizedDistance + Defines.Epsilon), DistanceQuantizeStep)), 0.1);
                    probability = (quantizedDistance - this.UnumFarLength) / (this.UnumTooFarLength - this.UnumFarLength);


                    if (probability >= 1.0 || Utilities.CustomRandom.NextDouble() < probability)
                    {

                        temporaryBall.Name = "b";
                        temporaryBall.Distance = (float?)quantizedDistance;
                        temporaryBall.Id = ball.BallId;
                        temporaryBall.Direction = (int)Math.Round(180.0 / Math.PI * angle, MidpointRounding.AwayFromZero);


                        Visuals.balls.Add(temporaryBall);


                    }
                    else
                    {
                        if (unquantizedDistance != 0.0)
                        {
                            Vector vtmp = -this.objectVelocity;
                            Vector etmp = ball.Position - this.Position;
                            etmp = etmp / (float)unquantizedDistance;

                            distanceChange = vtmp.X * etmp.X + vtmp.Y * etmp.Y;
                            //         dir_chg = RAD2DEG * ( vtmp.y * etmp.x
                            //                               - vtmp.x * etmp.y ) / un_quant_dist;
                            directionChange = vtmp.Y * etmp.X - vtmp.X * etmp.Y;
                            directionChange /= unquantizedDistance;
                            directionChange *= 180.0 / Math.PI;

                            directionChange = (directionChange == 0.0
                                        ? 0.0
                                        : Utilities.Quantize(directionChange, this.DirectionQuantizeStep));
                            distanceChange = (quantizedDistance
                                         * Utilities.Quantize(distanceChange
                                                     / unquantizedDistance, 0.02));
                        }
                        else
                        {
                            directionChange = 0.0;
                            distanceChange = 0.0;
                        }
                        temporaryBall.Name = "b";
                        temporaryBall.Id = ball.BallId;
                        temporaryBall.Distance = (float?)quantizedDistance;
                        temporaryBall.Direction = (int)Math.Round(180.0 / Math.PI * angle, MidpointRounding.AwayFromZero);
                        temporaryBall.DirChange = (float?)directionChange;
                        temporaryBall.DistChange = (float?)distanceChange;
                        Visuals.balls.Add(temporaryBall);


                    }

                }
                else if (unquantizedDistance < this.VisibleDistanceSquared)
                {

                    temporaryBall.Name = "B";
                    temporaryBall.Id = ball.BallId;
                    Visuals.balls.Add(temporaryBall);

                }
            }
        }

        /// <summary>
        /// Sets the visual information for players.
        /// </summary>
        private void SetPlayers()
        {
            double angle;
            double unquantizedDistance;
            double quantizedDistance;
            double probability;
            double distanceChange, directionChange;
            VisualPlayer temp = new VisualPlayer();
            Visuals.players.Clear();

            foreach (var player in Stadium.Players)
            {
                temp = new VisualPlayer();
                temp.BodyDir = null;
                temp.Goalie = null;
                temp.HeadDir = null;
                temp.Kicking = null;
                temp.Tackling = null;
                temp.Teamname = "";
                temp.DirChange = null;
                temp.Direction = null;
                temp.Distance = null;
                temp.DistChange = null;
                temp.Id = null;
                temp.Name = "";

                angle = Utilities.Normalize_angle(this.AngleFromBody(player) - this.AngleNeckCommitted);
                unquantizedDistance = this.currentPosition.distanceSquared(player.Position);


                if (Math.Abs(angle) < this.VisibleAngle * 0.5)
                {
                    unquantizedDistance = Math.Sqrt(unquantizedDistance);
                    quantizedDistance = Utilities.Quantize(Math.Exp(Utilities.Quantize(Math.Log(unquantizedDistance + Defines.Epsilon), DistanceQuantizeStep)), 0.1);
                    probability = (quantizedDistance - this.TeamFarLength) / (this.TeamTooFarLength - this.TeamFarLength);


                    if (probability >= 1.0 || Utilities.CustomRandom.NextDouble() < probability)
                    {

                        temp.Name = "p";
                        temp.Distance = (float?)quantizedDistance;
                        temp.Direction = (int)Math.Round(180.0 / Math.PI * angle, MidpointRounding.AwayFromZero);
                        Visuals.players.Add(temp);

                    }
                    else
                    {
                        probability = (quantizedDistance - this.UnumFarLength) / (this.UnumTooFarLength - this.UnumFarLength);

                        if (probability >= 1.0 || Utilities.CustomRandom.NextDouble() < probability)
                        {
                            temp.Name = "p";
                            temp.Teamname = player.Team;
                            if (Parameters.ServerParam.Instance.Hide_teams && Team != player.Team)
                            {
                                temp.Teamname = "unknown";
                            }
                            temp.Distance = (float?)quantizedDistance;
                            temp.Direction = (int)Math.Round(180.0 / Math.PI * angle, MidpointRounding.AwayFromZero);
                            Visuals.players.Add(temp);
                        }
                        else
                        {
                            if (unquantizedDistance != 0.0)
                            {
                                Vector vtmp = -this.currentPosition;
                                Vector etmp = player.Position - this.Position;
                                etmp = etmp / (float)unquantizedDistance;

                                distanceChange = vtmp.X * etmp.X + vtmp.Y * etmp.Y;
                                //         dir_chg = RAD2DEG * ( vtmp.y * etmp.x
                                //                               - vtmp.x * etmp.y ) / un_quant_dist;
                                directionChange = vtmp.Y * etmp.X - vtmp.X * etmp.Y;
                                directionChange /= unquantizedDistance;
                                directionChange *= 180.0 / Math.PI;

                                directionChange = (directionChange == 0.0
                                            ? 0.0
                                            : Utilities.Quantize(directionChange, this.DirectionQuantizeStep));
                                distanceChange = (quantizedDistance
                                             * Utilities.Quantize(distanceChange
                                                         / unquantizedDistance, 0.02));
                            }
                            else
                            {
                                directionChange = 0.0;
                                distanceChange = 0.0;
                            }
                            temp.Id = player.Unum;
                            temp.Name = "p";
                            temp.Teamname = player.Team;
                            if (Parameters.ServerParam.Instance.Hide_teams && Team != player.Team)
                            {
                                temp.Teamname = "unkown";
                                temp.Id = ((BasicObject)player).Id;
                            }
                            temp.Distance = (float?)quantizedDistance;
                            temp.Direction = (int)Math.Round(180.0 / Math.PI * angle, MidpointRounding.AwayFromZero);
                            temp.BodyDir = (int)Math.Round((180.0 / Math.PI) * Utilities.Normalize_angle(player.AngleBodyCommitted + player.AngleBodyCommitted - this.AngleBodyCommitted - this.AngleNeckCommitted), MidpointRounding.AwayFromZero);
                            temp.DirChange = (float?)directionChange;
                            temp.DirChange = (float?)distanceChange;
                            temp.HeadDir = (int)Math.Round((180.0 / Math.PI) * Utilities.Normalize_angle(player.AngleBodyCommitted - this.AngleBodyCommitted - this.AngleNeckCommitted), MidpointRounding.AwayFromZero);
                            temp.Kicking = player.Kicking;
                            temp.Tackling = player.Tackling;
                            temp.Goalie = player.IsGoalie;
                            Visuals.players.Add(temp);

                        }

                    }

                }
                else if (unquantizedDistance < this.VisibleDistanceSquared)
                {

                    temp.Name = "P";

                    Visuals.players.Add(temp);

                }
            }
        }

        /// <summary>
        /// Sends full state message to player
        /// </summary>
        internal void updateFullstate()
        {
            FullState.Time = Stadium.Time;
            FullState.Playmode = Stadium.Playmode;
            FullPlayerData tempPlayerData;

            foreach (var player in Stadium.Players)
            {
                if (!FullState.Players.ContainsKey(player.Id))
                {
                    tempPlayerData = new FullPlayerData();
                    FullState.Players.Add(player.Id, tempPlayerData);

                }
                else
                {
                    tempPlayerData = FullState.Players[player.Id];
                }

                tempPlayerData.BodyDirection = player.AngleBodyCommitted * (float)(180.0 / Math.PI);
                tempPlayerData.Effort = player.Effort;
                tempPlayerData.Goalie = player.IsGoalie;
                tempPlayerData.HeadDirection = player.AngleNeck * (float)(180.0 / Math.PI); ;
                tempPlayerData.Kicking = player.KickCycles > 0;
                tempPlayerData.Recovery = player.Recovery;
                tempPlayerData.Stamina = player.Stamina;
                tempPlayerData.StaminaCapacity = player.StaminaCapacity;
                tempPlayerData.Tackling = player.TackleCycles > 0;
                tempPlayerData.Team = player.Team;
                tempPlayerData.Unum = player.Unum;
                tempPlayerData.VelX = player.Velocity.X;
                tempPlayerData.VelY = player.Velocity.Y;
                tempPlayerData.X = player.Position.X;
                tempPlayerData.Y = player.Position.Y;

            }

            foreach (var ball in Stadium.Balls)
            {

                FullBallData tempBallData;

                if (!FullState.Balls.ContainsKey(ball.Id))
                {
                    tempBallData = new FullBallData();
                    FullState.Balls.Add(ball.Id, tempBallData);
                }
                else
                {
                    tempBallData = FullState.Balls[ball.Id];
                }

                tempBallData.BallId = ball.Id;
                tempBallData.VelX = ball.Velocity.X;
                tempBallData.VelY = ball.Velocity.Y;
                tempBallData.X = ball.Position.X;
                tempBallData.Y = ball.Position.Y;
            }

        }

        /// <summary>
        /// Performs a turn
        /// </summary>
        internal override void turnObject()
        {
            AngleBodyCommitted = AngleBody;
            AngleNeckCommitted = AngleNeck;
            objectVelocity.X = 0.0f;
            objectVelocity.Y = 0.0f;
            objectVelocity.Z = 0.0f;
            objectAcceleration.X = 0.0f;
            objectAcceleration.Y = 0.0f;
            objectAcceleration.Z = 0.0f;
        }

        /// <summary>
        /// Updates player angles
        /// </summary>
        internal override void updateAngle()
        {
            AngleBodyCommitted = AngleBody;
            AngleNeckCommitted = AngleNeck;
        }

        /// <summary>
        /// Sets player collided with post
        /// </summary>
        internal override void collidedWithPost()
        {
            AddState((int)PlayerState.POST_COLLIDE);
            HasCollidedWithPost = true;
        }

        /// <summary>
        /// Decreases the player's hear capacity
        /// </summary>
        /// <param name="sender">The player who is being heard from</param>
        internal void decrementHearCapacity(Player sender)
        {
            if (Team == sender.Team)
            {
                HearCapacityFromTeammate
                    -= ServerParam.Instance.Hear_Decay;
            }
            else
            {
                HearCapacityFromOpponent
                    -= ServerParam.Instance.Hear_Decay;
            }
        }

        /// <summary>
        /// Determines if the player can hear fully from the sender
        /// </summary>
        /// <param name="sender">The player being heard from</param>
        /// <returns>True iff the player is able to hear the player.</returns>
        internal bool canHearFullFrom(Player sender)
        {
            if (Team == (sender.Team))
            {
                return HearCapacityFromTeammate
                    >= (int)ServerParam.Instance.Hear_Decay;
            }
            else
            {
                return HearCapacityFromOpponent
                    >= (int)ServerParam.Instance.Hear_Decay;
            }
        }

        /// <summary>
        /// Recovers all the player's stamina states
        /// </summary>
        internal void recoverAll()
        {
            Stamina = ServerParam.Instance.Stamina_max;
            Recovery = 1.0f;
            Effort = Playertype.EffortMax;

            StaminaCapacity = ServerParam.Instance.Stamina_Capacity;

            ConsumedStamina = 0.0f;

            HearCapacityFromTeammate = ServerParam.Instance.Hear_Max;
            HearCapacityFromOpponent = ServerParam.Instance.Hear_Max;
        }

        /// <summary>
        /// Restores stamina capacity
        /// </summary>
        internal void recoverStaminaCapacity()
        {
            StaminaCapacity = ServerParam.Instance.Stamina_Capacity;
        }

        /// <summary>
        /// Updates stamina, effort, and recovery.. decrementing and incrementing according to parameters
        /// </summary>
        internal void UpdatePlayerStamina()
        {
            ServerParam serverParameters = ServerParam.Instance;

            if (Stamina <= serverParameters.Recover_dec * serverParameters.Stamina_max)
            {
                if (Recovery > serverParameters.Recover_min)
                {
                    Recovery -= serverParameters.Recover_dec;
                }

                if (Recovery < serverParameters.Recover_min)
                {
                    Recovery = serverParameters.Recover_min;
                }
            }

            if (Stamina <= serverParameters.Effort_dec_thr * serverParameters.Stamina_max)
            {
                if (Effort > Playertype.EffortMin)
                {
                    Effort -= serverParameters.Effort_dec;
                }

                if (Effort < Playertype.EffortMin)
                {
                    Effort = Playertype.EffortMin;
                }
            }

            if (Stamina >= serverParameters.Effort_inc_thr * serverParameters.Stamina_max)
            {
                if (Effort < Playertype.EffortMax)
                {
                    Effort += serverParameters.Effort_inc;
                    if (Effort > Playertype.EffortMax)
                    {
                        Effort = Playertype.EffortMax;
                    }
                }
            }

            //Stamina += ( Recovery * Playertype->staminaIncMax() );
            //if ( Stamina > param.staminaMax() )
            //{
            //    Stamina = param.staminaMax();
            //}

            float stamina_inc = Math.Min(Recovery * Playertype.StaminaIncrementMax,
                                           serverParameters.Stamina_max - Stamina);

            if (serverParameters.Stamina_Capacity >= 0.0)
            {
                if (stamina_inc > StaminaCapacity)
                {
                    stamina_inc = StaminaCapacity;
                }
            }

            Stamina += stamina_inc;
            if (Stamina > serverParameters.Stamina_max)
            {
                Stamina = serverParameters.Stamina_max;
            }

            if (serverParameters.Stamina_Capacity >= 0.0)
            {
                StaminaCapacity -= stamina_inc;
                if (StaminaCapacity < 0.0f)
                {
                    StaminaCapacity = 0.0f;
                }
            }
        }

        /// <summary>
        /// Updates the hear capacity for the player
        /// </summary>
        internal void UpdateHearCapacities()
        {
            HearCapacityFromTeammate += ServerParam.Instance.Hear_Inc;
            if (HearCapacityFromTeammate > (int)ServerParam.Instance.Hear_Max)
            {
                HearCapacityFromTeammate = ServerParam.Instance.Hear_Max;
            }

            HearCapacityFromOpponent += ServerParam.Instance.Hear_Inc;
            if (HearCapacityFromOpponent > (int)ServerParam.Instance.Hear_Max)
            {
                HearCapacityFromOpponent = ServerParam.Instance.Hear_Max;
            }

            if (CyclesGoalieCatchBan > 0)
            {
                --CyclesGoalieCatchBan;
            }
        }

        /// <summary>
        /// Resets all collisions to false
        /// </summary>
        internal void ResetCollisionFlags()
        {
            HasCollidedWithBall = false;
            HasCollidedWithPlayer = false;
            HasCollidedWithPost = false;
        }

        /// <summary>
        /// Resets all command flags
        /// </summary>
        internal void ResetCommandFlags()
        {
            if (KickCycles >= 0)
            {
                --KickCycles;
            }

            if (TackleCycles > 0)
            {
                --TackleCycles;
            }

            if (TackleCycles == 0)
            {
                IsPrimaryCommandPerformedThisCycle = false;
            }

            HasNeckBeenTurned = false;
        }

        /// <summary>
        /// Clears the marking that the player is offside
        /// </summary>
        internal void SetNotOffsides()
        {
            IsOffsides = false;
        }

        /// <summary>
        /// Sets the player as offside
        /// </summary>
        /// <param name="offsideXCoordinate">The line that officially marks offsides</param>
        internal void SetAsOffsides(float offsideXCoordinate)
        {
            IsOffsides = true;
            offsidesPosition.X = offsideXCoordinate;
            offsidesPosition.Y = Position.Y;
        }

        /// <summary>
        /// Places the player at a specific location with 0 velcoity and acceleration
        /// </summary>
        /// <param name="newLocation">Location to place the player</param>
        internal void Place(Vector newLocation)
        {
            Position = newLocation;
            objectVelocity.assign(0.0f, 0.0f);
            objectAcceleration.assign(0.0f, 0.0f);
        }

        /// <summary>
        /// Places the player in a specific location, with a specific angle, velocity, and acceleraton
        /// </summary>
        /// <param name="newPosition">Position to place the player</param>
        /// <param name="newAngle">Angle to orient the player</param>
        /// <param name="newVelocity">Velocity to set the player to</param>
        /// <param name="newAcceleration">Acceleration to set the player to</param>
        internal void Place(Vector newPosition, float newAngle, Vector newVelocity, Vector newAcceleration)
        {
            moveTo(newPosition, newVelocity, newAcceleration);


            AngleBody = newAngle;
            AngleBodyCommitted = newAngle;

        }

        #endregion


        #region Communication Methods

        #region AudioStuff

        class StateAudio
        {

            public StateAudio()
            { }

            public virtual
            KeyValuePair<Player, string> getMsg(List<KeyValuePair<Player, string>> msgs) { return new KeyValuePair<Player, string>(null, ""); }

            public virtual
            Player getFocusTarget()
            {
                return null;
            }
        }

        class Unfocused
            : StateAudio
        {

            public Unfocused()
            { }

            public override
            KeyValuePair<Player, string> getMsg(List<KeyValuePair<Player, string>> msgs)
            {

                KeyValuePair<Player, string> temp = new KeyValuePair<Player, string>(null, "");

                if (msgs.Count == 1)
                {

                    temp = msgs[0];
                    msgs.Clear();
                }
                else
                {
                    int idx = Utilities.CustomRandom.Next(msgs.Count);

                    temp = msgs[idx];
                    msgs.RemoveAt(idx);
                }

                return temp;

            }
        }

        class Focused
            : Unfocused
        {

            protected Player M_key;

            public Focused()
            { M_key = null; }

            public virtual
            void focusOn(Player key)
            {
                M_key = key;
            }

            public override
            KeyValuePair<Player, string> getMsg(List<KeyValuePair<Player, string>> msgs)
            {
                KeyValuePair<Player, string> temp;

                int idx = msgs.FindIndex(delegate(KeyValuePair<Player, string> k) { if (k.Key.Equals(M_key)) { return true; } return false; });

                if (idx < 0 || idx > msgs.Count)
                {
                    return base.getMsg(msgs);
                }

                List<KeyValuePair<Player, string>> t = msgs.FindAll(delegate(KeyValuePair<Player, string> k) { if (k.Key.Equals(M_key)) return true; return false; });

                idx = Utilities.CustomRandom.Next(t.Count);

                temp = t[idx];

                msgs.Remove(temp);

                return temp;

            }

            public override
            Player getFocusTarget()
            {
                return M_key;
            }
        }

        // since we might be switching back and forth between being focused
        // or unfocused, both states are created on the construction of
        // PlayerEar rather than creating them on demand and destroying them
        // when we are finished with them.
        Unfocused M_unfocused = new Unfocused();
        Focused M_focused = new Focused();

        StateAudio M_state_p = new StateAudio();
        uint M_focus_count = 0;

        #endregion


        /// <summary>
        /// Sends referee audio contained in string msg
        /// </summary>
        /// <param name="msg"></param>
        internal void sendRefereeAudio(string msg)
        {
            AudioMessage refMsg = new AudioMessage();
            refMsg.msg = msg;
            refMsg.time = Stadium.Time;
            refMsg.source = "referee";
            RefereeMessages.Enqueue(refMsg);
        }


        /// <summary>
        /// Sends player audio
        /// </summary>
        /// <param name="player">Player sending audio</param>
        /// <param name="msg">Message from that player</param>
        internal void sendPlayerAudio(Player player, string msg)
        {
            if (this.Equals(player)) { sendSelfAudio(msg); }
            else
            {

                sendNonSelfPlayerAudio(player, msg);

            }
        }

        private void sendNonSelfPlayerAudio(Player player, string msg)
        {
            if (!nonSelfPlayerPredicate(player))
            {
                return;
            }

            if (!complete.ContainsKey(player.Team))
            {
                complete[player.Team] = false;
            }

            if (!partial.ContainsKey(player.Team))
            {
                partial[player.Team] = false;
            }

            if (!partial[player.Team] && !complete[player.Team])
            {
                return;
            }
            KeyValuePair<Player, string> p = new KeyValuePair<Player, string>(player, msg);
            M_player_msgs.Add(p);
        }

        internal Dictionary<string, bool> complete = new Dictionary<string, bool>();
        internal Dictionary<string, bool> partial = new Dictionary<string, bool>();

        internal List<KeyValuePair<Player, string>> M_player_msgs = new List<KeyValuePair<Player, string>>();

        private bool nonSelfPlayerPredicate(Player player)
        {
            return (this.ThisPlayerState != (int)PlayerState.DISABLE && (player.Position.distance(Position) <= ServerParam.Instance.Audio_Cut_Dist));
        }

        internal Queue<string> M_self_msgs = new Queue<string>();
        private int TimesCarded;
        public int FoulCount;

        private void sendSelfAudio(string msg)
        {
            M_self_msgs.Enqueue(msg);
        }

        /// <summary>
        /// Sends a new cycle
        /// </summary>
        internal void NewCycle()
        {
            foreach (var m in M_self_msgs)
            {
                sendCachedSelfAudio(this, m);
            }

            M_self_msgs.Clear();

            while (M_player_msgs.Count > 0)
            {
                var m = M_state_p.getMsg(M_player_msgs);
                sendCachedNonSelfPlayerAudio(m.Key, m.Value);
            }
        }

        private void sendCachedNonSelfPlayerAudio(Player player, string msg)
        {
            AudioMessage hear = new AudioMessage();
            hear.time = Stadium.Time;


            if (canHearFullFrom(player))
            {
                if (complete.ContainsKey(player.Team) && complete[player.Team])
                {
                    hear.msg = msg;
                    int dir = (int)Math.Round(180.0 / Math.PI * AngleFromBody(player));

                    hear.source = dir.ToString();


                    PlayerMessages.Enqueue(hear);
                    decrementHearCapacity(player);
                }
            }
            else
            {
                if (partial.ContainsKey(player.Team) && partial[player.Team])
                {

                    hear.msg = "";
                    int dir = (int)Math.Round(180.0 / Math.PI * AngleFromBody(player));
                    hear.source = dir.ToString();
                    PlayerMessages.Enqueue(hear);

                }
            }
        }

        private void sendCachedSelfAudio(Player player, string msg)
        {
            AudioMessage hear = new AudioMessage();
            hear.time = Stadium.Time;
            hear.source = "self";
            hear.msg = msg;

            PlayerMessages.Enqueue(hear);
        }

        /// <summary>
        /// Sends a focus on message
        /// </summary>
        /// <param name="player">Player to focus on</param>
        internal void focusOn(Player player)
        {

            M_focused.focusOn(player);
            M_state_p = M_focused;
            M_focus_count++;
        }

        /// <summary>
        /// Sends focus off message
        /// </summary>
        internal void focusOff()
        {
            M_state_p = M_unfocused;
            M_focus_count++;
        }

        /// <summary>
        /// Gets the focus target of the player
        /// </summary>
        /// <returns></returns>
        internal Player getFocusTarget()
        {

            return M_state_p.getFocusTarget();

        }

        /// <summary>
        /// Gets the focus count of the player
        /// </summary>
        /// <returns></returns>
        internal uint getFocusCount()
        {
            return M_focus_count;
        }


        /// <summary>
        /// Sends a set ear message
        /// </summary>
        /// <param name="on">On or off</param>
        /// <param name="side">Side player is on</param>
        /// <param name="complete">Complete?</param>
        /// <param name="partial">Partial?</param>
        internal void setEar(bool on, string team, bool completeP, bool partialP)
        {


            if (completeP)
            {
                complete[team] = on;
                //std::cout << "Set left complete to: " << on << std::endl;
            }
            if (partialP)
            {
                partial[team] = on;
                //std::cout << "Set left partial to: " << on << std::endl;
            }

        }


        #endregion
    }
}
