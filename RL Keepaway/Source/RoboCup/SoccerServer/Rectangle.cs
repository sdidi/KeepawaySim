// -*-C#-*-

/***************************************************************************
                                   RArea.cs
                    Class that represents a rectangular area.
                             -------------------
    begin                : JUL-2009
    credit               : Translated from / Based on the RoboCup Soccer Server
                           by The RoboCup Soccer Server  Maintenance Group.
                           (Implementation done by Phillip Verbancsics of the
                            Evolutionary Complexity Research Group)
                           Maintenance Group.
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

namespace RoboCup.Geometry
{
    /// <summary>
    /// Structure that represents a Rectangular area.
    /// </summary>
    public class Rectangle
    {

        #region Instance Variables
        /// <summary>
        /// Left side of the rectangle
        /// </summary>
        public float left;

        /// <summary>
        /// Right side of the rectangle
        /// </summary>
        public float right;

        /// <summary>
        /// Top of the rectangle
        /// </summary>
        public float top;

        /// <summary>
        /// Bottom of the rectangle
        /// </summary>
        public float bottom;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that takes in the exact locations of each side of the rectangle.
        /// </summary>
        /// <param name="leftSide">Left side's position.</param>
        /// <param name="rightSide">Right side's position.</param>
        /// <param name="topSide">Top side's position.</param>
        /// <param name="bottomSide">Bottom side's position.</param>
        public Rectangle(float leftSide, float rightSide, float topSide, float bottomSide)
        {
            left = leftSide;
            right = rightSide;
            top = topSide;
            bottom = bottomSide;

        }

        /// <summary>
        /// Constructor that builds a rectangle off of a center and size, given by two vectors.
        /// </summary>
        /// <param name="center">Center of the rectangle.</param>
        /// <param name="size">X and Y sizes of the rectangle.</param>
        public Rectangle(Vector center, Vector size)
        {
            left = center.X - size.X * 0.5f;
            right = center.X + size.X * 0.5f;
            top = center.Y - size.Y * 0.5f;
            bottom = center.Y + size.Y * 0.5f;

        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Determines if a given vector is inside this rectangle.
        /// </summary>
        /// <param name="position">The vector being looked at.</param>
        /// <returns>Returns true iff the components of the vector lay within the rectangle.</returns>
        public bool ContainsPoint(Vector position)
        {
            return (position.X >= left)
                && (position.X <= right)
                && (position.Y >= top)
                && (position.Y <= bottom);
        }

        /// <summary>
        /// Finds the nearest horizontal edge of this rectangle to a vector.
        /// </summary>
        /// <param name="position">The vector being looked at.</param>
        /// <returns>Returns the location of the nearest point on the nearest horizontal edge.</returns>
        public Vector NearestHorizontalEdgeToPoint(Vector position)
        {
            return new Vector(Math.Min(Math.Max(position.X, left), right),
                            (Math.Abs(position.Y - top) < Math.Abs(position.Y - bottom)
                              ? top : bottom));
        }

        /// <summary>
        /// Finds the nearest vertical edge of this rectangle to a vector.
        /// </summary>
        /// <param name="position">The vector being looked at.</param>
        /// <returns>Returns the location of the nearest point on the nearest vertical edge.</returns>
        public Vector NearestVerticalEdgeToPoint(Vector position)
        {
            return new Vector((Math.Abs(position.X - left) < Math.Abs(position.X - right)
                              ? left : right),
                            Math.Min(Math.Max(position.Y, top), bottom));
        }

        /// <summary>
        /// Finds the nearest edge of this rectangle to a vector.
        /// </summary>
        /// <param name="position">The vector being looked at.</param>
        /// <returns>Returns the location of the nearest point on the nearest edge.</returns>
        public Vector NearestEdgeToPoint(Vector position)
        {
            if (Math.Min(Math.Abs(position.X - left), Math.Abs(position.X - right))
                 < Math.Min(Math.Abs(position.Y - top), Math.Abs(position.Y - bottom)))
            {
                return NearestVerticalEdgeToPoint(position);
            }
            else
            {
                return NearestHorizontalEdgeToPoint(position);
            }
        }

        /// <summary>
        /// Produces a vector that is located randomly within the rectangle.
        /// </summary>
        /// <returns>Returns a new vector that is within the rectangle.</returns>
        public Vector RandomLocationInside()
        {

            return new Vector((float)Utilities.CustomRandom.NextDouble(left, right),
                            (float)Utilities.CustomRandom.NextDouble(bottom, top));
        }

        /// <summary>
        /// Produces a string representation of the rectangle.
        /// </summary>
        /// <returns>Returns the string #A[h:left~right,v:top~bottom].</returns>
        public override string ToString()
        {
            return "#A[h:" + left + "~" + right + ",v:" + top + "~" + bottom + "]";
        }

        #endregion
    }
}
