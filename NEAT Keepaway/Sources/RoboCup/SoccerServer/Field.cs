// -*-C#-*-

/***************************************************************************
                                   Field.cs
                   Field class to represent the field the game is played on.
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

namespace RoboCup.Environment
{
    /// <summary>
    /// Represents the field of play
    /// </summary>
    internal class Field
    {

        #region Internal Structures/Classes

        /// <summary>
        /// Structure the represents a goal mouth.
        /// </summary>
        internal struct GoalMouth
        {
            char side;
            float startY;
            float endY;

            /// <summary>
            /// Constructor for GoalMouth struct
            /// </summary>
            /// <param name="goalSide">The side of the goal.</param>
            /// <param name="goalStartY">The starting Y position of the goal.</param>
            /// <param name="goalEndY">The ending Y position of the goal.</param>
            internal GoalMouth(char goalSide, float goalStartY, float goalEndY)
            {
                side = goalSide;
                startY = goalStartY;
                endY = goalEndY;
            }

            /// <summary>
            /// Creates a rectangle that represents the goal mouth.
            /// </summary>
            /// <param name="rectangle">Rectangle that contains the X coordinates for the goal mouth.</param>
            /// <returns></returns>
            internal Rectangle toRectangle(Rectangle rectangle)
            {
                Rectangle mouth;
                if (side == 'l')
                {
                    mouth.startX = mouth.endX = rectangle.startX;
                }
                else
                {
                    mouth.startX = mouth.endX = rectangle.endX;
                }
                mouth.startY = startY;
                mouth.endY = endY;
                return mouth;
            }
        }

        /// <summary>
        /// Structure to represent a flag
        /// </summary>
        internal struct Flag
        {
            /// <summary>
            /// The X coordainte of the flag.
            /// </summary>
            internal float x;
            /// <summary>
            /// The Y coordainte of the flag.
            /// </summary>
            internal float y;
            /// <summary>
            /// The name of the flag.
            /// </summary>
            internal string name;

            internal Flag(float xPosition, float yPosition, string flagName)
            {
                x = xPosition; 
                y = yPosition; 
                name = flagName;
            }

            /// <summary>
            /// Changes the actual distance to percentages of distance based on the inputer rectangle.
            /// </summary>
            /// <param name="rectangle">The rectangle to base percentages on</param>
            /// <param name="doChangeX">Do we change X?</param>
            /// <param name="doChangeY">Do we change Y?</param>
            /// <returns></returns>
            internal Flag percentify(Rectangle rectangle, bool doChangeX, bool doChangeY)
            {
                Flag newFlag = this;
                if (doChangeX)
                {
                    newFlag.x *= 0.01f;
                    newFlag.x = (rectangle.startX * (1 - newFlag.x)
                               + rectangle.endX * (1 + newFlag.x)) * 0.5f;
                }
                if (doChangeY)
                {
                    newFlag.y *= 0.01f;
                    newFlag.y = (rectangle.startY * (1 - newFlag.y)
                               + rectangle.endY * (1 + newFlag.y)) * 0.5f;
                }
                return newFlag;
            }
        }

        /// <summary>
        /// Strcuture to represent rectangle.
        /// </summary>
        internal struct Rectangle
        {
            /// <summary>
            /// Starting X coordinate of the rectangle.
            /// </summary>
            internal float startX;
            /// <summary>
            /// Starting Y coordiante of the rectangle.
            /// </summary>
            internal float startY;
            /// <summary>
            /// Ending X coordiante of the rectangle.
            /// </summary>
            internal float endX;
            /// <summary>
            /// Ending Y coordinate of the rectangle.
            /// </summary>
            internal float endY;

            /// <summary>
            /// Constructor to create a rectangle.
            /// </summary>
            /// <param name="startingX">The starting X coordinate.</param>
            /// <param name="startingY">The starting Y coordinate.</param>
            /// <param name="endingX">The ending X coordinate.</param>
            /// <param name="endingY">The ending Y coordainte.</param>
            internal Rectangle(float startingX, float startingY, float endingX, float endingY)
            {
                startX = startingX;
                startY = startingY;
                endX = endingX;
                endY = endingY;
            }

            /// <summary>
            /// Changes the rectangle to a percentage of the distance in a given rectangle.
            /// </summary>
            /// <param name="rectangle">The rectangle to change to.</param>
            /// <param name="doChangeStartX">Do we change the starting X?</param>
            /// <param name="doChangeStartY">Do we change the starting Y?</param>
            /// <param name="doChangeEndX">Do we change the ending X?</param>
            /// <param name="doChangeEndY">Do we change the ending Y?</param>
            /// <returns></returns>
            internal Rectangle percentify(Rectangle rectangle,
                                               bool doChangeStartX,
                                               bool doChangeStartY,
                                               bool doChangeEndX,
                                               bool doChangeEndY)
            {
                Rectangle newRectangle = this;
                if (doChangeStartX)
                {
                    newRectangle.startX *= 0.01f;
                    newRectangle.startX = (rectangle.startX * (1 - newRectangle.startX)
                                    + rectangle.endX * (1 + newRectangle.startX)) * 0.5f;
                }
                if (doChangeStartY)
                {
                    newRectangle.startY *= 0.01f;
                    newRectangle.startY = (rectangle.startY * (1 - newRectangle.startY)
                                    + rectangle.endY * (1 + newRectangle.startY)) * 0.5f;
                }
                if (doChangeEndX)
                {
                    newRectangle.endX *= 0.01f;
                    newRectangle.endX = (rectangle.startX * (1 - newRectangle.endX)
                                  + rectangle.endX * (1 + newRectangle.endX)) * 0.5f;
                }
                if (doChangeEndY)
                {
                    newRectangle.endY *= 0.01f;
                    newRectangle.endY = (rectangle.startY * (1 - newRectangle.endY)
                                  + rectangle.endY * (1 + newRectangle.endY)) * 0.5f;
                }
                return newRectangle;
            }
        }

        /// <summary>
        /// Stack for processing landmarks
        /// </summary>
        Stack<Rectangle> rectangleStack = new Stack<Rectangle>();

        #endregion

        #region Default Landmarks

        /// <summary>
        /// The default rectangle representing the playable field.
        /// </summary>
        static readonly Rectangle DefaultPitch = new Rectangle(-52.5f, -34.0f, 52.5f, 34.0f);

        /// <summary>
        /// The default center flag.
        /// </summary>
        static readonly Flag FlagCenter = new Flag(0.0f, 0.0f, "c");

        /// <summary>
        /// The default center-top flag.
        /// </summary>
        static readonly Flag FlagCenterTop = new Flag(0.0f, -100.0f, "c t");

        /// <summary>
        /// The default center-bottom flag.
        /// </summary>
        static readonly Flag FlagCenterBottom = new Flag(0.0f, 100.0f, "c b");

        /// <summary>
        /// The default right-top flag.
        /// </summary>
        static readonly Flag FlagRightTop = new Flag(100.0f, -100.0f, "r t");

        /// <summary>
        /// The default right-bottom flag.
        /// </summary>
        static readonly Flag FlagRightBottom = new Flag(100.0f, 100.0f, "r b");

        /// <summary>
        /// The default left-top flag.
        /// </summary>
        static readonly Flag FlagLeftTop = new Flag(-100.0f, -100.0f, "l t");

        /// <summary>
        /// The default left-bottom flag.
        /// </summary>
        static readonly Flag FlagLeftBottom = new Flag(-100.0f, 100.0f, "l b");

        /// <summary>
        /// The default mouth of the right goal.
        /// </summary>
        static readonly GoalMouth GoalMouthRight = new GoalMouth('r', -7.01f, 7.01f);

        /// <summary>
        /// The default mouth of the left goal.
        /// </summary>
        static readonly GoalMouth GoalMouthLeft = new GoalMouth('l', -7.01f, 7.01f);

        /// <summary>
        /// The default flag of the bottom of the right goal.
        /// </summary>
        static readonly Flag FlagGoalRightBottom = new Flag(0.0f, 100.0f, "g r b");

        /// <summary>
        /// The default flag for the right goal.
        /// </summary>
        static readonly Flag FlagGoalRight = new Flag(0.0f, 0.0f, "r");

        /// <summary>
        /// The default flag for the top of the right goal.
        /// </summary>
        static readonly Flag FlagGoalRightTop = new Flag(0.0f, -100.0f, "g r t");

        /// <summary>
        /// The default flag for the bottom of the left goal.
        /// </summary>
        static readonly Flag FlagGoalLeftBottom = new Flag(0.0f, 100.0f, "g l b");

        /// <summary>
        /// Default flag for the left goal.
        /// </summary>
        static readonly Flag FlagGoalLeft = new Flag(0.0f, 0.0f, "l");

        /// <summary>
        /// Default flag for the top of the left goal.
        /// </summary>
        static readonly Flag FlagGoalLeftTop = new Flag(0.0f, -100.0f, "g l t");

        /// <summary>
        /// Rectangle representing the default right penalty region.
        /// </summary>
        static readonly Rectangle PenaltyRight = new Rectangle(36.0f, -20.16f, 100.0f, 20.16f);
        
        /// <summary>
        /// Default flag for the right penalty's bottom
        /// </summary>
        static readonly Flag FlagPenaltyRightBottom = new Flag(-100.0f, 100.0f, "p r b");

        /// <summary>
        /// Default flag for the right penalty's center
        /// </summary>
        static readonly Flag FlagPenaltyRightCenter = new Flag(-100.0f, 0.0f, "p r c");

        /// <summary>
        /// Default flag for the right penalty's top
        /// </summary>
        static readonly Flag FlagPenaltyRightTop = new Flag(-100.0f, -100.0f, "p r t");

        /// <summary>
        /// Rectangle representing the default left penalty region.
        /// </summary>
        static readonly Rectangle PenaltyLeft = new Rectangle(-36.0f, -20.16f, -100.0f, 20.16f);
      
        /// <summary>
        /// The default flag for the left penalty's bottom.
        /// </summary>
        static readonly Flag FlagPenaltyLeftBottom = new Flag(-100.0f, 100.0f, "p l b");

        /// <summary>
        /// The default flag for the left penalty's center.
        /// </summary>
        static readonly Flag FlagPenaltyLeftCenter = new Flag(-100.0f, 0.0f, "p l c");

        /// <summary>
        /// The default flag for the left penalty's top.
        /// </summary>
        static readonly Flag FlagPenaltyLeftTop = new Flag(-100.0f, -100.0f, "p l t");

        /// <summary>
        /// Rectangle representing the outer boundary of the pitch.
        /// </summary>
        static readonly Rectangle OuterPitch = new Rectangle(-57.5f, -39.0f, 57.5f, 39.0f);

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTop0 = new Flag(0.0f, -100.0f, "t 0");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopRight10 = new Flag(10.0f, -100.0f, "t r 10");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopRight20 = new Flag(20.0f, -100.0f, "t r 20");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopRight30 = new Flag(30.0f, -100.0f, "t r 30");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopRight40 = new Flag(40.0f, -100.0f, "t r 40");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopRight50 = new Flag(50.0f, -100.0f, "t r 50");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopLeft10 = new Flag(-10.0f, -100.0f, "t l 10");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopLeft20 = new Flag(-20.0f, -100.0f, "t l 20");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopLeft30 = new Flag(-30.0f, -100.0f, "t l 30");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopLeft40 = new Flag(-40.0f, -100.0f, "t l 40");

        /// <summary>
        /// A default top flag.
        /// </summary>
        static readonly Flag FlagTopLeft50 = new Flag(-50.0f, -100.0f, "t l 50");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottom0 = new Flag(0.0f, 100.0f, "b 0");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomRight10 = new Flag(10.0f, 100.0f, "b r 10");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomRight20 = new Flag(20.0f, 100.0f, "b r 20");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomRight30 = new Flag(30.0f, 100.0f, "b r 30");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomRight40 = new Flag(40.0f, 100.0f, "b r 40");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomRight50 = new Flag(50.0f, 100.0f, "b r 50");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomLeft10 = new Flag(-10.0f, 100.0f, "b l 10");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomLeft20 = new Flag(-20.0f, 100.0f, "b l 20");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomLeft30 = new Flag(-30.0f, 100.0f, "b l 30");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomLeft40 = new Flag(-40.0f, 100.0f, "b l 40");

        /// <summary>
        /// A default bottom flag.
        /// </summary>
        static readonly Flag FlagBottomLeft50 = new Flag(-50.0f, 100.0f, "b l 50");

        /// <summary>
        /// A default right flag.
        /// </summary>
        static readonly Flag FlagRight0 = new Flag(100.0f, 0.0f, "r 0");

        /// <summary>
        /// A default right flag.
        /// </summary>
        static readonly Flag FlagRightTop10 = new Flag(100.0f, -10.0f, "r t 10");

        /// <summary>
        /// A default right flag.
        /// </summary>
        static readonly Flag FlagRightTop20 = new Flag(100.0f, -20.0f, "r t 20");

        /// <summary>
        /// A default right flag.
        /// </summary>
        static readonly Flag FlagRightTop30 = new Flag(100.0f, -30.0f, "r t 30");

        /// <summary>
        /// A default right flag.
        /// </summary>
        static readonly Flag FlagRightBottom10 = new Flag(100.0f, 10.0f, "r b 10");

        /// <summary>
        /// A default right flag.
        /// </summary>
        static readonly Flag FlagRightBottom20 = new Flag(100.0f, 20.0f, "r b 20");

        /// <summary>
        /// A default right flag.
        /// </summary>
        static readonly Flag FlagRightBottom30 = new Flag(100.0f, 30.0f, "r b 30");

        /// <summary>
        /// A default left flag.
        /// </summary>
        static readonly Flag FlagLeft0 = new Flag(-100.0f, 0.0f, "l 0");

        /// <summary>
        /// A default left flag.
        /// </summary>
        static readonly Flag FlagLeftTop10 = new Flag(-100.0f, -10.0f, "l t 10");

        /// <summary>
        /// A default left flag.
        /// </summary>
        static readonly Flag FlagLeftTop20 = new Flag(-100.0f, -20.0f, "l t 20");

        /// <summary>
        /// A default left flag.
        /// </summary>
        static readonly Flag FlagLeftTop30 = new Flag(-100.0f, -30.0f, "l t 30");

        /// <summary>
        /// A default left flag.
        /// </summary>
        static readonly Flag FlagLeftBottom10 = new Flag(-100.0f, 10.0f, "l b 10");

        /// <summary>
        /// A default left flag.
        /// </summary>
        static readonly Flag FlagLeftBottom20 = new Flag(-100.0f, 20.0f, "l b 20");

        /// <summary>
        /// A default left flag.
        /// </summary>
        static readonly Flag FlagLeftBottom30 = new Flag(-100.0f, 30.0f, "l b 30");

        #endregion

        #region Instance Variables and Properties

        /// <summary>
        /// The left line of the field
        /// </summary>
        internal BasicObject lineLeft;
        /// <summary>
        /// The right line of the field
        /// </summary>
        internal BasicObject lineRight;
        /// <summary>
        /// The top line fo the field
        /// </summary>
        internal BasicObject lineTop;
        /// <summary>
        /// The bottom line of the field
        /// </summary>
        internal BasicObject lineBottom;
        /// <summary>
        /// List of field landmarks
        /// </summary>
        internal List<BasicObject> landmarks = new List<BasicObject>();
        /// <summary>
        /// List of the goals on the field
        /// </summary>
        internal List<BasicObject> goals = new List<BasicObject>();

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for the Field
        /// </summary>
        public Field():this(68.0f, 105.0f)
        {

        }

        /// <summary>
        /// Constructor that creates a field with a specified width and length
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        public Field(float width, float length)
        {


            lineLeft = new BasicObject("line l", new RoboCup.Geometry.Vector(-length / 2.0f, 0.0f));
            lineRight = new BasicObject("line r", new RoboCup.Geometry.Vector(length / 2.0f, 0.0f));
            lineTop = new BasicObject("line t", new RoboCup.Geometry.Vector(-width / 2.0f, 0.0f));
            lineBottom = new BasicObject("line b", new RoboCup.Geometry.Vector(width / 2.0f, 0.0f));
            loadDefaults();

        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Loads the default LandMark values
        /// </summary>
        internal void loadDefaults()
        {

            rectangleStack.Push(DefaultPitch);

            addFlag(FlagCenter.percentify(rectangleStack.Peek(),
                                                        true, true));
            addFlag(FlagCenterTop.percentify(rectangleStack.Peek(),
                                                          true, true));
            addFlag(FlagCenterBottom.percentify(rectangleStack.Peek(),
                                                          true, true));
            addFlag(FlagRightTop.percentify(rectangleStack.Peek(),
                                                          true, true));
            addFlag(FlagRightBottom.percentify(rectangleStack.Peek(),
                                                          true, true));
            addFlag(FlagLeftTop.percentify(rectangleStack.Peek(),
                                                          true, true));
            addFlag(FlagLeftBottom.percentify(rectangleStack.Peek(),
                                                          true, true));
            rectangleStack.Push(GoalMouthRight.toRectangle(rectangleStack.Peek()));
            addFlag(FlagGoalRightBottom.percentify(rectangleStack.Peek(),
                                                            true, true));
            addFlag(FlagGoalRight.percentify(rectangleStack.Peek(),
                                                        true, true), true);
            addFlag(FlagGoalRightTop.percentify(rectangleStack.Peek(),
                                                            true, true));
            rectangleStack.Pop();
            rectangleStack.Push(GoalMouthLeft.toRectangle(rectangleStack.Peek()));
            addFlag(FlagGoalLeftBottom.percentify(rectangleStack.Peek(),
                                                            true, true));
            addFlag(FlagGoalLeft.percentify(rectangleStack.Peek(),
                                                        true, true), true);
            addFlag(FlagGoalLeftTop.percentify(rectangleStack.Peek(),
                                                            true, true));
            rectangleStack.Pop();
            rectangleStack.Push(PenaltyRight.percentify(rectangleStack.Peek(),
                                                               false, false,
                                                               true, false));

            addFlag(FlagPenaltyRightBottom.percentify(rectangleStack.Peek(),
                                                            true, true));
            addFlag(FlagPenaltyRightCenter.percentify(rectangleStack.Peek(),
                                                            true, true));
            addFlag(FlagPenaltyRightTop.percentify(rectangleStack.Peek(),
                                                            true, true));
            rectangleStack.Pop();
            rectangleStack.Push(PenaltyLeft.percentify(rectangleStack.Peek(),
                                                               false, false,
                                                               true, false));
            addFlag(FlagPenaltyLeftBottom.percentify(rectangleStack.Peek(),
                                                            true, true));
            addFlag(FlagPenaltyLeftCenter.percentify(rectangleStack.Peek(),
                                                            true, true));
            addFlag(FlagPenaltyLeftTop.percentify(rectangleStack.Peek(),
                                                            true, true));
            rectangleStack.Pop();
            rectangleStack.Push(OuterPitch);
            addFlag(FlagTop0.percentify(rectangleStack.Peek(),
                                                          false, true));
            addFlag(FlagTopRight10.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagTopRight20.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagTopRight30.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagTopRight40.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagTopRight50.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagTopLeft10.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagTopLeft20.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagTopLeft30.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagTopLeft40.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagTopLeft50.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottom0.percentify(rectangleStack.Peek(),
                                                          false, true));
            addFlag(FlagBottomRight10.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottomRight20.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottomRight30.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottomRight40.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottomRight50.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottomLeft10.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottomLeft20.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottomLeft30.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottomLeft40.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagBottomLeft50.percentify(rectangleStack.Peek(),
                                                             false, true));
            addFlag(FlagRight0.percentify(rectangleStack.Peek(),
                                                          true, false));
            addFlag(FlagRightTop10.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagRightTop20.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagRightTop30.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagRightBottom10.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagRightBottom20.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagRightBottom30.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagLeft0.percentify(rectangleStack.Peek(),
                                                          true, false));
            addFlag(FlagLeftTop10.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagLeftTop20.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagLeftTop30.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagLeftBottom10.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagLeftBottom20.percentify(rectangleStack.Peek(),
                                                             true, false));
            addFlag(FlagLeftBottom30.percentify(rectangleStack.Peek(),
                                                             true, false));
            rectangleStack.Pop();
            rectangleStack.Pop();
        }

        /// <summary>
        /// Adds a flag to the Field.
        /// </summary>
        /// <param name="flag">The flag to be added</param>
        internal void addFlag(Flag flag)
        {
            addFlag(flag, false);
        }

        /// <summary>
        /// Adds a flag to Field, that may or may not define a goal
        /// </summary>
        /// <param name="flag">Flag to be added</param>
        /// <param name="goal">Is the flag a goal?</param>
        internal void addFlag(Flag flag, bool goal)
        {
            // add goal to stadium
            string landmarkName;

            if (goal)
            {
                landmarkName = "g " + flag.name;

                addLandmark(new BasicObject(landmarkName, new Vector(flag.x, flag.y)));
            }
            else
            {
                landmarkName = "f " + flag.name;

                addLandmark(new BasicObject(landmarkName, new Vector(flag.x, flag.y)));
            }
        }

        /// <summary>
        /// Adds a new landmark to the field
        /// </summary>
        /// <param name="pObject">The landmark to add</param>
        internal void addLandmark(BasicObject landmark)
        {

                if (landmark.Name[0] == 'g')
                {
                    goals.Add(landmark);
                }
                landmarks.Add(landmark);
        }

        #endregion
    }
}
