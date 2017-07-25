// -*-C#-*-

/***************************************************************************
                                   CArea.cs
                    Class that represents a circular area.
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

namespace RoboCup.Geometry
{
    /// <summary>
    /// Structure that represents a circular area.
    /// </summary>
    public class Circle
    {
        #region Instance Variables

        Vector circleCenter;

        /// <summary>
        /// Vector representing the center of the circle.
        /// </summary>
        public Vector Center
        {

            get { return circleCenter; }

        }

        float circleRadius;
        /// <summary>
        /// The radius of the circle.
        /// </summary>
        public float Radius
        {
            get { return circleRadius; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor to create a ciruclar area.
        /// </summary>
        /// <param name="center">Vector representing the center of the circle.</param>
        /// <param name="radius">Double representing the radius of the circle.</param>
        public Circle(Vector center, float radius)
        {
            circleCenter = new Vector(center.X, center.Y);
            circleRadius = radius;
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Determines if a given vector is within the circle.
        /// </summary>
        /// <param name="position">The position being considered.</param>
        /// <returns>Returns true iff the distance from the center to the vector is less than the radius.</returns>
        public bool containsPoint(Vector position)
        {
            return circleCenter.distance(position) <= circleRadius;
        }

        /// <summary>
        /// Finds the nearest point on the edge of the circle to a vector.
        /// </summary>
        /// <param name="position">The position being considered.</param>
        /// <returns>Returns a new vector representing a point on the edge of the circle.</returns>
        public Vector closestPointOnCircleToPosition(Vector position)
        {
            Vector distanceFromCenter = position - circleCenter;

            if (distanceFromCenter.X == 0.0f && distanceFromCenter.Y == 0.0f)
            {
                distanceFromCenter = new Vector(Defines.Epsilon, Defines.Epsilon);
            }

            distanceFromCenter.normalize(circleRadius);

            return circleCenter + distanceFromCenter;
        }

        /// <summary>
        /// Produces a string representation of the circluar area.
        /// </summary>
        /// <returns>Returns the string #A[x:C.x,y:C.y,r:Radius]</returns>
        public override string ToString()
        {
            return "#A[x:" + Center.X + ",y:" + Center.Y + ",r:" + Radius + "]";
        }

        /// <summary>
        /// Produces a HashCode based on a string representation of the circluar area.
        /// </summary>
        /// <returns>Returns an integer HashCode.</returns>
        public override int GetHashCode()
        {
            return (Center.X.ToString() +","+ Center.Y.ToString() +","+ Radius.ToString()).GetHashCode();
        }

        /// <summary>
        /// Determines if this instance is equal to the input object.
        /// </summary>
        /// <param name="obj">Object being compared to.</param>
        /// <returns>Returns true iff obj is the same reference.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Circle)) return false;
            return base.Equals(obj);

        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two circular areas are equal.
        /// </summary>
        /// <param name="leftHandSideCircle">First circular area.</param>
        /// <param name="rightHandSideCircle">Second circular area.</param>
        /// <returns>Returns true iff the centers and radii are equal.</returns>
        public static bool operator ==(Circle leftHandSideCircle, Circle rightHandSideCircle)
        {
            return leftHandSideCircle.Center == rightHandSideCircle.Center && leftHandSideCircle.Radius == rightHandSideCircle.Radius;
        }

        /// <summary>
        /// Determines if two cirular areas are not equal.
        /// </summary>
        /// <param name="leftHandSideCircle">First circular area.</param>
        /// <param name="rightHandSideCircle">Second ciruclar area.</param>
        /// <returns>Returns true iff the centers or the radii are not equal.</returns>
        public static bool operator !=(Circle leftHandSideCircle, Circle rightHandSideCircle)
        {
            return !(leftHandSideCircle.Center == rightHandSideCircle.Center && leftHandSideCircle.Radius == rightHandSideCircle.Radius);
        }

        #endregion

    }
}
