// -*-C#-*-

/***************************************************************************
                                   Referee.cs
                   Base Referee class, Referees enforce the rules.
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
using RoboCup.Parameters;

namespace RoboCup.Referees
{
    /// <summary>
    /// General referee class to enforce rules of soccer
    /// </summary>
    public class Referee
    {
        #region Class Variables and Static Constructor

        /// <summary>
        /// Area defined as the penalty area
        /// </summary>
        static Rectangle pen_area;

        /// <summary>
        /// "l" and "r" areas of the s_half_time_count
        /// </summary>
        static readonly Rectangle fld_l;
        static readonly Rectangle fld_r;

        static Referee()
        {
            // according to FIFA the ball is catchable if it is at
            // least partly within the penalty area, thus we add ball size
            pen_area = new Rectangle(new Vector(-ServerParam.PITCH_LENGTH / 2
                                        + ServerParam.PENALTY_AREA_LENGTH / 2.0f,
                                        0.0f),
                               new Vector(ServerParam.PENALTY_AREA_LENGTH
                                        + ServerParam.Instance.Ball_size * 2,
                                        ServerParam.PENALTY_AREA_WIDTH
                                        + ServerParam.Instance.Ball_size * 2));

            fld_l = new Rectangle(new Vector(-ServerParam.PITCH_LENGTH / 4, 0.0f),
                              new Vector(ServerParam.PITCH_LENGTH / 2,
                                       ServerParam.PITCH_WIDTH));

            fld_r = new Rectangle(new Vector(ServerParam.PITCH_LENGTH / 4, 0.0f),
                              new Vector(ServerParam.PITCH_LENGTH / 2,
                                       ServerParam.PITCH_WIDTH));
        }

        #endregion

        #region Instance Variables

        internal Stadium M_stadium;
        /// <summary>
        /// stadium the referee is overseeing
        /// </summary>
        public Stadium Stadium
        {
            get { return M_stadium; }
            set { M_stadium = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor of the Referee class
        /// </summary>
        /// <param name="stadium">The stadium the referee oversees</param>
        public Referee(Stadium stadium)
        {
            Stadium = stadium;

        }

        #endregion

        #region Instance Methods

        #region Virtuals
        /// <summary>
        /// Analyzes the current stadium configuration for situations the Referee must handle
        /// </summary>
        public virtual void doAnalyse()
        {

        }

        /// <summary>
        /// Changes the playmode of the stadium
        /// </summary>
        /// <param name="pm">The new playmode</param>
        public virtual void doPlayModeChange(PlayMode pm)
        {

        }

        /// <summary>
        /// Performs necessary actions when ball is touched
        /// </summary>
        /// <param name="it">The player touching the ball</param>
        public virtual void doBallTouched(Player it)
        {

        }

        /// <summary>
        /// Performs necesary actions when player kicks the ball
        /// </summary>
        /// <param name="kicker">The player who is kicking the ball</param>
        public virtual void doKickTaken(Player kicker)
        {

        }

        /// <summary>
        /// Performs necessary actions when player catches the ball
        /// </summary>
        /// <param name="catcher">The player who caught the ball</param>
        public virtual void doCaughtBall(Player catcher)
        {

        }

        #endregion



        /// <summary>
        /// Awards a goal kick to a side
        /// </summary>
        /// <param name="side">string of the goal kick</param>
        /// <param name="pos">Position of the goal kick</param>
        public void awardGoalKick(string side, Vector pos)
        {
            if (pos.Y > 0)
            {
                pos.Y = ServerParam.GOAL_AREA_WIDTH * 0.5f;
            }
            else
            {
                pos.Y = -ServerParam.GOAL_AREA_WIDTH * 0.5f;
            }


            if (side == "l")
            {
                pos.X = -ServerParam.PITCH_LENGTH * 0.5f + ServerParam.GOAL_AREA_LENGTH;
                M_stadium.placeBall(PlayMode.GoalKick_Left, "l", pos, 0);
            }
            else
            {
                pos.X = ServerParam.PITCH_LENGTH * 0.5f - ServerParam.GOAL_AREA_LENGTH;
                M_stadium.placeBall(PlayMode.GoalKick_Right, "r", pos, 0);
            }
        }

        public void awardFreeKick(string side, Vector pos)
        {
            pos = truncateToPitch(pos);
            pos = moveOutOfPenalty(side, pos);

            if (side == "l")
            {

                M_stadium.placeBall(PlayMode.FreeKick_Left, "l", pos);
            }
            else if (side == "r")
            {
                M_stadium.placeBall(PlayMode.FreeKick_Right, "r", pos);
            }
        }

        public void awardDropBall(Vector pos)
        {

            M_stadium.BallCatcher = null;

            pos = truncateToPitch(pos);
            pos = moveOutOfPenalty("n", pos);

            M_stadium.placeBall(PlayMode.Drop_Ball, "n", pos);
            M_stadium.placePlayersInField();

            if (!isPenaltyShootOut(M_stadium.Playmode))
            {
                M_stadium.Playmode = (PlayMode.PlayOn);
            }
        }

        public void awardKickIn(string side, Vector pos)
        {
            M_stadium.BallCatcher = null;

            pos = truncateToPitch(pos);

            if (side == "l")
            {
                M_stadium.placeBall(PlayMode.KickIn_Left, "l", pos);
            }
            else
            {
                M_stadium.placeBall(PlayMode.KickIn_Right, "r", pos);
            }
        }

        public void awardCornerKick(string side, Vector pos)
        {
            M_stadium.BallCatcher = null;

            if (pos.Y > 0)
            {
                pos.Y
                    = ServerParam.PITCH_WIDTH * 0.5f
                    - ServerParam.Instance.Ckick_margin;
            }
            else
            {
                pos.Y
                    = -ServerParam.PITCH_WIDTH * 0.5f
                    + ServerParam.Instance.Ckick_margin;
            }

            if (side == "l")
            {
                pos.X
                    = ServerParam.PITCH_LENGTH * 0.5f
                    - ServerParam.Instance.Ckick_margin;
                M_stadium.placeBall(PlayMode.CornerKick_Left, "l", pos);
            }
            else
            {
                pos.X
                    = ServerParam.PITCH_LENGTH * 0.5f
                    + ServerParam.Instance.Ckick_margin;
                M_stadium.placeBall(PlayMode.CornerKick_Right, "r", pos);
            }
        }

        /// <summary>
        /// Determines if ball has/is crossing the goal line
        /// </summary>
        /// <param name="side">string of the goal</param>
        /// <param name="prev_ball_pos">The ball's previous position</param>
        /// <returns>True iff goal is scored</returns>
        public bool crossGoalLine(string side, Vector prev_ball_pos)
        {

            if (prev_ball_pos.X == M_stadium.Ball.Position.X)
            {
                // Ball[0] cannot have crossed gline
                //          std.cout << time << ": vertcal movement\n";
                return false;
            }

            if (Math.Abs(M_stadium.Ball.Position.X)
                 <= ServerParam.PITCH_LENGTH * 0.5 + ServerParam.Instance.Ball_size)
            {
                // Ball[0] hasn't crossed gline
                //          std.cout << time << ": hasn't crossed\n";
                return false;
            }

            if (Math.Abs(prev_ball_pos.X)
                 > ServerParam.PITCH_LENGTH * 0.5 + ServerParam.Instance.Ball_size)
            {
                // Ball[0] already over the gline
                //          std.cout << time << ": already crossed\n";
                return false;
            }

            if (((side == "l" ? -1 : 1) * M_stadium.Ball.Position.X) >= 0)
            {
                //Ball[0] in wrong half
                //          std.cout << time << ": wrong_half\n";
                return false;
            }

            if (Math.Abs(prev_ball_pos.Y) > (ServerParam.Instance.Goal_width * 0.5
                                                  + ServerParam.Instance.GoalPostRadius)
                 && Math.Abs(prev_ball_pos.X) > ServerParam.PITCH_LENGTH * 0.5)
            {
                // then the only goal that could have been scored would be
                // from going behind the goal post.  I'm pretty sure that
                // isn't possible anyway, but just in case this function acts
                // as a float check
                //          std.cout << time << ": behind_half\n";
                return false;
            }

            float delta_x = M_stadium.Ball.Position.X - prev_ball_pos.X;
            float delta_y = M_stadium.Ball.Position.Y - prev_ball_pos.Y;

            // we already checked above that Ball.pos.X != prev_ball_pos.X, so delta_x cannot be zero.
            float gradient = delta_y / delta_x;
            float offset = prev_ball_pos.Y - gradient * prev_ball_pos.X;

            // determine y for x = ServerParam.PITCH_LENGTH*0.5 + ServerParam.Instance.Ball_size * -side
            float x = (ServerParam.PITCH_LENGTH * 0.5f + ServerParam.Instance.Ball_size) * (side == "l" ? 1 : -1);
            float y_intercept = gradient * x + offset;

            //      std.cout << time << ": prev = " << prev_Ball[0]_pos << std.endl;
            //      std.cout << time << ": curr = " << Ball[0].pos << std.endl;
            //      std.cout << time << ": delta_x = " << delta_x << std.endl;
            //      std.cout << time << ": delta_y = " << delta_y << std.endl;
            //      std.cout << time << ": grad = " << gradient << std.endl;
            //      std.cout << time << ": off = " << offset << std.endl;
            //      std.cout << time << ": x = " << x << std.endl;
            //      std.cout << time << ": y_inter = " << y_intercept << std.endl;


            return Math.Abs(y_intercept) <= (ServerParam.Instance.Goal_width * 0.5
                                                 + ServerParam.Instance.GoalPostRadius);

        }

        /// <summary>
        /// Sets the players on their sides of the field
        /// </summary>
        public virtual void placePlayersInTheirField()
        {


            bool kick_off_offside = ServerParam.Instance.Forbid_Kick_off_Offside
                                          && (M_stadium.Playmode == PlayMode.KickOff_Left || M_stadium.Playmode == PlayMode.KickOff_Right);

            foreach (var it in Stadium.Players)
            {
                if (!it.Enable) continue;

                switch (it.Side)
                {
                    case "l":
                        if (it.Position.X > 0)
                        {
                            if (kick_off_offside)
                            {
                                it.moveTo(new Vector(-it.Size, it.Position.Y), new Vector(0, 0), new Vector(0, 0));
                            }
                            else
                            {
                                it.moveTo(fld_l.RandomLocationInside(), new Vector(0, 0), new Vector(0, 0));
                            }
                        }
                        break;
                    case "r":
                        if (it.Position.X < 0)
                        {
                            if (kick_off_offside)
                            {
                                it.moveTo(new Vector(it.Size, it.Position.Y), new Vector(0, 0), new Vector(0, 0));
                            }
                            else
                            {
                                it.moveTo(fld_r.RandomLocationInside(), new Vector(0, 0), new Vector(0, 0));
                            }
                        }
                        break;
                    case "n":
                    default:
                        break;
                }

                if (it.Side != M_stadium.KickOffSide)
                {
                    Circle expand_c = new Circle(new Vector(0.0f, 0.0f),
                                    ServerParam.KICK_OFF_CLEAR_DISTANCE + it.Size);

                    if (expand_c.containsPoint(it.Position))
                    {
                        it.moveTo(expand_c.closestPointOnCircleToPosition(it.Position), new Vector(0, 0), new Vector(0, 0));
                    }
                }
            }
        }


        public void clearPlayersFromBall(string side)
        {


            PlayMode pm = M_stadium.Playmode;

            float clear_dist = ((pm == PlayMode.Back_Pass_Left
                                         || pm == PlayMode.Back_Pass_Right)
                                       ? ServerParam.GOAL_AREA_LENGTH
                                       : ServerParam.KICK_OFF_CLEAR_DISTANCE);
            bool indirect = (pm == PlayMode.Back_Pass_Left
                                   || pm == PlayMode.Back_Pass_Right
                                   || pm == PlayMode.IndFreeKick_Left
                                   || pm == PlayMode.IndFreeKick_Right);
            float goal_half_width = ServerParam.Instance.Goal_width * 0.5f;

            float max_x = ServerParam.PITCH_LENGTH * 0.5f + ServerParam.PITCH_MARGIN;
            float max_y = ServerParam.PITCH_WIDTH * 0.5f + ServerParam.PITCH_MARGIN;

            bool ball_at_corner
               = (Math.Abs(M_stadium.Ball.Position.X) > max_x - clear_dist
                   && Math.Abs(M_stadium.Ball.Position.Y) > max_y - clear_dist);

            for (int loop = 0; loop < 10; ++loop)
            {
                bool exist = false;
                foreach (Player it in M_stadium.Players)
                {
                    if ((it).ThisPlayerState == (int)PlayerState.DISABLE)
                    {
                        continue;
                    }

                    if (side == "n"
                         || (it).Side == side)
                    {
                        if (indirect
                             && Math.Abs((it).Position.X) >= ServerParam.PITCH_LENGTH * 0.5
                             && Math.Abs((it).Position.Y) <= goal_half_width)
                        {
                            // defender is allowed to stand on the goal line.
                            continue;
                        }

                        Circle clear_area = new Circle(M_stadium.Ball.Position,
                                          clear_dist + (it).Size);
                        if (clear_area.containsPoint((it).Position))
                        {
                            Circle expand_clear_area = new Circle(M_stadium.Ball.Position,
                                                     clear_dist + (it).Size + 1.0e-5f);
                            Vector new_pos = expand_clear_area.closestPointOnCircleToPosition((it).Position);

                            if (ball_at_corner
                                 && Math.Abs(new_pos.X) > ServerParam.PITCH_LENGTH * 0.5
                                 && Math.Abs(new_pos.Y) > ServerParam.PITCH_WIDTH * 0.5)
                            {
                                new_pos -= M_stadium.Ball.Position;
                                new_pos.rotate((float)Math.PI);
                                new_pos += M_stadium.Ball.Position;
                            }

                            if (indirect
                                 && Math.Abs(new_pos.X) > ServerParam.PITCH_LENGTH * 0.5
                                 && Math.Abs(new_pos.Y) < goal_half_width)
                            {
                                float tangent
                                    = (new_pos.Y - M_stadium.Ball.Position.Y)
                                    / (new_pos.X - M_stadium.Ball.Position.X);
                                new_pos.X = ServerParam.PITCH_LENGTH * 0.5f
                                    * (new_pos.X > 0.0 ? 1.0f : -1.0f);
                                new_pos.Y = M_stadium.Ball.Position.Y
                                    + tangent * (new_pos.X - M_stadium.Ball.Position.X);
                            }

                            if (Math.Abs(new_pos.X) > max_x)
                            {
                                float r = clear_dist + (it).Size;
                                float theta = (float)Math.Acos((max_x - Math.Abs(M_stadium.Ball.Position.X)) / r);
                                float tmp_y = (float)Math.Abs(r * Math.Sin(theta));
                                new_pos.X = (new_pos.X < 0.0 ? -max_x : +max_x);
                                new_pos.Y = (float)(M_stadium.Ball.Position.Y
                                              + (new_pos.Y < M_stadium.Ball.Position.Y
                                                  ? -tmp_y - 1.0e-5
                                                  : +tmp_y + 1.0e-5));
                            }

                            if (Math.Abs(new_pos.Y) > max_y)
                            {
                                float r = clear_dist + (it).Size;
                                float theta = (float)Math.Acos((max_y - Math.Abs(M_stadium.Ball.Position.Y)) / r);
                                float tmp_x = (float)Math.Abs(r * Math.Sin(theta));
                                new_pos.X = (float)(M_stadium.Ball.Position.X
                                              + (new_pos.X < M_stadium.Ball.Position.X
                                                  ? -tmp_x - 1.0e-5
                                                  : +tmp_x + 1.0e-5));
                                new_pos.Y = (new_pos.Y < 0.0 ? -max_y : +max_y);
                            }

                            if (Math.Abs(new_pos.X) > max_x
                                 || Math.Abs(new_pos.Y) > max_y)
                            {
                                new_pos -= M_stadium.Ball.Position;
                                new_pos.rotate((float)Math.PI);
                                new_pos += M_stadium.Ball.Position;
                            }

                            (it).Place(new_pos);
                            exist = true;
                        }
                    }
                }

                if (exist)
                {
                    M_stadium.collisions();
                }
                else
                {
                    break;
                }
            }
        }

        #endregion

        #region Class Methods

        /// <summary>
        /// Truncates the ball's position to the pitch
        /// </summary>
        /// <param name="ball_pos">The current ball position</param>
        /// <returns>The truncated ball position</returns>
        public static Vector truncateToPitch(Vector ball_pos)
        {

            ball_pos.X = (float)Math.Min(ball_pos.X, +ServerParam.PITCH_LENGTH * 0.5);
            ball_pos.X = (float)Math.Max(ball_pos.X, -ServerParam.PITCH_LENGTH * 0.5);
            ball_pos.Y = (float)Math.Min(ball_pos.Y, +ServerParam.PITCH_WIDTH * 0.5);
            ball_pos.Y = (float)Math.Max(ball_pos.Y, -ServerParam.PITCH_WIDTH * 0.5);

            return ball_pos;

        }

        /// <summary>
        /// Moves the ball out of penalty position
        /// </summary>
        /// <param name="side">The side to move it to</param>
        /// <param name="ball_pos">The current ball position</param>
        /// <returns>The new ball position</returns>
        public static Vector moveOutOfPenalty(string side, Vector ball_pos)
        {

            if (side != "r")
            {
                if (ball_pos.X <= (-ServerParam.PITCH_LENGTH * 0.5
                                     + ServerParam.PENALTY_AREA_LENGTH)
                     && Math.Abs(ball_pos.Y) <= ServerParam.PENALTY_AREA_WIDTH * 0.5)
                {
                    ball_pos.X
                        = -ServerParam.PITCH_LENGTH * 0.5f
                        + ServerParam.PENALTY_AREA_LENGTH + Defines.Epsilon;
                    if (ball_pos.Y > 0)
                    {
                        ball_pos.Y = +ServerParam.PENALTY_AREA_WIDTH * 0.5f + Defines.Epsilon;
                    }
                    else
                    {
                        ball_pos.Y = -ServerParam.PENALTY_AREA_WIDTH * 0.5f - Defines.Epsilon;
                    }
                }
            }

            if (side != "l")
            {
                if (ball_pos.X >= (ServerParam.PITCH_LENGTH * 0.5
                                     - ServerParam.PENALTY_AREA_LENGTH)
                     && Math.Abs(ball_pos.Y) <= ServerParam.PENALTY_AREA_WIDTH * 0.5)
                {
                    ball_pos.X
                        = ServerParam.PITCH_LENGTH * 0.5f
                        - ServerParam.PENALTY_AREA_LENGTH - Defines.Epsilon;
                    if (ball_pos.Y > 0)
                    {
                        ball_pos.Y = +ServerParam.PENALTY_AREA_WIDTH * 0.5f + Defines.Epsilon;
                    }
                    else
                    {
                        ball_pos.Y = -ServerParam.PENALTY_AREA_WIDTH * 0.5f - Defines.Epsilon;
                    }
                }
            }

            return ball_pos;

        }


        public static Vector moveOutOfGoalArea(string side,
                            Vector ball_pos)
        {
            if (side != "r")
            {
                if (ball_pos.X <= (-ServerParam.PITCH_LENGTH * 0.5
                                     + ServerParam.GOAL_AREA_LENGTH)
                     && Math.Abs(ball_pos.Y) <= ServerParam.GOAL_AREA_WIDTH * 0.5)
                {
                    ball_pos.X
                        = (float)(-ServerParam.PITCH_LENGTH * 0.5
                        + ServerParam.GOAL_AREA_LENGTH + Defines.Epsilon);
                }
            }

            if (side != "l")
            {
                if (ball_pos.X >= (ServerParam.PITCH_LENGTH * 0.5
                                     - ServerParam.GOAL_AREA_LENGTH)
                     && Math.Abs(ball_pos.Y) <= ServerParam.GOAL_AREA_WIDTH * 0.5)
                {
                    ball_pos.X
                        = (float)(ServerParam.PITCH_LENGTH * 0.5
                        - ServerParam.GOAL_AREA_LENGTH - Defines.Epsilon);
                }
            }

            return ball_pos;
        }


        /// <summary>
        /// Moves the ball into penalty position
        /// </summary>
        /// <param name="side">The side of the penalty</param>
        /// <param name="ball_pos">The current ball position</param>
        /// <returns>The new ball position</returns>
        public static Vector moveIntoPenalty(string side, Vector pos)
        {

            if (side != "r")
            {
                if (pos.X > (-ServerParam.PITCH_LENGTH * 0.5
                               + ServerParam.PENALTY_AREA_LENGTH
                               + ServerParam.Instance.Ball_size))
                {
                    pos.X
                        = (float)(-ServerParam.PITCH_LENGTH * 0.5
                        + ServerParam.PENALTY_AREA_LENGTH
                        + ServerParam.Instance.Ball_size);
                }

                if (Math.Abs(pos.Y) > (ServerParam.PENALTY_AREA_WIDTH * 0.5
                                            + ServerParam.Instance.Ball_size))
                {
                    if (pos.Y > 0)
                    {
                        pos.Y
                            = (float)(ServerParam.PENALTY_AREA_WIDTH * 0.5
                            + ServerParam.Instance.Ball_size);
                    }
                    else
                    {
                        pos.Y
                            = (float)(-ServerParam.PENALTY_AREA_WIDTH * 0.5
                            - ServerParam.Instance.Ball_size);
                    }
                }
            }
            if (side != "l")
            {
                if (pos.X < (ServerParam.PITCH_LENGTH * 0.5
                               - ServerParam.PENALTY_AREA_LENGTH
                               - ServerParam.Instance.Ball_size))
                {
                    pos.X
                        = (float)(ServerParam.PITCH_LENGTH * 0.5
                        - ServerParam.PENALTY_AREA_LENGTH
                        - ServerParam.Instance.Ball_size);
                }

                if (Math.Abs(pos.Y) > (ServerParam.PENALTY_AREA_WIDTH * 0.5
                                            + ServerParam.Instance.Ball_size))
                {
                    if (pos.Y > 0)
                    {
                        pos.Y
                            = (float)(ServerParam.PENALTY_AREA_WIDTH * 0.5
                            + ServerParam.Instance.Ball_size);
                    }
                    else
                    {
                        pos.Y
                            = (float)(-ServerParam.PENALTY_AREA_WIDTH * 0.5
                            - ServerParam.Instance.Ball_size);
                    }
                }
            }

            return pos;

        }

        /// <summary>
        /// Determines the position is in a penalty area
        /// </summary>
        /// <param name="side">The side of the penalty</param>
        /// <param name="pos">The current position</param>
        /// <returns>True iff position is in penalty area</returns>
        public static bool inPenaltyArea(string side, Vector pos)
        {


            if (side != "r")
            {

                if (pen_area.ContainsPoint(pos))
                {
                    return true;
                }
            }

            if (side != "l")
            {

                if (pen_area.ContainsPoint(pos))
                {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// Determines if there is a penalty shootout
        /// </summary>
        /// <param name="pm">The current playmode</param>
        /// <param name="side">The current side</param>
        /// <returns>True iff penalty shoout</returns>
        public static bool isPenaltyShootOut(PlayMode pm, string side)
        {

            bool bLeft = false, bRight = true;
            switch (pm)
            {
                case PlayMode.PenaltySetup_Left:
                case PlayMode.PenaltyReady_Left:
                case PlayMode.PenaltyTaken_Left:
                case PlayMode.PenaltyMiss_Left:
                case PlayMode.PenaltyScore_Left:
                    bLeft = true;
                    break;
                case PlayMode.PenaltySetup_Right:
                case PlayMode.PenaltyReady_Right:
                case PlayMode.PenaltyTaken_Right:
                case PlayMode.PenaltyMiss_Right:
                case PlayMode.PenaltyScore_Right:
                    bRight = true;
                    break;
                default:
                    return false;
            }

            if (side == "n" && (bLeft == true || bRight == true))
            {
                return true;
            }
            else if (side == "l" && bLeft == true)
            {
                return true;
            }
            else if (side == "r" && bRight == true)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        /// <summary>
        /// Determines if there is a penalty shootout
        /// </summary>
        /// <param name="pm">The current playmode</param>
        /// <returns>True iff penalty shoout</returns>
        public static bool isPenaltyShootOut(PlayMode pm)
        {
            return isPenaltyShootOut(pm, "n");

        }



        #endregion

        public virtual void doTackleTaken(Player tackler, bool foul)
        {

        }

        public virtual void doPaint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

        }

        public virtual void doNewEpisode()
        {

        }

        public virtual bool doEpisodeEnded()
        {
            return false;
        }
    }

    public class TimeRef : Referee
    {
        int s_half_time_count = 0;

        public TimeRef(Stadium std)
            : base(std)
        {
            s_half_time_count = 0;
        }

        public override void doAnalyse()
        {

            PlayMode pm = M_stadium.Playmode;
            if (pm == PlayMode.BeforeKickOff
                 || pm == PlayMode.TimeOver
                 || pm == PlayMode.AfterGoal_Left
                 || pm == PlayMode.AfterGoal_Right
                 || pm == PlayMode.OffSide_Left
                 || pm == PlayMode.OffSide_Right
                 || pm == PlayMode.Foul_Charge_Left
                 || pm == PlayMode.Foul_Charge_Right
                 || pm == PlayMode.Foul_Push_Left
                 || pm == PlayMode.Foul_Push_Right
                 || pm == PlayMode.Back_Pass_Left
                 || pm == PlayMode.Back_Pass_Right
                 || pm == PlayMode.Free_Kick_Fault_Left
                 || pm == PlayMode.Free_Kick_Fault_Right
                 || pm == PlayMode.CatchFault_Left
                 || pm == PlayMode.CatchFault_Right)
            {
                return;
            }

            RoboCup.Parameters.ServerParam param = RoboCup.Parameters.ServerParam.Instance;

            /* if a value of half_time is negative, then ignore time. */
            if (param.Half_Time > 0)
            {
                int normal_time = param.Half_Time * param.Nr_normal_halfs;
                int maximum_time
                    = normal_time
                    + param.Extra_Half_time * param.Nr_extra_halfs;

                /* check for penalty shoot-outs, half_time and extra_time. */
                if (M_stadium.Time >= maximum_time)
                {
                    if (param.Penalty_Shoot_Outs
                         && M_stadium.TeamLeftPoint == M_stadium.TeamRightPoint)
                    {
                        return; // handled by PenaltyRef
                    }
                    else
                    {
                        M_stadium.sendRefereeAudio("time_up");
                        M_stadium.Playmode = (PlayMode.TimeOver);
                        return;
                    }
                }
                // overtime
                else if (M_stadium.Time >= normal_time)
                {
                    int extra_count = (s_half_time_count + 1) - param.Nr_normal_halfs;

                    if (!M_stadium.Players.Exists((p => p.Enable && p.Side == "l"))
                         || !!M_stadium.Players.Exists((p => p.Enable && p.Side == "r")))
                    {
                        M_stadium.sendRefereeAudio("time_up_without_a_team");
                        M_stadium.Playmode = (PlayMode.TimeOver);
                        return;
                    }
                    // check half time in overtime
                    else if (M_stadium.Time >= (normal_time
                                                    + (param.Extra_Half_time * extra_count)))
                    {
                        // when normal halves have just finished (i.e. extra_count==0),
                        // referee always check the score difference.
                        if (extra_count == 0
                             && M_stadium.TeamLeftPoint != M_stadium.TeamRightPoint)
                        {
                            M_stadium.sendRefereeAudio("time_up");
                            M_stadium.Playmode = (PlayMode.TimeOver);
                        }
                        // otherwise, the game is go into the overtime.
                        else
                        {
                            ++s_half_time_count;
                            M_stadium.sendRefereeAudio("time_extended");
                            string kick_off_side = (s_half_time_count % 2 == 0
                                                   ? "l"
                                                   : "r");
                            M_stadium.callHalfTime(kick_off_side, s_half_time_count);
                            placePlayersInTheirField();
                        }

                        return;
                    }
                }
                // if not in overtime, check whether halfTime cycles have been passed
                else if (M_stadium.Time >= param.Half_Time * (s_half_time_count + 1))
                {
                    ++s_half_time_count;
                    string kick_off_side = (s_half_time_count % 2 == 0
                                           ? "l"
                                           : "r");
                    M_stadium.sendRefereeAudio("half_time");
                    M_stadium.callHalfTime(kick_off_side, s_half_time_count);
                    placePlayersInTheirField();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Referee class that handles offsides situations
    /// </summary>
    class OffsideRef : RoboCup.Referees.Referee
    {
        #region Constants

        /// <summary>
        /// Time to wait after offsides
        /// </summary>
        private const int AFTER_OFFSIDE_WAIT = 30;

        #endregion

        #region Instance Variables

        /// <summary>
        /// The last player to kick the ball
        /// </summary>
        private RoboCup.Objects.Player M_last_kicker;
        /// <summary>
        /// The last offside position
        /// </summary>
        private Geometry.Vector M_offside_pos;
        /// <summary>
        /// Time since the last offside
        /// </summary>
        private int M_after_offside_time;

        /// <summary>
        /// The corner kick area
        /// </summary>
        readonly Geometry.Rectangle pt = new RoboCup.Geometry.Rectangle(new Vector(0.0f, 0.0f),
                           new Vector(ServerParam.PITCH_LENGTH
                                    - 2.0f * ServerParam.Instance.Ckick_margin,
                                    ServerParam.PITCH_WIDTH
                                    - 2.0f * ServerParam.Instance.Ckick_margin));
        /// <summary>
        /// The "l" goal area
        /// </summary>
        readonly Geometry.Rectangle g_l = new Rectangle(new Vector(-ServerParam.PITCH_LENGTH / 2.0f
                                         + ServerParam.GOAL_AREA_LENGTH / 2.0f,
                                         0.0f),
                                new Vector(ServerParam.GOAL_AREA_LENGTH,
                                         ServerParam.GOAL_AREA_WIDTH));

        /// <summary>
        /// The "r" goal area
        /// </summary>
        readonly Geometry.Rectangle g_r = new Rectangle(new Vector(+ServerParam.PITCH_LENGTH / 2.0f
                                         - ServerParam.GOAL_AREA_LENGTH / 2.0f,
                                         0.0f),
                                new Vector(ServerParam.GOAL_AREA_LENGTH,
                                         ServerParam.GOAL_AREA_WIDTH));

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor for Offside Referee Class
        /// </summary>
        /// <param name="stadium">The stadium the referee is observing</param>
        public OffsideRef(Stadium stadium)
            : base(stadium)
        {
            M_after_offside_time = 0;
            M_last_kicker = null;
            M_offside_pos = new RoboCup.Geometry.Vector(0, 0);
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Analyzes for offsides situations
        /// </summary>
        public override void doAnalyse()
        {
            if (!ServerParam.Instance.Use_Offside)
            {
                return;
            }

            if (isPenaltyShootOut(M_stadium.Playmode))
            {
                return;
            }

            if (M_stadium.Playmode == PlayMode.BeforeKickOff
                 || M_stadium.Playmode == PlayMode.KickOff_Left
                || M_stadium.Playmode == PlayMode.KickOff_Right
                )
            {
                if (ServerParam.Instance.Forbid_Kick_off_Offside)
                {
                    placePlayersInTheirField();
                }
                return;
            }

            if (M_stadium.Playmode == PlayMode.OffSide_Left)
            {
                clearPlayersFromBall("l");
                checkPlayerAfterOffside("l");
                if (++M_after_offside_time > AFTER_OFFSIDE_WAIT)
                {
                    M_stadium.change_play_mode(PlayMode.FreeKick_Right);
                }
                return;
            }

            if (M_stadium.Playmode == PlayMode.OffSide_Right)
            {
                clearPlayersFromBall("r");
                checkPlayerAfterOffside("r");
                if (++M_after_offside_time > AFTER_OFFSIDE_WAIT)
                {
                    M_stadium.change_play_mode(PlayMode.FreeKick_Left);
                }
                return;
            }

            if (M_stadium.Playmode != PlayMode.PlayOn)
            {
                foreach (var p in M_stadium.Players)
                {
                    (p).SetNotOffsides();
                }
                return;
            }

            float dist = ServerParam.Instance.Offside_Active_Area_Size;

            {
                foreach (var p in M_stadium.Players)
                {
                    if ((p).IsOffsides)
                    {
                        float tmp = (p).Position.distance(M_stadium.Ball.Position);
                        if (tmp < dist)
                        {
                            dist = tmp;
                            M_offside_pos = (p).OffsidePos;
                        }
                    }
                }
            }

            if (dist != ServerParam.Instance.Offside_Active_Area_Size)
            {
                callOffSide();
            }
        }

        /// <summary>
        /// Handles the ball being touched
        /// </summary>
        /// <param name="it">The player touching the ball</param>
        public override void doBallTouched(RoboCup.Objects.Player it)
        {
            if (!ServerParam.Instance.Use_Offside)
            {
                return;
            }

            M_last_kicker = it;

            setOffsideMark(it);
        }

        /// <summary>
        /// Handles when a kick is taken offside
        /// </summary>
        /// <param name="kicker">The player who is kicking the ball</param>
        public override void doKickTaken(RoboCup.Objects.Player kicker)
        {
            if (!ServerParam.Instance.Use_Offside)
            {
                return;
            }

            M_last_kicker = kicker;

            setOffsideMark(kicker);
        }

        /// <summary>
        /// Performs necessary changes with respect to play mode
        /// </summary>
        /// <param name="pm">The new playmode</param>
        public override void doPlayModeChange(RoboCup.PlayMode pm)
        {
            if (pm != PlayMode.PlayOn)
            {
                foreach (var p in Stadium.Players)
                {
                    (p).SetNotOffsides();
                }
            }
        }

        /// <summary>
        /// Sets the position where the offsides occurred
        /// </summary>
        /// <param name="kicker">The player who kicked the offsides</param>
        private void setOffsideMark(Objects.Player kicker)
        {
            if (kicker.IsOffsides)
            {
                M_offside_pos = kicker.OffsidePos;
                callOffSide();
                return;
            }

            foreach (var p in Stadium.Players)
            {

                (p).SetNotOffsides();

            }

            //if (m_no_offsides )
            if (M_stadium.Playmode == PlayMode.GoalKick_Left

                || M_stadium.Playmode == PlayMode.KickIn_Left

                || M_stadium.Playmode == PlayMode.CornerKick_Left
                || M_stadium.Playmode == PlayMode.GoalKick_Right

                || M_stadium.Playmode == PlayMode.KickIn_Right

                || M_stadium.Playmode == PlayMode.CornerKick_Right
                )
            {
                return;
            }

            float fast = 0;
            float second = 0;
            float offside_line;

            switch (kicker.Side)
            {
                case "l":
                    foreach (var p in Stadium.Players)
                    {
                        if ((p).ThisPlayerState == (int)PlayerState.DISABLE)
                        {
                            continue;
                        }

                        if ((p).Side == "r")
                        {
                            if ((p).Position.X > second)
                            {
                                second = (p).Position.X;
                                if (second > fast)
                                {
                                    float temp;
                                    temp = second;
                                    second = fast;
                                    fast = temp;

                                }
                            }
                        }
                    }

                    if (second > M_stadium.Ball.Position.X)
                    {
                        offside_line = second;
                    }
                    else
                    {
                        offside_line = M_stadium.Ball.Position.X;
                    }

                    foreach (var p in Stadium.Players)
                    {
                        if ((p).ThisPlayerState == (int)PlayerState.DISABLE)
                        {
                            continue;
                        }

                        if ((p).Side == "l"
                             && (p).Position.X > offside_line
                             && (p).Unum != kicker.Unum)
                        {
                            (p).SetAsOffsides(offside_line);
                        }
                    }
                    break;

                case "r":
                    foreach (var p in Stadium.Players)
                    {
                        if ((p).ThisPlayerState == (int)PlayerState.DISABLE)
                        {
                            continue;
                        }

                        if ((p).Side == "l")
                        {
                            if ((p).Position.X < second)
                            {
                                second = (p).Position.X;
                                if (second < fast)
                                {
                                    float temp;
                                    temp = second;
                                    second = fast;
                                    fast = temp;
                                }
                            }
                        }
                    }

                    if (second < M_stadium.Ball.Position.X)
                    {
                        offside_line = second;
                    }
                    else
                    {
                        offside_line = M_stadium.Ball.Position.X;
                    }

                    foreach (var p in Stadium.Players)
                    {
                        if ((p).ThisPlayerState == (int)PlayerState.DISABLE)
                            continue;
                        if ((p).Side == "r"
                             && (p).Position.X < offside_line
                             && (p).Unum != kicker.Unum)
                        {
                            (p).SetAsOffsides(offside_line);
                        }
                    }
                    break;
                case "n":
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Calls for offsides
        /// </summary>
        private void callOffSide()
        {
            if (isPenaltyShootOut(M_stadium.Playmode))
            {
                return;
            }

            Geometry.Vector pos = new RoboCup.Geometry.Vector(0, 0);



            M_after_offside_time = 0;

            if (M_offside_pos.X > ServerParam.PITCH_LENGTH / 2.0
                 || g_r.ContainsPoint(M_offside_pos))
            {
                pos.X = +(float)(ServerParam.PITCH_LENGTH / 2.0 - ServerParam.GOAL_AREA_LENGTH);
                pos.Y = (float)((M_offside_pos.Y > 0 ? 1 : -1) * ServerParam.GOAL_AREA_WIDTH / 2.0);
            }
            else if (M_offside_pos.X < -ServerParam.PITCH_LENGTH / 2.0
                      || g_l.ContainsPoint(M_offside_pos))
            {
                pos.X = (float)(-ServerParam.PITCH_LENGTH / 2.0 + ServerParam.GOAL_AREA_LENGTH);
                pos.Y = (float)((M_offside_pos.Y > 0 ? 1 : -1) * ServerParam.GOAL_AREA_WIDTH / 2.0);
            }
            else if (!pt.ContainsPoint(M_offside_pos))
            {
                pos = pt.NearestEdgeToPoint(M_offside_pos);
            }
            else
            {
                pos = M_offside_pos;
            }

            if (M_last_kicker.Side == "l")
            {
                M_stadium.placeBall(PlayMode.OffSide_Left, "r", pos);
            }
            else
            {
                M_stadium.placeBall(PlayMode.OffSide_Right, "l", pos);
            }



            foreach (var p in Stadium.Players)
            {
                (p).SetNotOffsides();
            }

            M_stadium.placePlayersInField();
            clearPlayersFromBall(M_last_kicker.Side);
        }

        /// <summary>
        /// Checks a player after an offsides
        /// </summary>
        private void checkPlayerAfterOffside(string side)
        {
            Circle c = new Circle(M_offside_pos, ServerParam.Instance.Offside_Active_Area_Size);

            string offsideside = "n";
            Vector ce = new Vector(0, 0), si = new Vector(0, 0);

            if (M_stadium.Playmode == PlayMode.OffSide_Right)
            {
                ce += new Vector((float)(ServerParam.PITCH_LENGTH / 4 + M_offside_pos.X / 2), 0.0f);
                si += new Vector(ServerParam.PITCH_LENGTH / 2 - M_offside_pos.X,
                               ServerParam.PITCH_WIDTH);
                offsideside = "r";
            }
            else if (M_stadium.Playmode == PlayMode.OffSide_Left)
            {
                ce += new Vector(-ServerParam.PITCH_LENGTH / 4 + M_offside_pos.X / 2, 0.0f);
                si += new Vector(ServerParam.PITCH_LENGTH / 2 + M_offside_pos.X,
                               ServerParam.PITCH_WIDTH);
                offsideside = "l";
            }
            else
            {
                return;
            }

            Rectangle fld = new Rectangle(ce, si);

            foreach (var p in Stadium.Players)
            {
                if (p != null && (p).ThisPlayerState == (int)PlayerState.DISABLE)
                {
                    continue;
                }

                if ((p).Side == offsideside)
                {
                    if (c.containsPoint((p).Position))
                    {
                        (p).moveTo(c.closestPointOnCircleToPosition((p).Position), new Vector(0, 0), new Vector(0, 0));
                    }
                    if (!fld.ContainsPoint((p).Position))
                    {
                        if (M_stadium.Playmode == PlayMode.OffSide_Right && offsideside == "r")
                        {
                            (p).moveTo(fld.NearestVerticalEdgeToPoint((p).Position)
                                                            + new Vector(ServerParam.Instance.Offside_Kick_Margin, 0), new Vector(0, 0), new Vector(0, 0));
                        }
                        else
                        {
                            (p).moveTo(fld.NearestVerticalEdgeToPoint((p).Position)
                                                            - new Vector(ServerParam.Instance.Offside_Kick_Margin, 0), new Vector(0, 0), new Vector(0, 0));
                        }
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Keepaway Referee class to enforce the rules of Keepaway
    /// </summary>
    public class KeepawayRef : RoboCup.Referees.Referee
    {
        #region Instance Variables and Constants

        public bool episodeEnded = false;
        /// <summary>
        /// The keepaway training message
        /// </summary>
        private const string trainingMsg = "training Keepaway 1";
        /// <summary>
        /// Time until the ball is considered turned over
        /// </summary>
        private const int TURNOVER_TIME = 20;
        /// <summary>
        /// The episode number
        /// </summary>
        private int M_episode;
        /// <summary>
        /// The number of keepers
        /// </summary>
        private int M_keepers;
        /// <summary>
        /// The number of takers
        /// </summary>
        private int M_takers;
        /// <summary>
        /// The time
        /// </summary>
        private int M_time;
        /// <summary>
        /// The time it has been taken
        /// </summary>
        private int M_take_time;
        /// <summary>
        /// The start time of the current episode
        /// </summary>
        private DateTime M_start_time;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor of the Keepaway referee
        /// </summary>
        /// <param name="stadium">The stadium the referee is observing</param>
        public KeepawayRef(Stadium stadium)
            : base(stadium)
        {
            M_episode = 0;
            M_keepers = 0;
            M_takers = 0;
            M_time = 0;
            M_take_time = 0;
            M_start_time = DateTime.Now;
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Analyzes the situation for keepaway rules
        /// </summary>
        public override void doAnalyse()
        {
            if (ServerParam.Instance.Keepaway)
            {
                if (Stadium.Time == 0)
                {
                    resetField();
                }

                if (M_stadium.Playmode == PlayMode.PlayOn)
                {
                    if (!ballInKeepawayArea())
                    {

                        //  M_stadium.sendRefereeAudio(trainingMsg);
                        resetField();
                        episodeEnded = true;
                    }
                    else if (M_take_time >= TURNOVER_TIME)
                    {
                        //  M_stadium.sendRefereeAudio(trainingMsg);
                        resetField();
                        episodeEnded = true;
                    }
                    else
                    {
                        bool keeperPoss = false;

                        foreach (var p in M_stadium.Players)
                        {
                            Vector ppos = (p).Position;

                            if ((p).ThisPlayerState != (int)PlayerState.DISABLE
                                 && ppos.distance(M_stadium.Ball.Position)
                                 < ServerParam.Instance.KickableArea)
                            {
                                if ((p).Side == "l")
                                {
                                    keeperPoss = true;
                                }
                                else if ((p).Side == "r")
                                {
                                    keeperPoss = false;
                                    M_take_time++;
                                    break;
                                }
                            }
                        }
                        if (keeperPoss)
                            M_take_time = 0;
                    }
                }
            }
        }

        /// <param name="it">The player touching the ball</param>
        public override void doBallTouched(RoboCup.Objects.Player it)
        {
        }

        /// <param name="kicker">The player who is kicking the ball</param>
        public override void doKickTaken(RoboCup.Objects.Player kicker)
        {
        }

        /// <summary>
        /// Performs the necessary playmode updates for keepaway
        /// </summary>
        /// <param name="pm">The new playmode</param>
        public override void doPlayModeChange(RoboCup.PlayMode pm)
        {
            if (ServerParam.Instance.Keepaway)
            {
                if (pm == PlayMode.PlayOn && M_episode == 0)
                {
                    M_episode = 1;

                    foreach (var p in M_stadium.Players)
                    {
                        if ((p).ThisPlayerState != (int)PlayerState.DISABLE)
                        {
                            if ((p).Side == "l")
                                M_keepers++;
                            else if ((p).Side == "r")
                                M_takers++;
                        }
                    }

                    resetField();
                }
            }
        }

        /// <summary>
        /// Checks if the ball is within the keepaway area
        /// </summary>
        private bool ballInKeepawayArea()
        {
            Vector ball_pos = M_stadium.Ball.Position;
            return (Math.Abs(ball_pos.X) < ServerParam.Instance.Keepaway_Length * 0.5 &&
                     Math.Abs(ball_pos.Y) < ServerParam.Instance.Keepaway_Width * 0.5);
        }



        /// <summary>
        /// Resets the keepaway field after an episode
        /// </summary>
        private void resetField()
        {
            M_keepers = 0;
            //    M_start_time = Stadium.Time;
            M_take_time = 0;
            M_takers = 0;
            M_time = Stadium.Time;

            for (int i = 0; i < Stadium.Players.Count; i++)
            {

                if (Stadium.Players[i].Team == "keepers")
                {
                    M_keepers++;
                }
                else if (Stadium.Players[i].Team == "takers")
                {
                    M_takers++;

                }

            }
            int keeper_pos = Utilities.CustomRandom.Next(M_keepers);
            Stadium.Time = 0;
            //int keeper_pos = boost.uniform_smallint<>( 0, M_keepers - 1 )( rcss.random.DefaultRNG.Instance );

            foreach (var p in Stadium.Players)
            {
                if (p != null && (p).ThisPlayerState != (int)PlayerState.DISABLE)
                {
                    float x, y;
                    if ((p).Side == "l")
                    {
                        switch (keeper_pos)
                        {
                            case 0:
                                x = (float)(-ServerParam.Instance.Keepaway_Length * 0.5 + Utilities.drand(0, 3));
                                y = (float)(-ServerParam.Instance.Keepaway_Width * 0.5 + Utilities.drand(0, 3));
                                break;
                            case 1:
                                x = (float)(ServerParam.Instance.Keepaway_Length * 0.5 - Utilities.drand(0, 3));
                                y = (float)(-ServerParam.Instance.Keepaway_Width * 0.5 + Utilities.drand(0, 3));
                                break;
                            case 2:
                                x = (float)(ServerParam.Instance.Keepaway_Length * 0.5 - Utilities.drand(0, 3));
                                y = (float)(ServerParam.Instance.Keepaway_Width * 0.5 - Utilities.drand(0, 3));
                                break;
                            default:
                                x = (float)(Utilities.drand(-1, 1)); y = (float)(Utilities.drand(-1, 1));
                                break;
                        }

                        (p).Place(new Vector(x, y));

                        keeper_pos = (keeper_pos + 1) % M_keepers;
                    }
                    else if ((p).Side == "r")
                    {
                        x = -(float)(ServerParam.Instance.Keepaway_Length * 0.5 + Utilities.drand(0, 3));
                        y = (float)(ServerParam.Instance.Keepaway_Width * 0.5 - Utilities.drand(0, 3));

                        (p).Place(new Vector(x, y));
                    }
                }
            }

            M_stadium.set_ball(
                                new Vector((float)(-ServerParam.Instance.Keepaway_Length * 0.5 + 4.0),
                                         (float)(-ServerParam.Instance.Keepaway_Width * 0.5 + 4.0)));
            M_take_time = 0;
        }

        #endregion
    }

    public class BallStuckRef : Referee
    {

        public Vector M_last_ball_pos;
        public int M_counter;

        public BallStuckRef(Stadium std)
            : base(std)
        {
            M_last_ball_pos = new Vector(-1000, -1000);
            M_counter = 0;
        }
        public override void doAnalyse()
        {

            if (ServerParam.Instance.Ball_Stuck_Area <= 0.0)
            {
                return;
            }

            if (M_stadium.Playmode != PlayMode.PlayOn)
            {
                M_last_ball_pos.assign(M_stadium.Ball.Position);
                M_counter = 0;
                return;
            }

            if (M_stadium.Ball.Position.distanceSquared(M_last_ball_pos)
                 <= Math.Pow(ServerParam.Instance.Ball_Stuck_Area, 2))
            {
                ++M_counter;

                if (M_counter >= 50)
                {
                    M_last_ball_pos.assign(M_stadium.Ball.Position);
                    M_counter = 0;

                    awardDropBall(M_stadium.Ball.Position);
                }
            }
            else
            {
                M_last_ball_pos.assign(M_stadium.Ball.Position);
                M_counter = 0;
            }

        }
    }

    public class FreeKickRef : Referee
    {
        const int AFTER_FREE_KICK_FAULT_WAIT = 30;
        public int M_goal_kick_count = 0;
        public bool M_kick_taken = false;
        public Player M_kick_taker = null;
        public int M_kick_taker_dashes = 0;
        public int M_after_free_kick_fault_time = 0;
        private int M_timer;

        public FreeKickRef(Stadium std)
            : base(std)
        {
            M_goal_kick_count = 0;
            M_kick_taken = false;
            M_kick_taker = null;
            M_kick_taker_dashes = 0;
        }

        public bool goalKick(PlayMode pm)
        {
            return (pm == PlayMode.GoalKick_Left || pm == PlayMode.GoalKick_Right);
        }

        public override void doKickTaken(Player kicker)
        {
            if (isPenaltyShootOut(M_stadium.Playmode))
            {
                return;
            }

            if (goalKick(M_stadium.Playmode))
            {
                if ((M_stadium.Playmode == PlayMode.GoalKick_Right
                       && kicker.Side != "l")
                     || (M_stadium.Playmode == PlayMode.GoalKick_Left
                          && kicker.Side != "r")
                     )
                {
                    // opponent player kicks tha ball while ball is in penalty areas.
                    awardGoalKick(kicker.Side == "l" ? "r" : "l", M_stadium.Ball.Position);
                    M_goal_kick_count = -1;
                    M_kick_taken = false;
                    return;
                }

                if (M_kick_taken)
                {
                    // ball was not kicked directly into play (i.e. out of penalty area
                    // without touching another player
                    if (!kicker.Equals(M_kick_taker))
                    {
                        if (ServerParam.Instance.Proper_Goal_Kicks)
                        {
                            awardGoalKick(M_kick_taker.Side,
                                           M_stadium.Ball.Position);
                        }
                    }
                    else if (M_kick_taker_dashes != M_kick_taker.DashCount)
                    {
                        if (ServerParam.Instance.Free_Kick_Faults)
                        {
                            M_stadium.setPlayerState(M_kick_taker.Team,
                                                      M_kick_taker.Unum,
                                                      (int)PlayerState.FREE_KICK_FAULT);
                            callFreeKickFault(kicker.Side,
                                               M_stadium.Ball.Position);
                        }
                        else if (ServerParam.Instance.Proper_Goal_Kicks)
                        {
                            awardGoalKick(M_kick_taker.Side,
                                           M_stadium.Ball.Position);
                        }
                    }
                    // else it's part of a compound kick
                }
                //else
                //{
                M_kick_taken = true;
                M_kick_taker = kicker;
                M_kick_taker_dashes = M_kick_taker.DashCount;
                //}
            }
            else if (M_stadium.Playmode != PlayMode.PlayOn)
            {
                M_kick_taken = true;
                M_kick_taker = kicker;
                M_kick_taker_dashes = M_kick_taker.DashCount;
                M_stadium.Playmode = (PlayMode.PlayOn);
            }
            else // PlayMode.PlayOn
            {
                if (M_kick_taken)
                {
                    if (M_kick_taker == kicker
                         && ServerParam.Instance.Free_Kick_Faults)
                    {
                        if (M_kick_taker.DashCount > M_kick_taker_dashes)
                        {
                            M_stadium.setPlayerState(M_kick_taker.Team,
                                                      M_kick_taker.Unum,
                                                      (int)PlayerState.FREE_KICK_FAULT);
                            callFreeKickFault(M_kick_taker.Side,
                                               M_stadium.Ball.Position);
                        }
                    }
                    else
                    {
                        M_kick_taken = false;
                    }
                }
            }
        }

        public void callFreeKickFault(string side, Vector pos)
        {
            pos = truncateToPitch(pos);
            pos = moveOutOfPenalty(side, pos);

            M_stadium.BallCatcher = null;

            if (side == "l")
            {
                M_stadium.placeBall(PlayMode.Free_Kick_Fault_Left, "r", pos);
            }
            else if (side == "r")
            {
                M_stadium.placeBall(PlayMode.Free_Kick_Fault_Right, "l", pos);
            }

            M_after_free_kick_fault_time = 0;
        }

        public bool freeKick(PlayMode pm)
        {
            switch (pm)
            {
                case PlayMode.KickOff_Left:
                case PlayMode.KickIn_Left:
                case PlayMode.FreeKick_Left:
                case PlayMode.CornerKick_Left:
                case PlayMode.IndFreeKick_Left:
                case PlayMode.KickOff_Right:
                case PlayMode.KickIn_Right:
                case PlayMode.FreeKick_Right:
                case PlayMode.CornerKick_Right:
                case PlayMode.IndFreeKick_Right:
                    return true;
                default:
                    return false;
            }
        }

        public bool ballStopped()
        {
            return M_stadium.Ball.Velocity.Radius < ServerParam.Instance.Stopped_Ball_Vel;
        }

        public bool tooManyGoalKicks()
        {
            return M_goal_kick_count >= ServerParam.Instance.Max_Goal_Kicks;
        }

        public override void doBallTouched(Player it)
        {
            if ((M_stadium.Playmode == PlayMode.GoalKick_Left
          && it.Side != "l")
        || (M_stadium.Playmode == PlayMode.GoalKick_Right
             && it.Side != "r")
        )
            {
                // opponent it kicks tha ball while ball is in penalty area.
                awardGoalKick(it.Side == "r" ? "l" : "r", M_stadium.Ball.Position);
                M_goal_kick_count = -1;
                M_kick_taken = false;
                return;
            }

            if (M_kick_taken)
            {
                if (M_kick_taker.Equals(it)
                     && ServerParam.Instance.Free_Kick_Faults)
                {
                    if (M_kick_taker.DashCount > M_kick_taker_dashes)
                    {
                        M_stadium.setPlayerState(M_kick_taker.Team,
                                                  M_kick_taker.Unum,
                                                  (int)PlayerState.FREE_KICK_FAULT);
                        callFreeKickFault(M_kick_taker.Side,
                                           M_stadium.Ball.Position);
                    }
                    /// else do nothing yet as the it just colided with the ball instead of dashing into it
                }
                else
                {
                    M_kick_taken = false;
                }
            }
        }

        public override void doAnalyse()
        {
            PlayMode pm = M_stadium.Playmode;

            if (isPenaltyShootOut(pm))
            {
                return;
            }

            if (pm == PlayMode.Free_Kick_Fault_Left)
            {
                clearPlayersFromBall("l");
                if (++M_after_free_kick_fault_time > AFTER_FREE_KICK_FAULT_WAIT)
                {
                    M_stadium.Playmode = (PlayMode.FreeKick_Right);
                }
                return;
            }

            if (pm == PlayMode.Free_Kick_Fault_Right)
            {
                clearPlayersFromBall("r");
                if (++M_after_free_kick_fault_time > AFTER_FREE_KICK_FAULT_WAIT)
                {
                    M_stadium.Playmode = (PlayMode.FreeKick_Left);
                }
                return;
            }

            if (pm == PlayMode.Back_Pass_Left
                 || pm == PlayMode.Back_Pass_Right
                 || pm == PlayMode.CatchFault_Left
                 || pm == PlayMode.CatchFault_Right)
            {
                // analyzed by CatchRef
                return;
            }

            if (goalKick(pm))
            {
                placePlayersForGoalkick();
                M_stadium.placePlayersInField();

                if (!inPenaltyArea("n", M_stadium.Ball.Position))
                {
                    M_stadium.Playmode = (PlayMode.PlayOn);
                }
                else
                {
                    if (M_kick_taken
                         && ServerParam.Instance.Proper_Goal_Kicks)
                    {
                        if (ballStopped())
                        {
                            if (tooManyGoalKicks())
                            {
                                awardFreeKick(M_kick_taker.Side == "l" ? "r" : "l",
                                               M_stadium.Ball.Position);
                            }
                            else
                            {
                                awardGoalKick(M_kick_taker.Side,
                                               M_stadium.Ball.Position);
                            }
                        }
                    }
                    else
                    {
                        if (M_timer > -1)
                        {
                            M_timer--;
                        }

                        if (M_timer == 0)
                        {
                            awardDropBall(M_stadium.Ball.Position);
                        }
                    }
                }

                return;
            }

            if (pm != PlayMode.PlayOn
                 && pm != PlayMode.BeforeKickOff
                 && pm != PlayMode.TimeOver
                 && pm != PlayMode.AfterGoal_Left
                 && pm != PlayMode.AfterGoal_Right
                 && pm != PlayMode.OffSide_Left
                 && pm != PlayMode.OffSide_Right
                 && pm != PlayMode.Foul_Charge_Left
                 && pm != PlayMode.Foul_Charge_Right
                 && pm != PlayMode.Foul_Push_Left
                 && pm != PlayMode.Foul_Push_Right
                 && pm != PlayMode.Back_Pass_Left
                 && pm != PlayMode.Back_Pass_Right
                 && pm != PlayMode.Free_Kick_Fault_Left
                 && pm != PlayMode.Free_Kick_Fault_Right
                 && pm != PlayMode.CatchFault_Left
                 && pm != PlayMode.CatchFault_Right)
            {
                if (M_stadium.Ball.Velocity.X != 0.0
                     || M_stadium.Ball.Velocity.Y != 0.0)
                {
                    M_stadium.Playmode = (PlayMode.PlayOn);
                }
            }

            M_stadium.placePlayersInField();

            if (pm != PlayMode.PlayOn
                 && pm != PlayMode.TimeOver
                 && pm != PlayMode.GoalKick_Left
                 && pm != PlayMode.GoalKick_Right)
            {
                clearPlayersFromBall(M_stadium.KickOffSide == "r" ? "l" : "r");
            }

            if (freeKick(pm))
            {
                if (M_timer > -1)
                {
                    M_timer--;
                }

                if (M_timer == 0)
                {
                    awardDropBall(M_stadium.Ball.Position);
                }
            }
        }
        static Rectangle p_l = new Rectangle(new Vector(-ServerParam.PITCH_LENGTH / 2
                                 + ServerParam.PENALTY_AREA_LENGTH / 2.0f,
                                 0.0f),
                        new Vector(ServerParam.PENALTY_AREA_LENGTH,
                                 ServerParam.PENALTY_AREA_WIDTH));

        static Rectangle p_r = new Rectangle(new Vector(ServerParam.PITCH_LENGTH / 2
                         - ServerParam.PENALTY_AREA_LENGTH / 2.0f,
                         0.0f),
                new Vector(ServerParam.PENALTY_AREA_LENGTH,
                         ServerParam.PENALTY_AREA_WIDTH));
        Rectangle p_area = p_r;

        public void placePlayersForGoalkick()
        {


            string oppside = "r";

            if (M_stadium.Playmode == PlayMode.GoalKick_Left)
            {
                oppside = "r";
                p_area = p_l;
            }
            else
            {
                oppside = "l";
                p_area = p_r;
            }

            foreach (var p in Stadium.Players)
            {
                if (!(p).Enable) continue;

                if ((p).Side == oppside)
                {
                    float size = (p).Size;
                    Rectangle expand_area = new Rectangle(p_area.left - size,
                                       p_area.right + size,
                                       p_area.top - size,
                                       p_area.bottom + size);

                    if (expand_area.ContainsPoint((p).Position))
                    {
                        Vector new_pos = expand_area.NearestEdgeToPoint((p).Position);
                        if (new_pos.X * (oppside == "l" ? 1 : -1) >= ServerParam.PITCH_LENGTH / 2)
                        {
                            new_pos.X
                                = (ServerParam.PITCH_LENGTH / 2
                                    - ServerParam.PENALTY_AREA_LENGTH
                                    - size)
                                * (oppside == "l" ? 1 : -1);
                        }

                        (p).moveTo(new_pos, new Vector(0, 0), new Vector(0, 0));
                    }
                }
            }
        }

        public override void doPlayModeChange(PlayMode pm)
        {
            if (pm != PlayMode.PlayOn)
            {
                M_kick_taken = false;
            }

            if (pm == PlayMode.KickOff_Left
                 || pm == PlayMode.KickIn_Left
                 || pm == PlayMode.FreeKick_Left
                 || pm == PlayMode.IndFreeKick_Left
                 || pm == PlayMode.CornerKick_Left
                 || pm == PlayMode.GoalKick_Left)
            {
                clearPlayersFromBall("r");
            }
            else if (pm == PlayMode.KickOff_Right
                      || pm == PlayMode.KickIn_Right
                      || pm == PlayMode.FreeKick_Right
                      || pm == PlayMode.IndFreeKick_Right
                      || pm == PlayMode.CornerKick_Right
                      || pm == PlayMode.GoalKick_Right)
            {
                clearPlayersFromBall("l");
            }
            else if (pm == PlayMode.Drop_Ball)
            {
                clearPlayersFromBall("n");
            }

            if (goalKick(pm))
            {
                M_timer = 50;

                if (!goalKick(M_stadium.Playmode))
                {
                    M_goal_kick_count = 0;
                }
                else
                {
                    M_goal_kick_count++;
                }
            }
            else
            {
                M_goal_kick_count = 0;
            }

            if (freeKick(pm))
            {
                M_timer = 50;
            }

            if (!freeKick(pm) && !goalKick(pm))
            {
                M_timer = -1;
            }

            if (pm == PlayMode.Free_Kick_Fault_Left
                 || pm == PlayMode.Free_Kick_Fault_Right)
            {
                M_stadium.BallCatcher = null;
                M_after_free_kick_fault_time = 0;
            }
        }
    }

    public class TouchRef : Referee
    {

        static int AFTER_GOAL_WAIT = 50;
        Vector M_prev_ball_pos = new Vector(0, 0);
        private int M_after_goal_time = 0;
        Player M_last_touched = null;
        private bool M_indirect_mode = false;
        private Player M_last_indirect_kicker = null;

        public TouchRef(Stadium std)
            : base(std)
        {

        }

        public override void doAnalyse()
        {
            analyseImpl();

            M_prev_ball_pos.assign(M_stadium.Ball.Position);
        }

        public void analyseImpl()
        {
            if (isPenaltyShootOut(M_stadium.Playmode))
            {
                return;
            }

            if (M_stadium.Playmode == PlayMode.AfterGoal_Left)
            {
                if (++M_after_goal_time > AFTER_GOAL_WAIT)
                {
                    M_stadium.placeBall(PlayMode.KickOff_Right, "r", new Vector(0.0f, 0.0f));
                    placePlayersInTheirField();
                }
                return;
            }

            if (M_stadium.Playmode == PlayMode.AfterGoal_Right)
            {
                if (++M_after_goal_time > AFTER_GOAL_WAIT)
                {
                    M_stadium.placeBall(PlayMode.KickOff_Left, "l", new Vector(0.0f, 0.0f));
                    placePlayersInTheirField();
                }
                return;
            }

            if (checkGoal())
            {
                return;
            }

            if (M_stadium.Playmode != PlayMode.AfterGoal_Left
                 && M_stadium.Playmode != PlayMode.AfterGoal_Right
                 && M_stadium.Playmode != PlayMode.TimeOver)
            {
                if (Math.Abs(M_stadium.Ball.Position.X)
                     > ServerParam.PITCH_LENGTH * 0.5
                     + ServerParam.Instance.Ball_size)
                {
                    // check for goal kick or corner kick
                    string side = "n";
                    if (M_last_touched != null)
                    {
                        side = M_last_touched.Side;
                    }

                    if (M_stadium.Ball.Position.X
                         > ServerParam.PITCH_LENGTH * 0.5
                         + ServerParam.Instance.Ball_size)
                    {
                        if (side == "r")
                        {
                            awardCornerKick("l", M_stadium.Ball.Position);
                        }
                        else if (M_stadium.BallCatcher == null)
                        {
                            awardGoalKick("r", M_stadium.Ball.Position);
                        }
                        else
                        {
                            // the ball is caught and the goalie must have
                            // moved to a position beyond the opponents goal
                            // line.  Let the catch ref clean up the mess
                            return;
                        }
                    }
                    else if (M_stadium.Ball.Position.X
                              < ServerParam.PITCH_LENGTH * 0.5
                              - ServerParam.Instance.Ball_size)
                    {
                        if (side == "l")
                        {
                            awardCornerKick("r", M_stadium.Ball.Position);
                        }
                        else if (M_stadium.BallCatcher == null)
                        {
                            awardGoalKick("l", M_stadium.Ball.Position);
                        }
                        else
                        {
                            // the ball is caught and the goalie must have
                            // moved to a position beyond the opponents goal
                            // line.  Let the catch ref clean up the mess
                            return;
                        }
                    }
                }
                else if (Math.Abs(M_stadium.Ball.Position.Y)
                          > ServerParam.PITCH_WIDTH * 0.5
                          + ServerParam.Instance.Ball_size)
                {
                    // check for kick in.

                    string side = "n";
                    if (M_last_touched != null)
                    {
                        side = M_last_touched.Side;
                    }

                    if (side == "n")
                    {
                        // somethings gone wrong but give a drop ball
                        awardDropBall(M_stadium.Ball.Position);
                    }
                    else
                    {
                        awardKickIn((side == "r" ? "l" : "r"), M_stadium.Ball.Position);
                    }
                }
            }
        }

        public bool checkGoal()
        {
            if (M_stadium.Playmode == PlayMode.AfterGoal_Left
         || M_stadium.Playmode == PlayMode.AfterGoal_Right
         || M_stadium.Playmode == PlayMode.TimeOver)
            {
                return false;
            }

            if (M_indirect_mode)
            {
                return false;
            }

            ServerParam param = ServerParam.Instance;

            // FIFA rules:  Ball has to be completely outside of the pitch to be considered out
            //    static RArea pt( PVector(0.0,0.0),
            //                       PVector( ServerParam.PITCH_LENGTH
            //                                + ServerParam.Instance.ballSize() * 2,
            //                                ServerParam.PITCH_WIDTH
            //                                + ServerParam.Instance.ballSize() * 2 ) );

            if (Math.Abs(M_stadium.Ball.Position.X)
                 <= ServerParam.PITCH_LENGTH * 0.5 + param.Ball_size)
            {
                return false;
            }


            if ((M_stadium.BallCatcher == null
                   || M_stadium.BallCatcher.Side == "l")
                 && crossGoalLine("l", M_prev_ball_pos)
                 && !isPenaltyShootOut(M_stadium.Playmode))
            {
                M_stadium.score("r");
                announceGoal("r");
                M_after_goal_time = 0;
                M_stadium.placeBall(PlayMode.AfterGoal_Right, "l", M_stadium.Ball.Position);

                M_stadium.Playmode = (PlayMode.AfterGoal_Right);

                return true;
            }
            else if ((M_stadium.BallCatcher == null
                        || M_stadium.BallCatcher.Side == "r")
                      && crossGoalLine("r", M_prev_ball_pos)
                      && !isPenaltyShootOut(M_stadium.Playmode))
            {
                M_stadium.score("l");
                announceGoal("l");
                M_after_goal_time = 0;
                M_stadium.placeBall(PlayMode.AfterGoal_Left, "r", M_stadium.Ball.Position);


                M_stadium.Playmode = (PlayMode.AfterGoal_Left);

                return true;
            }

            return false;
        }

        public void announceGoal(string side)
        {
            M_stadium.sendRefereeAudio("goal_" + side + "_" + (side == "l" ? M_stadium.TeamLeftPoint : M_stadium.TeamRightPoint));
        }

        public override void doKickTaken(Player kicker)
        {
            if (Math.Abs(M_stadium.Ball.Position.X)
     <= ServerParam.PITCH_LENGTH * 0.5f + ServerParam.Instance.Ball_size)
            {
                if (M_stadium.Playmode == PlayMode.PlayOn
                     && M_last_indirect_kicker != null
                     && !kicker.Equals(M_last_indirect_kicker))
                {
                    M_last_indirect_kicker = null;
                    M_indirect_mode = false;
                }

                if (M_indirect_mode)
                {
                    M_last_indirect_kicker = kicker;
                }

                M_last_touched = kicker;
            }
        }

        public override void doBallTouched(Player kicker)
        {
            if (Math.Abs(M_stadium.Ball.Position.X)
     <= ServerParam.PITCH_LENGTH * 0.5f + ServerParam.Instance.Ball_size)
            {
                if (M_stadium.Playmode == PlayMode.PlayOn
                     && M_last_indirect_kicker != null
                     && M_last_indirect_kicker.Equals(kicker))
                {
                    M_last_indirect_kicker = null;
                    M_indirect_mode = false;
                }

                if (M_indirect_mode)
                {
                    M_last_indirect_kicker = kicker;
                }

                M_last_touched = kicker;
            }
        }

        public override void doPlayModeChange(PlayMode pm)
        {
            if (pm != PlayMode.PlayOn)
            {
                M_last_touched = null;
            }

            if (indirectFreeKick(pm))
            {
                M_last_indirect_kicker = null;
                M_indirect_mode = true;
            }
            else if (pm != PlayMode.PlayOn && pm != PlayMode.Drop_Ball)
            {
                M_last_indirect_kicker = null;
                M_indirect_mode = false;
            }
        }

        public bool indirectFreeKick(PlayMode pm)
        {
            switch (pm)
            {
                case PlayMode.IndFreeKick_Right:
                case PlayMode.IndFreeKick_Left:
                    return true;
                default:
                    return false;
            }
        }


    }

    public class CatchRef : Referee
    {

        static int AFTER_BACKPASS_WAIT = 30;
        static int AFTER_CATCH_FAULT_WAIT = 30;
        private bool M_team_l_touched = false;
        private bool M_team_r_touched = false;
        private Player M_last_back_passer = null;
        private int M_last_back_passer_time = 0;
        private int M_after_back_pass_time = 0;
        private int M_after_catch_fault_time = 0;

        public CatchRef(Stadium std)
            : base(std)
        {

        }


        public override void doKickTaken(Player kicker)
        {
            {
                //     if ( ! kicker.isGoalie() )
                {
                    if (kicker.Side == "l")
                    {
                        M_team_l_touched = true;
                    }
                    else if (kicker.Side == "r")
                    {
                        M_team_r_touched = true;
                    }

                    if (M_team_l_touched && M_team_r_touched)
                    {
                        M_last_back_passer = null;
                    }
                    else if (kicker.IsGoalie)
                    {
                        if (M_last_back_passer != null
                             && M_last_back_passer.Side != kicker.Side)
                        {

                        }
                        else
                        {
                            M_last_back_passer = kicker;
                            M_last_back_passer_time = M_stadium.Time;
                        }
                    }
                    else if (!M_last_back_passer.Equals(kicker))
                    {
                        M_last_back_passer = kicker;
                        if (M_last_back_passer == null
                             || M_last_back_passer.Side != kicker.Side)
                        {
                            M_last_back_passer_time = M_stadium.Time;
                        }
                    }
                }
                //     else if ( M_last_back_passer != NULL
                //               && M_last_back_passer.team() != kicker.team() )
                //     {
                //         M_last_back_passer = NULL;
                //         // The else if above is to handle rare situations where a player from team
                //         // A kicks the ball, the goalie from team B kicks it, and then the goalie
                //         // from team A cacthes it.  This should not be concidered a back pass and
                //         // the else if make sure of that.
                //     }
            }
        }

        public override void doBallTouched(Player player)
        {
            if (player.Side == "l")
            {
                M_team_l_touched = true;
            }
            else if (player.Side == "r")
            {
                M_team_r_touched = true;
            }

            if (M_team_l_touched && M_team_r_touched)
            {
                M_last_back_passer = null;
            }
        }

        public override void doCaughtBall(Player catcher)
        {
            if (isPenaltyShootOut(M_stadium.Playmode))
            {
                return;
            }

            // check handling violation
            if (M_stadium.Playmode != PlayMode.AfterGoal_Left
                 && M_stadium.Playmode != PlayMode.AfterGoal_Right
                 && M_stadium.Playmode != PlayMode.TimeOver
                 && !inPenaltyArea(catcher.Side, M_stadium.Ball.Position))
            {
                callCatchFault(catcher.Side, M_stadium.Ball.Position);
                return;
            }

            // check backpass violation
            if (M_stadium.Playmode != PlayMode.AfterGoal_Left
                 && M_stadium.Playmode != PlayMode.AfterGoal_Right
                 && M_stadium.Playmode != PlayMode.TimeOver
                 && M_stadium.Time != M_last_back_passer_time
                 && M_last_back_passer != null
                //&& M_last_back_passer != &catcher
                 && M_last_back_passer.Team == catcher.Team
                 && ServerParam.Instance.Back_Passes)
            {
                //M_last_back_passer.alive |= BACK_PASS;
                M_stadium.setPlayerState(M_last_back_passer.Team,
                                          M_last_back_passer.Unum,
                                          (int)PlayerState.BACK_PASS);
                callBackPass(catcher.Side);

                return;
            }

            M_last_back_passer = null;

            awardFreeKick(catcher.Side, M_stadium.Ball.Position);
        }

        public void callBackPass(string side)
        {
            Vector pos = new Vector(0, 0);
            pos.assign(truncateToPitch(M_stadium.Ball.Position));
            //pos = moveOutOfPenalty( side, pos );
            pos = moveOutOfGoalArea(side, pos);

            M_stadium.BallCatcher = null;

            if (side == "l")
            {
                M_stadium.placeBall(PlayMode.Back_Pass_Left, "r", pos);
            }
            else
            {
                M_stadium.placeBall(PlayMode.Back_Pass_Right, "l", pos);
            }

            M_last_back_passer = null;
            M_after_back_pass_time = 0;
        }

        public void callCatchFault(string side, Vector pos)
        {
            Vector pos1 = new Vector(0, 0);
            pos1.assign(pos);
            pos1 = truncateToPitch(pos1);
            //pos = moveIntoPenalty( side, pos );
            pos1 = moveOutOfPenalty(side, pos1);

            M_stadium.BallCatcher = null;

            if (side == "l")
            {
                M_stadium.placeBall(PlayMode.CatchFault_Left, "r", pos1);
            }
            else if (side == "r")
            {
                M_stadium.placeBall(PlayMode.CatchFault_Right, "l", pos1);
            }

            M_after_catch_fault_time = 0;
        }

        public override void doAnalyse()
        {
            PlayMode pm = M_stadium.Playmode;

            M_team_l_touched = false;
            M_team_r_touched = false;

            if (isPenaltyShootOut(pm))
            {
                return;
            }

            if (pm == PlayMode.Back_Pass_Left)
            {
                clearPlayersFromBall("l");
                if (++M_after_back_pass_time > AFTER_BACKPASS_WAIT)
                {
                    //M_stadium.changePlayMode( PM_FreeKick_Right );
                    M_stadium.Playmode = (PlayMode.IndFreeKick_Right);
                }
                return;
            }

            if (pm == PlayMode.Back_Pass_Right)
            {
                clearPlayersFromBall("r");
                if (++M_after_back_pass_time > AFTER_BACKPASS_WAIT)
                {
                    //M_stadium.changePlayMode( PM_FreeKick_Left );
                    M_stadium.Playmode = (PlayMode.IndFreeKick_Left);
                }
                return;
            }

            if (pm == PlayMode.CatchFault_Left)
            {
                clearPlayersFromBall("l");
                if (++M_after_catch_fault_time > AFTER_CATCH_FAULT_WAIT)
                {
                    //M_stadium.Playmode = ( PlayMode.IndFreeKick_Right );
                    M_stadium.Playmode = (PlayMode.FreeKick_Right);
                }
                return;
            }

            if (pm == PlayMode.CatchFault_Right)
            {
                clearPlayersFromBall("r");
                if (++M_after_catch_fault_time > AFTER_CATCH_FAULT_WAIT)
                {
                    //M_stadium.Playmode = ( PlayMode.IndFreeKick_Left );
                    M_stadium.Playmode = (PlayMode.FreeKick_Left);
                }
                return;
            }


            if (M_stadium.BallCatcher != null
                 && pm != PlayMode.AfterGoal_Left
                 && pm != PlayMode.AfterGoal_Right
                 && pm != PlayMode.TimeOver
                 && !inPenaltyArea(M_stadium.BallCatcher.Side,
                                     M_stadium.Ball.Position))
            {
                callCatchFault(M_stadium.BallCatcher.Side,
                                M_stadium.Ball.Position);
            }
        }

        public override void doPlayModeChange(PlayMode pm)
        {
            if (pm != PlayMode.PlayOn)
            {
                M_last_back_passer = null;
            }

            if (pm == PlayMode.Back_Pass_Left
                 || pm == PlayMode.Back_Pass_Left)
            {
                M_stadium.BallCatcher = null;
                M_last_back_passer = null;
                M_after_back_pass_time = 0;
            }
            else if (pm == PlayMode.CatchFault_Left
                      || pm == PlayMode.CatchFault_Left)
            {
                M_stadium.BallCatcher = null;
                M_after_catch_fault_time = 0;
            }
        }
    }

    public class FoulRef : Referee
    {
        static int AFTER_FOUL_WAIT = 30;
        int M_after_foul_time = 0;

        public FoulRef(Stadium std)
            : base(std)
        {

        }

        public override void doPlayModeChange(PlayMode pm)
        {
            if (pm == PlayMode.Foul_Charge_Left
     || pm == PlayMode.Foul_Charge_Right
     || pm == PlayMode.Foul_Push_Left
     || pm == PlayMode.Foul_Push_Right)
            {
                M_after_foul_time = 0;
            }
        }

        public void callYellowCard(Player tackler)
        {
            callFoul(tackler);

            M_stadium.yellowCard(tackler.Team,
                                  tackler.Unum);
        }

        public void callFoul(Player tackler)
        {
            Vector pos = new Vector(0, 0);
            pos.assign(tackler.Position);
            pos = truncateToPitch(pos);
            pos = moveOutOfGoalArea("n", pos);

            M_stadium.BallCatcher = null;

            if (tackler.Side == "l")
            {
                pos = moveOutOfPenalty("r", pos);
                //M_stadium.placeBall( intentional ? PM_Foul_Push_Left : PM_Foul_Charge_Left,
                M_stadium.placeBall(PlayMode.Foul_Charge_Left,
                                     "r", pos);
            }
            else if (tackler.Side == "r")
            {
                pos = moveOutOfPenalty("l", pos);
                M_stadium.placeBall(PlayMode.Foul_Charge_Right,
                                     "l", pos);
            }

            tackler.FoulCount++;

            M_after_foul_time = 0;
        }

        public override void doAnalyse()
        {
            PlayMode pm = M_stadium.Playmode;

            if (isPenaltyShootOut(pm))
            {
                return;
            }

            if (pm == PlayMode.Foul_Charge_Left
                 || pm == PlayMode.Foul_Push_Left)
            {
                clearPlayersFromBall("l");
                if (++M_after_foul_time > AFTER_FOUL_WAIT)
                {
                    if (Referee.inPenaltyArea("l", M_stadium.Ball.Position))
                    {
                        M_stadium.Playmode = (PlayMode.IndFreeKick_Right);
                    }
                    else
                    {
                        M_stadium.Playmode = (PlayMode.FreeKick_Right);
                    }
                }
                return;
            }

            if (pm == PlayMode.Foul_Charge_Right
                 || pm == PlayMode.Foul_Push_Right)
            {
                clearPlayersFromBall("r");
                if (++M_after_foul_time > AFTER_FOUL_WAIT)
                {
                    if (Referee.inPenaltyArea("r", M_stadium.Ball.Position))
                    {
                        M_stadium.Playmode = (PlayMode.IndFreeKick_Left);
                    }
                    else
                    {
                        M_stadium.Playmode = (PlayMode.FreeKick_Left);
                    }
                }
                return;
            }
        }

        public override void doTackleTaken(Player tackler, bool foul)
        {
            if (isPenaltyShootOut(M_stadium.Playmode))
            {
                return;
            }

            bool detect_charge = false;
            bool detect_yellow = false;

            double ball_dist2 = tackler.Position.distanceSquared(M_stadium.Ball.Position);
            double ball_angle = (M_stadium.Ball.Position - tackler.Position).Theta;

            //std::cerr << M_stadium.time() << " (tackleTaken) "
            //          << " (tackler " << SideStr( tackler.side() ) << ' ' << tackler.unum() << ")"
            //          << std::endl;

            foreach (var p in Stadium.Players)
            {
                if (!p.Enable) continue;
                if (p.Side == tackler.Side) continue;

                // if ( ! p.ballKickable() ) continue; // no kickable

                bool pre_check = false;




                Vector player_rel = (p).Position - tackler.Position;

                if (player_rel.RadiusSquared > ball_dist2)
                {
                    //std::cerr << "---. " << (p).unum() << " ball near." << std::endl;
                    continue; // further than ball
                }

                //std::cerr << "-. (player " << SideStr( (p).side() ) << ' ' << (p).unum() << ")\n";

                player_rel.rotate(-(float)ball_angle);

                if (player_rel.X < 0.0
                     || Math.Abs(player_rel.Y) > (p).Size + tackler.Size)
                {
                    //std::cerr << "---. " << (p).unum() << " behind or big y_diff. rel=" << player_rel
                    //          << std::endl;
                    continue;
                }

                double body_diff = Math.Abs(Utilities.Normalize_angle(p.AngleBodyCommitted - ball_angle));
                if (body_diff > Math.PI * 0.5)
                {
                    //std::cerr << "---. " << (p).unum() << " over body angle. angle=" << body_diff / M_PI * 180.0
                    //          << std::endl;
                    continue;
                }

                if (foul)
                {
                    if (pre_check)
                    {
                        //std::cerr << "---. " << (p).unum() << " detected yellow_card." << std::endl;
                        detect_yellow = true;
                    }
                }
                else
                {
                    if (true)
                    {
                        //std::cerr << "---. " << (p).unum() << " detected foul. prob=" << rng.p() << std::endl;
                        detect_charge = true;
                    }
                }

                //std::cerr << "---. not detected foul. prob=" << rng.p()<< std::endl;
            }

            if (detect_yellow)
            {
                callYellowCard(tackler);
            }
            else if (detect_charge)
            {
                callFoul(tackler);
            }
        }

    }

    public class PenaltyRef : Referee
    {

        Vector M_prev_ball_pos = new Vector(0, 0);
        string M_pen_side = "l";
        Player M_last_taker = null;
        List<int> M_sLeftPenTaken = new List<int>();
        List<int> M_sRightPenTaken = new List<int>();

        bool first_time = true;
        string M_cur_pen_taker = "r";
        private bool M_timeover = false;
        private int M_timer = 0;
        private int M_pen_nr_taken;

        public PenaltyRef(Stadium std)
            : base(std)
        {

        }
        public override void doCaughtBall(Player catcher)
        {
            if (!isPenaltyShootOut(M_stadium.Playmode))
            {
                return;
            }


            if (M_stadium.Playmode == PlayMode.PenaltyTaken_Left
                 || M_stadium.Playmode == PlayMode.PenaltyTaken_Right)
            {
                if (catcher.Side == M_cur_pen_taker)
                {
                    // taker team's goalie catches the ball
                    penalty_foul(M_cur_pen_taker == "l" ? "r" : "l");
                }
                else if (!inPenaltyArea(M_pen_side, M_stadium.Ball.Position))
                {

                    penalty_foul(M_cur_pen_taker == "l" ? "r" : "l");
                }
                else
                {
                    // legal catch
                    penalty_miss(M_cur_pen_taker);
                }
            }
            else if (M_stadium.Playmode == PlayMode.PenaltyReady_Left
                      || M_stadium.Playmode == PlayMode.PenaltyReady_Right)
            {

                penalty_foul(catcher.Side);
            }

            // freeze the ball
            //     M_stadium.Ball.moveTo( M_stadium.Ball.Position,
            //                             //0.0,
            //                             PVector( 0.0, 0.0 ),
            //                             PVector( 0.0, 0.0 ) );
            M_stadium.placeBall(Stadium.Playmode, "n", M_stadium.Ball.Position);

        }

        public void penalty_foul(string side)
        {

            M_stadium.sendRefereeAudio((side == "l"
                                          ? "penalty_foul_l"
                                          : "penalty_foul_r"));

            // if team takes penalty and makes mistake . miss, otherwise . score
            if (side == "l" && M_cur_pen_taker == "l")
            {
                penalty_miss("l");
            }
            else if (side == "r" && M_cur_pen_taker == "r")
            {
                penalty_miss("r");
            }
            else if (side == "l")
            {
                penalty_score("r");
            }
            else
            {
                penalty_score("l");
            }

        }

        public void penalty_check_score()
        {




            // if both players have taken more than nr_kicks penalties . check for winner
            if (M_pen_nr_taken > 2 * ServerParam.Instance.Pen_Nr_Kicks)
            {
                if (M_pen_nr_taken % 2 == 0
                    && (M_stadium.PenaltyScoredLeft
                         != M_stadium.PenaltyScoredRight))
                {

                    if (M_stadium.PenaltyScoredLeft
                         > M_stadium.PenaltyScoredRight)
                    {
                        M_stadium.sendRefereeAudio("penalty_winner_l");
                    }
                    else
                    {
                        M_stadium.sendRefereeAudio("penalty_winner_r");
                    }
                    //M_stadium.changePlayMode( PM_TimeOver );
                    M_timeover = true;
                }
            }
            // if both players have taken nr_kicks and max_extra_kicks penalties . quit
            else if (M_pen_nr_taken > 2 * (ServerParam.Instance.Pen_Max_Extra_Kicks
                                             + ServerParam.Instance.Pen_Nr_Kicks)
                      )
            {

                if (ServerParam.Instance.Pen_Random_Winner)
                {
                    if (Utilities.drand(0, 1) < 0.5)
                    {
                        M_stadium.sendRefereeAudio("penalty_winner_l");
                        M_stadium.PenaltyWinner = ("l");

                    }
                    else
                    {
                        M_stadium.sendRefereeAudio("penalty_winner_r");
                        M_stadium.PenaltyWinner = ("r");

                    }
                }
                else
                {
                    M_stadium.sendRefereeAudio("penalty_draw");
                }
                //M_stadium.changePlayMode( PM_TimeOver );
                M_timeover = true;
            }
            // during normal kicks, check whether one team cannot win anymore
            else
            {
                // first calculate how many penalty kick sessions are left
                // and add this to the current number of points of both teams
                // finally, subtract 1 point from the team that has already shot this turn
                int iPenLeft = ServerParam.Instance.Pen_Nr_Kicks - M_pen_nr_taken / 2;
                int iMaxExtraLeft = M_stadium.PenaltyScoredLeft + iPenLeft;
                int iMaxExtraRight = M_stadium.PenaltyScoredRight + iPenLeft;
                if (M_pen_nr_taken % 2 == 1)
                {
                    if (M_cur_pen_taker == "l")
                    {
                        iMaxExtraLeft--;
                    }
                    else if (M_cur_pen_taker == "r")
                    {
                        iMaxExtraRight--;
                    }
                }

                if (iMaxExtraLeft < M_stadium.PenaltyScoredRight)
                {

                    M_stadium.sendRefereeAudio("penalty_winner_r");
                    M_stadium.Playmode = (PlayMode.TimeOver);
                }
                else if (iMaxExtraRight < M_stadium.TeamLeftPoint)
                {

                    M_stadium.sendRefereeAudio("penalty_winner_l");
                    //M_stadium.changePlayMode( PM_TimeOver );
                    M_timeover = true;
                }
            }
        }

        public void penalty_miss(string side)
        {
            M_stadium.Playmode = (side == "l"
                                      ? PlayMode.PenaltyMiss_Left
                                      : PlayMode.PenaltyMiss_Right);
            M_pen_nr_taken++;

            if (side == "r")
            {
                M_stadium.penaltyScore("r", false);
            }
            else
            {
                M_stadium.penaltyScore("l", false);
            }

            penalty_check_score();
        }

        public override void doKickTaken(Player kicker)
        {
            if (!isPenaltyShootOut(M_stadium.Playmode))
            {
                return;
            }


    // if in setup it is not allowed to kick the ball
            else if (M_stadium.Playmode == PlayMode.PenaltySetup_Left
                      || M_stadium.Playmode == PlayMode.PenaltySetup_Right)
            {
                penalty_foul(kicker.Side);
            }
            // cannot kick second time after penalty was taken
            else if (ServerParam.Instance.Pen_Allow_Mult_Kicks == false
                      && (M_stadium.Playmode == PlayMode.PenaltyTaken_Left
                           || M_stadium.Playmode == PlayMode.PenaltyTaken_Right)
                      && kicker.Side == M_cur_pen_taker)
            {
                penalty_foul(M_cur_pen_taker);
            }
            else if (M_stadium.Playmode == PlayMode.PenaltyReady_Left
                      || M_stadium.Playmode == PlayMode.PenaltyTaken_Left
                      || M_stadium.Playmode == PlayMode.PenaltyReady_Right
                      || M_stadium.Playmode == PlayMode.PenaltyTaken_Right)
            {
                if ((M_stadium.Playmode == PlayMode.PenaltyReady_Left
                       || M_stadium.Playmode == PlayMode.PenaltyReady_Right)
                     && kicker.Side == M_cur_pen_taker
                     && (("l" == M_cur_pen_taker
                            && M_sLeftPenTaken.Exists(k => k == kicker.Unum))
                          || ("r" == M_cur_pen_taker
                               && M_sRightPenTaken.Exists(k => k == kicker.Unum))
                          )
                     )
                {
                    // this kicker has already taken the kick
                    penalty_foul(M_cur_pen_taker);
                }
                else if (M_last_taker != null
                          && M_last_taker.Side == M_cur_pen_taker
                          && !M_last_taker.Equals(kicker))
                {
                    // not a taker player in the same team must not kick the ball.
                    penalty_foul(M_cur_pen_taker);
                }
                else if (kicker.Side != M_cur_pen_taker
                          && !kicker.IsGoalie)
                {
                    // field player in the defending team must not kick the ball.
                    penalty_foul(M_cur_pen_taker == "l" ? "r" : "l");
                }
                else
                {
                    M_last_taker = kicker;
                }
            }

            // if we were ready for penalty . change play mode
            if (M_stadium.Playmode == PlayMode.PenaltyReady_Left)
            {
                // when penalty is taken, add player, multiple copies are deleted
                M_sLeftPenTaken.Add(kicker.Unum);
                if (M_sLeftPenTaken.Count == Stadium.Players.FindAll(p => p.Side == "l").Count)
                {
                    M_sLeftPenTaken.Clear();
                }
                M_stadium.Playmode = (PlayMode.PenaltyTaken_Left);
            }
            else if (M_stadium.Playmode == PlayMode.PenaltyReady_Right)
            {
                M_sRightPenTaken.Add(kicker.Unum);
                if (M_sRightPenTaken.Count == Stadium.Players.FindAll(p => p.Side == "r").Count)
                {
                    M_sRightPenTaken.Clear();
                }
                M_stadium.Playmode = (PlayMode.PenaltyTaken_Right);
            }
            // if it was not allowed to kick, don't move ball
            else if (M_stadium.Playmode != PlayMode.PenaltyTaken_Left
                      && M_stadium.Playmode != PlayMode.PenaltyTaken_Right)
            {
                M_stadium.placeBall(Stadium.Playmode, M_pen_side, M_stadium.Ball.Position);
            }
        }


        public void penalty_score(string side)
        {
            M_stadium.Playmode = (side == "r"
                                      ? PlayMode.PenaltyScore_Right
                                      : PlayMode.PenaltyScore_Left);

            if (side == "r")
            {
                M_stadium.penaltyScore("r", true);
            }
            else
            {
                M_stadium.penaltyScore("l", true);
            }
            M_pen_nr_taken++;
            penalty_check_score();
        }

        public override void doAnalyse()
        {
            startPenaltyShootout();
            analyseImpl();

            M_prev_ball_pos.assign(Stadium.Ball.Position);
        }

        public void startPenaltyShootout()
        {

            ServerParam param = ServerParam.Instance;

            // if normal and extra time are over . start the penalty procedure or quit
            if (first_time
                 && param.Penalty_Shoot_Outs
                 && M_stadium.Playmode != PlayMode.BeforeKickOff
                 && M_stadium.TeamLeftPoint == M_stadium.TeamRightPoint
                 && ((param.Half_Time < 0
                        && param.Nr_normal_halfs + param.Nr_extra_halfs == 0)
                      || (param.Half_Time >= 0
                           && (M_stadium.Time >=
                                (param.Half_Time * param.Nr_normal_halfs
                                  + param.Extra_Half_time * param.Nr_extra_halfs)))
                      )
                 )
            {
                if (Utilities.drand(0, 1) < 0.5)       // choose random side of the playfield
                {
                    M_pen_side = "r";            // and inform players
                }

                M_stadium.BallCatcher = null;
                M_stadium.sendRefereeAudio(M_pen_side == "l"
                                            ? "penalty_onfield_l"
                                            : "penalty_onfield_r");
                // choose at random who starts (note that in penalty_init, actually the
                // opposite player is chosen since there the playMode changes)
                M_cur_pen_taker = (Utilities.drand(0, 1) < 0.5) ? "l" : "r";

                // place the goalkeeper of the opposite field close to the penalty goal
                // otherwise it is hard to get there before pen_setup_wait cycles
                string side = (M_pen_side == "l") ? "r" : "l";
                foreach (Player p in M_stadium.Players)
                {
                    if (p.Enable
                         && p.Side == side
                         && p.IsGoalie)
                    {
                        (p).moveTo(new Vector((M_pen_side == "l" ? -1 : 1)
                                               * (ServerParam.PITCH_LENGTH / 2 - 10),
                                               10), new Vector(0, 0), new Vector(0, 0));
                    }
                }

                penalty_init();
                first_time = false;
            }
        }

        public void penalty_init()
        {
            // change the play mode such that the other side can take the penalty
            // and place the ball at the penalty spot
            M_cur_pen_taker = (M_cur_pen_taker == "l"
                                ? "r"
                                : "l");
            PlayMode pm = (M_cur_pen_taker == "l"
                            ? PlayMode.PenaltySetup_Left
                            : PlayMode.PenaltySetup_Right);
            M_stadium.placeBall(pm,
                                 "n",
                                 new Vector((M_pen_side == "l" ? -1 : 1)
                                          * (ServerParam.PITCH_LENGTH / 2
                                              - ServerParam.Instance.Pen_Dist_X),
                                          0.0f));
        }

        public void placeTakerTeamPlayers()
        {
            bool bPenTaken = (M_stadium.Playmode == PlayMode.PenaltyTaken_Right
                                     || M_stadium.Playmode == PlayMode.PenaltyTaken_Left);

            Player taker = (M_last_taker != null
                                     ? M_last_taker
                                     : getCandidateTaker());

            Vector goalie_wait_pos_b = new Vector(-(M_pen_side == "l" ? 1 : -1) * (ServerParam.PITCH_LENGTH / 2 + 2.0f), +25.0f);
            Vector goalie_wait_pos_t = new Vector(-(M_pen_side == "l" ? 1 : -1) * (ServerParam.PITCH_LENGTH / 2 + 2.0f), -25.0f);

            // then replace the players from the specified side
            foreach (Player p in Stadium.Players)
            {
                if (!(p).Enable) continue;

                if ((p).Side != M_cur_pen_taker) continue;

                if ((p) == taker)
                {
                    if (!bPenTaken
                         && taker.Position.distance(M_stadium.Ball.Position) > 2.0)
                    {
                        //PVector new_pos( -M_pen_side * ( ServerParam::PITCH_LENGTH/2 - ServerParam::instance().pen_dist_x - 2.0 ),
                        //0.0 );
                        Circle c = new Circle(M_stadium.Ball.Position, 2.0f);
                        //(p).moveTo( new_pos );
                        (p).moveTo(c.closestPointOnCircleToPosition(taker.Position), new Vector(0, 0), new Vector(0, 0));
                    }
                }
                else
                {
                    if ((p).IsGoalie)
                    {
                        Circle c = new Circle(((p).Position.Y > 0.0 ? goalie_wait_pos_b : goalie_wait_pos_t),
                                 2.0f);
                        if (!c.containsPoint((p).Position))
                        {
                            (p).moveTo(c.closestPointOnCircleToPosition((p).Position), new Vector(0, 0), new Vector(0, 0));
                        }
                    }
                    else // not goalie
                    {
                        Circle center = new Circle(new Vector(0.0f, 0.0f),
                                      ServerParam.KICK_OFF_CLEAR_DISTANCE
                                      - (p).Size
                            //- ServerParam::instance().pspeed_max
                                      );
                        if (!center.containsPoint((p).Position))
                        {
                            //(p).moveTo( PVector::fromPolar( 6.5, Deg2Rad( i*15 ) ) );
                            (p).moveTo(center.closestPointOnCircleToPosition((p).Position), new Vector(0, 0), new Vector(0, 0));
                        }
                    }
                }
            }
        }

        public Player getCandidateTaker()
        {
            List<int> sPenTaken = (M_cur_pen_taker == "l"
                                                  ? M_sLeftPenTaken
                                                  : M_sRightPenTaken);

            Player candidate = null;
            Player goalie = null;
            double min_dist2 = double.PositiveInfinity;

            // first find the closest player to the ball
            foreach (Player p in Stadium.Players)
            {
                if (!(p).Enable) continue;

                if ((p).Side != M_cur_pen_taker) continue;

                if (sPenTaken.Exists(i => i == p.Unum))
                {
                    // players that have already taken a kick cannot be
                    // counted as a potential kicker.
                    continue;
                }

                if ((p).IsGoalie)
                {
                    goalie = (p);
                    continue;
                }

                double d2 = (p).Position.distanceSquared(M_stadium.Ball.Position);
                if (d2 < min_dist2)
                {
                    min_dist2 = d2;
                    candidate = (p);
                }
            }

            if (candidate == null)
            {
                return goalie;
            }

            return candidate;
        }


        public void penalty_place_all_players(string side)
        {
            if (side == M_cur_pen_taker)
            {
                placeTakerTeamPlayers();
            }
            else // other team
            {
                placeOtherTeamPlayers();
            }
        }

        public void analyseImpl()
        {
            string[] playmode_strings = Defines.PlaymodeStrings;

            if (!isPenaltyShootOut(M_stadium.Playmode))
            {
                return;
            }

            if (M_timeover)
            {
                M_stadium.Playmode = (PlayMode.TimeOver);
                return;
            }

            PlayMode pm = M_stadium.Playmode;

            bool bCheckLeft = penalty_check_players("l");
            bool bCheckRight = penalty_check_players("r");

            if (bCheckLeft)
            {
                penalty_place_all_players("l");
            }

            if (bCheckRight)
            {
                penalty_place_all_players("r");
            }

            bCheckRight = bCheckLeft = true;

            if (M_timer < 0)
            {
                // std.cerr << "(PenaltyRef.analyse) timer cannot be negative?" << std.endl;
            }
            else if (M_timer == 0)
            {
                handleTimeout(bCheckLeft, bCheckRight);
            }
            else // M_timer > 0
            {
                handleTimer(bCheckLeft, bCheckRight);
            }
        }

        public void placeOtherTeamPlayers()
        {
            bool bPenTaken = (M_stadium.Playmode == PlayMode.PenaltyTaken_Right
                                     || M_stadium.Playmode == PlayMode.PenaltyTaken_Left);
            double goalie_line
                = (M_pen_side == "l"
                    ? -ServerParam.PITCH_LENGTH / 2.0 + ServerParam.Instance.Pen_Max_Goalie_Dist_X
                    : +ServerParam.PITCH_LENGTH / 2.0 - ServerParam.Instance.Pen_Max_Goalie_Dist_X);


            foreach (Player p in Stadium.Players)
            {
                if (!(p).Enable) continue;

                if ((p).Side == M_cur_pen_taker) continue;

                // only move goalie in case the penalty has not been started yet.
                if ((p).IsGoalie)
                {
                    if (!bPenTaken)
                    {
                        if (M_pen_side == "l")
                        {
                            if ((p).Position.X - goalie_line > 0.0)
                            {
                                (p).moveTo(new Vector((float)goalie_line - 1.5f, 0.0f), new Vector(0, 0), new Vector(0, 0));
                            }
                        }
                        else
                        {
                            if ((p).Position.X - goalie_line < 0.0)
                            {
                                (p).moveTo(new Vector((float)goalie_line + 1.5f, 0.0f), new Vector(0, 0), new Vector(0, 0));
                            }
                        }
                    }
                }
                else // not goalie
                {
                    Circle center = new Circle(new Vector(0.0f, 0.0f),
                                  ServerParam.KICK_OFF_CLEAR_DISTANCE
                                  - (p).Size
                        //- ServerParam::instance().pspeed_max
                                  );
                    if (!center.containsPoint((p).Position))
                    {
                        // place other players in circle in penalty area
                        //(p).moveTo( PVector::fromPolar( 6.5, Deg2Rad( i*15 ) ) );
                        (p).moveTo(center.closestPointOnCircleToPosition((p).Position), new Vector(0, 0), new Vector(0, 0));
                    }
                }
            }
        }

        public bool penalty_check_players(string side)
        {
            PlayMode pm = M_stadium.Playmode;
            int iOutsideCircle = 0;
            bool bCheck = true;
            Vector posGoalie = new Vector(0, 0);
            //int     iPlayerOutside = -1, iGoalieNr=-1;
            Player outside_player = null;
            Player goalie = null;

            if (pm == PlayMode.PenaltyMiss_Left || pm == PlayMode.PenaltyMiss_Right
                 || pm == PlayMode.PenaltyScore_Left || pm == PlayMode.PenaltyScore_Right)
            {
                return true;
            }

            // for all players from side 'side' get the goalie pos and count how many
            // players are outside the center circle.
            foreach (Player p in M_stadium.Players)
            {
                if (!(p).Enable) continue;

                if ((p).Side == side)
                {
                    if ((p).IsGoalie)
                    {
                        goalie = p;
                        posGoalie.assign((p).Position);
                        continue;
                    }

                    Circle c = new Circle(new Vector(0.0f, 0.0f),
                             ServerParam.KICK_OFF_CLEAR_DISTANCE
                             - (p).Size);
                    if (!c.containsPoint((p).Position))
                    {
                        iOutsideCircle++;
                        outside_player = p;
                    }
                }
            }

            if (goalie == null)
            {
                return false;
            }

            // if the 'side' equals the one that takes the penalty shoot out
            if (side == M_cur_pen_taker)
            {
                // in case that goalie takes penalty kick
                // or taker goes into the center circle
                if (iOutsideCircle == 0)
                {
                    if (pm == PlayMode.PenaltySetup_Left || pm == PlayMode.PenaltySetup_Right)
                    {
                        if (goalie.Position.distance(M_stadium.Ball.Position) > 2.0)
                        {
                            // bCheck = false;
                        }
                        else
                        {
                            outside_player = goalie;
                        }
                    }
                }
                // if goalie not outside field, check fails
                else if (Math.Abs(posGoalie.X) < ServerParam.PITCH_LENGTH / 2.0 - 1.5
                          || Math.Abs(posGoalie.Y) < ServerParam.PENALTY_AREA_WIDTH / 2.0 - 1.5)
                {

                    bCheck = false;
                }
                // only one should be outside the circle . player that takes penalty
                else if (iOutsideCircle > 1)
                {

                    bCheck = false;
                }
                // in setup, player outside circle should be close to ball
                else if ((pm == PlayMode.PenaltySetup_Left || pm == PlayMode.PenaltySetup_Right)
                          && iOutsideCircle == 1)
                {
                    if (outside_player != null
                         && outside_player.Position.distance(M_stadium.Ball.Position) > 2.0)
                    {

                        bCheck = false;
                    }
                }
            }
            else //other team
            {
                // goalie does not stand in front of goal line
                if (M_stadium.Playmode != PlayMode.PenaltyTaken_Left
                     && M_stadium.Playmode != PlayMode.PenaltyTaken_Right)
                {
                    if (Math.Abs(posGoalie.X)
                         < ServerParam.PITCH_LENGTH / 2.0 - ServerParam.Instance.Pen_Max_Goalie_Dist_X
                         || Math.Abs(posGoalie.Y)
                         > ServerParam.Instance.Goal_width * 0.5)
                    {

                        bCheck = false;
                    }
                }
                // when receiving the penalty every player should be in center circle
                if (iOutsideCircle != 0)
                {

                    bCheck = false;
                }
            }

            if (bCheck
                 && outside_player != null)
            {
                // if in setup and already in set . check fails
                if ((side == "l"
                      && M_stadium.Playmode == PlayMode.PenaltySetup_Left
                      && M_sLeftPenTaken.Exists(p => p == outside_player.Unum))
                    || (side == "r"
                         && M_stadium.Playmode == PlayMode.PenaltySetup_Right
                         && M_sRightPenTaken.Exists(p => p == outside_player.Unum))
                    )
                {

                    bCheck = false;
                }
            }

            return bCheck;
        }

        public void handleTimeout(bool left_move_check,
                           bool right_move_check)
        {
            PlayMode pm = M_stadium.Playmode;



            if (pm == PlayMode.PenaltyMiss_Left
                 || pm == PlayMode.PenaltyScore_Left
                 || pm == PlayMode.PenaltyMiss_Right
                 || pm == PlayMode.PenaltyScore_Right)
            {
                penalty_init();
            }
            else if (left_move_check
                      && right_move_check)
            {
                if (pm == PlayMode.PenaltySetup_Left)
                {
                    M_stadium.Playmode = (PlayMode.PenaltyReady_Left);
                }
                else if (pm == PlayMode.PenaltySetup_Right)
                {
                    M_stadium.Playmode = (PlayMode.PenaltyReady_Right);
                }
                // time elapsed . missed goal
                else if (pm == PlayMode.PenaltyTaken_Left
                          || pm == PlayMode.PenaltyReady_Left)
                {
                    penalty_miss("l");
                }
                else if (pm == PlayMode.PenaltyTaken_Right
                          || pm == PlayMode.PenaltyReady_Right)
                {
                    penalty_miss("r");
                }
            }
            // if incorrect positioned , place them correctly
            else if (M_cur_pen_taker == "l")
            {
                penalty_foul((left_move_check == false) ? "l" : "r");
            }
            else if (M_cur_pen_taker == "r")
            {
                penalty_foul((right_move_check == false) ? "r" : "l");
            }
        }


        public void handleTimer(bool left_move_check,
                         bool right_move_check)
        {
            PlayMode pm = M_stadium.Playmode;

            --M_timer;

            if (pm == PlayMode.PenaltyScore_Left
                 || pm == PlayMode.PenaltyScore_Right
                 || pm == PlayMode.PenaltyMiss_Left
                 || pm == PlayMode.PenaltyMiss_Right)
            {
                // freeze the ball
                //         M_stadium.Ball.moveTo( M_stadium.Ball.Position,
                //                                 //0.0,
                //                                 PVector( 0.0, 0.0 ),
                //                                 PVector( 0.0, 0.0 ) );
                M_stadium.placeBall(pm, M_cur_pen_taker,
                                     M_stadium.Ball.Position);

                return;
            }

            if (left_move_check
                 && right_move_check)
            {
                // if ball crossed goalline, process goal and set ball on goalline
                if (crossGoalLine(M_pen_side, M_prev_ball_pos))
                {
                    if (pm == PlayMode.PenaltyTaken_Left)
                    {
                        penalty_score("l");
                    }
                    else if (pm == PlayMode.PenaltyTaken_Right)
                    {
                        penalty_score("r");
                    }
                    // freeze the ball at the current position.
                    M_stadium.placeBall(pm, M_pen_side, M_stadium.Ball.Position);
                }
                else if (Math.Abs(M_stadium.Ball.Position.X)
                          > ServerParam.PITCH_LENGTH * 0.5f
                          + ServerParam.Instance.Ball_size
                          || Math.Abs(M_stadium.Ball.Position.Y)
                          > ServerParam.PITCH_WIDTH * 0.5f
                          + ServerParam.Instance.Ball_size)
                {


                    M_stadium.placeBall(pm, M_pen_side, M_stadium.Ball.Position);
                    if (pm == PlayMode.PenaltyTaken_Left)
                    {
                        penalty_miss("l");
                    }
                    else if (pm == PlayMode.PenaltyTaken_Right)
                    {
                        penalty_miss("r");
                    }
                }
            }
            // if someone makes foul and we are not in setup . replace the players
            else if (pm == PlayMode.PenaltyReady_Left
                      || pm == PlayMode.PenaltyReady_Right
                      || pm == PlayMode.PenaltyTaken_Left
                      || pm == PlayMode.PenaltyTaken_Right)
            {
                if (M_cur_pen_taker == "l")
                {
                    penalty_foul((left_move_check == false) ? "l" : "r");
                }
                else if (M_cur_pen_taker == "r")
                {
                    penalty_foul((right_move_check == false) ? "r" : "l");
                }
            }
        }

    }

}
