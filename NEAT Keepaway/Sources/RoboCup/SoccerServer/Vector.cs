// -*-C#-*-

/***************************************************************************
                                   Vector.cs
                    Class that represents a plain vector.
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
    /// Structure that represents a plain vector.
    /// </summary>
    public class Vector
    {

        #region Instance Variables and Properties


        /// <summary>
        /// X component of the vector
        /// </summary>
        public float X;

        
        /// <summary>
        /// Y component of the vector
        /// </summary>
        public float Y;


        /// <summary>
        /// Z component of the vector
        /// </summary>
        public float Z;


        /// <summary>
        /// Squared distance of the vector
        /// </summary>
        public float RadiusSquared
        {
            get
            {
                return X * X + Y * Y + Z * Z; ;
            }
        }

        /// <summary>
        /// Distance of the vector
        /// </summary>
        public double Radius
        {
            get { return Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        /// <summary>
        /// Angle between the vector and the X-axis
        /// </summary>
        public double Theta
        {
            get { return ((X == 0.0f) && (Y == 0.0f) ? 0.0 : Math.Atan2(Y, X)); }
        }


        /// <summary>
        /// Angle between the vector and the XY-Plane
        /// </summary>
        public double Rho
        {
            get { return ((Z == 0.0f && (RadiusSquared - Z*Z) == 0.0f) ? 0.0 : Math.Atan2(Z, Math.Sqrt(this.RadiusSquared - Z*Z))); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Full constructor to defined a 3D vector.
        /// </summary>
        /// <param name="xx">X component.</param>
        /// <param name="yy">Y component.</param>
        /// <param name="zz">Z component.</param>
        public Vector(float xx, float yy, float zz)
        {
            X = xx;
            Y = yy;
            Z = zz;
        }

        /// <summary>
        /// Constructor for a 2D vector (Z defaults to 0.0).
        /// </summary>
        /// <param name="xx">X component.</param>
        /// <param name="yy">Y component.</param>
        public Vector(float xx, float yy)
        {
            X = xx;
            Y = yy;
            Z = 0.0f;
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Assigns values to the X and Y components of the vector.
        /// </summary>
        /// <param name="xx">X component.</param>
        /// <param name="yy">Y Component.</param>
        /// <returns>Returns this PVector with the altered components.</returns>
        public void assign(float xx, float yy)
        {
            X = xx;
            Y = yy;
        }

        /// <summary>
        /// Assingns values o the X, Y, and Z components of the vector.
        /// </summary>
        /// <param name="xx">X component.</param>
        /// <param name="yy">Y component.</param>
        /// <param name="zz">Z component.</param>
        /// <returns>Returns this PVector with altered components.</returns>
        public void assign(float xx, float yy, float zz)
        {
            X = xx;
            Y = yy;
            Z = zz;
        }

        /// <summary>
        /// Assigns this vector the values of another vector
        /// </summary>
        /// <param name="vector">The vector to be assigned to.</param>
        public void assign(Vector vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        /// <summary>
        /// Produces a HashCode based on a string represention of X,Y,Z
        /// </summary>
        /// <returns>An integer hash code.</returns>
        public override int GetHashCode()
        {
            return (X.ToString() + "," + Y.ToString() + "," + Z.ToString()).GetHashCode();
        } 

        /// <summary>
        /// Provides a string representation of the 2D vector in this object.
        /// </summary>
        /// <returns>The string #V[X,Y].</returns>
        public override string ToString()
        {
            return "#V[" + this.X.ToString("0.##") + "," + this.Y.ToString("0.##") + "]";
        }

        /// <summary>
        /// Provides a string representation of the 3D vector in this object.
        /// </summary>
        /// <returns>The string #V[X,Y,Z].</returns>
        public string ToString3D()
        {
            return "#V[" + this.X + "," + this.Y + "," + this.Z + "]";
        }

        /// <summary>
        /// Determines the input object is equal to the current PVector.
        /// </summary>
        /// <param name="obj">Object being compared to.</param>
        /// <returns>Returns true iff obj is the same object.</returns>
        public override bool Equals(object obj)
        {

            return base.Equals(obj);
        } 

        /// <summary>
        /// Roates the vector by an angle along the Z-axis.
        /// </summary>
        /// <param name="angle">Angle to rotate Th by.</param>
        /// <returns>This vector with resulting rotated values.</returns>
        public void rotate(float angle)
        {
            float radius = (float)Radius;
            float currentAngle = (float)Theta;

            X = radius * (float)Math.Cos(currentAngle + angle);
            Y = radius * (float)Math.Sin(currentAngle + angle);

        }

        /// <summary>
        /// Roates the vector along perpendicular line to the X and Y projection.
        /// </summary>
        /// <param name="angle">Angle to rotate Rho by.</param>
        /// <returns>This vector with resulting rotated values.</returns>
        public void rotateRho(float angle)
        {
            float radius = (float)Radius;
            float currentAngleTheta = (float)Theta;
            float currentAngleRho = (float)Rho;

            Z = radius * (float)Math.Sin(angle + currentAngleRho);

            X = radius * (float)(Math.Sin(angle + currentAngleRho) * Math.Cos(currentAngleTheta));
            Y = radius * (float)(Math.Sin(angle + currentAngleRho) * Math.Sin(currentAngleTheta));

        }

        /// <summary>
        /// Determines is this vector is between two other vectors.
        /// </summary>
        /// <param name="beginPoint">The beginning vector point.</param>
        /// <param name="endPoint">The ending vector point.</param>
        /// <returns>True if this vector's components lays between the other two vectors.</returns>
        public bool between(Vector beginPoint, Vector endPoint)
        {
            if (beginPoint.X > endPoint.X)
                return between(endPoint, beginPoint);

            if (beginPoint.X <= X && X <= endPoint.X)
            {
                if (beginPoint.Y < endPoint.Y)
                {
                    if (beginPoint.Y <= Y && Y <= endPoint.Y)
                    {
                        if (beginPoint.Z < endPoint.Z)
                        {
                            return (beginPoint.Z <= Z && Z <= endPoint.Z);
                        }
                        else
                        {
                            return (beginPoint.Z >= Z && Z >= endPoint.Z);
                        }
                    }
                }
                else
                {
                    if (beginPoint.Y >= Y && Y >= endPoint.Y)
                    {
                        if (beginPoint.Z < endPoint.Z)
                        {
                            return (beginPoint.Z <= Z && Z <= endPoint.Z);
                        }
                        else
                        {
                            return (beginPoint.Z >= Z && Z >= endPoint.Z);
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Normalizes this angle to a unit vector.
        /// </summary>
        /// <returns>Returns this vector with values adjusted to be unit.</returns>
        public void normalize()
        {
            Vector normalized = this * (1.0f / (float)Math.Max(Radius, Defines.Epsilon));
            this.X = normalized.X;
            this.Y = normalized.Y;
            this.Z = normalized.Z;

        }

        /// <summary>
        /// Normalizes a vector to a specific distance.
        /// </summary>
        /// <param name="newRadius">Distance to normalize vector to.</param>
        /// <returns>Returns this vector normalized to l.</returns>
        public void normalize(float newRadius)
        {

            Vector normalized = this * (newRadius / (float)Math.Max(Radius, Defines.Epsilon));
            this.X = normalized.X;
            this.Y = normalized.Y;
            this.Z = normalized.Z;
        }

        /// <summary>
        /// Determines the sqaure distance between this vector and an input origin.
        /// </summary>
        /// <param name="origin">The origin point to determine distance square from.</param>
        /// <returns>Returns the distance squared.</returns>
        public float distanceSquared(Vector origin)
        {
            return (this - origin).RadiusSquared;
        }

        /// <summary>
        /// Determines the distance between this vector and an input origin.
        /// </summary>
        /// <param name="origin">The origin point to determine distance from.</param>
        /// <returns>Returns the distance.</returns>
        public float distance(Vector origin)
        {
            return (float)(this - origin).Radius;
        }

        /// <summary>
        /// Finds the angle between two Vectors
        /// </summary>
        /// <param name="vector">The vector to find the angle to</param>
        /// <returns>The angle ebtween two PVectors</returns>
        public float angle(Vector vector)
        {
            float ang = (float)(vector.Theta - this.Theta);
            return (float)Utilities.Normalize_angle(ang);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Unary negation
        /// </summary>
        /// <param name="point">Vector being negated.</param>
        /// <returns>Returns that is -p.</returns>
        public static Vector operator-(Vector point){

            Vector temp = new Vector(-point.X, -point.Y, -point.Z);
            

            return temp;
        }

        /// <summary>
        /// Binary addition.
        /// </summary>
        /// <param name="point1">First vector.</param>
        /// <param name="point2">Second vector.</param>
        /// <returns>Returns a new vector, p + q.</returns>
        public static Vector operator +(Vector point1, Vector point2)
        {
            return new Vector(point1.X + point2.X, point1.Y + point2.Y, point1.Z + point2.Z);
        }

        /// <summary>
        /// Binary subtraction.
        /// </summary>
        /// <param name="point1">Vector being subtracted from.</param>
        /// <param name="point2">Vector being subtracted.</param>
        /// <returns>Returns a new vector, p - q.</returns>
        public static Vector operator -(Vector point1, Vector point2)
        {
            return new Vector(point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z);
        }

        /// <summary>
        /// Scaler multiplication.
        /// </summary>
        /// <param name="point">Vector being multiplied.</param>
        /// <param name="coefficient">Scaler multiplier.</param>
        /// <returns>Returns a new vector, p * a.</returns>
        public static Vector operator *(Vector point, float coefficient)
        {

            return new Vector(point.X*coefficient, point.Y*coefficient, point.Z*coefficient);

        }

        /// <summary>
        /// Dot product of two vectors.
        /// </summary>
        /// <param name="point1">First vector.</param>
        /// <param name="point2">Second vector.</param>
        /// <returns>Returns a float value that is p * q.</returns>
        public static float operator *(Vector point1, Vector point2)
        {
            return point1.X * point2.X + point1.Y * point2.Y + point1.Z * point2.Z;
        }

        /// <summary>
        /// Scaler division.
        /// </summary>
        /// <param name="point">Vector being divided.</param>
        /// <param name="coefficient">Scaler divisor.</param>
        /// <returns>Returns a new vector, p / a.</returns>
        public static Vector operator /(Vector point, float coefficient)
        {
            return new Vector(point.X / coefficient, point.Y / coefficient, point.Z / coefficient);

        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="point1">First vector being compared.</param>
        /// <param name="point2">Second vector being compared.</param>
        /// <returns>Returns true iff all components of p are equal to components of q.</returns>
        public static bool operator ==(Vector point1, Vector point2)
        {
            return point1.X == point2.X && point1.Y == point2.Y && point1.Z == point2.Z;
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="point1">First vector being compared.</param>
        /// <param name="point2">Second vector being compared.</param>
        /// <returns>Returns true iff at least one component of p does not equal the matching component in q.</returns>
        public static bool operator !=(Vector point1, Vector point2)
        {
            return !(point1.X == point2.X && point1.Y == point2.Y && point1.Z == point2.Z);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Produces a new 2D vector from polar coordinates.
        /// </summary>
        /// <param name="radius">The distance of the vector.</param>
        /// <param name="angle">The angle of the vector with the X-axis.</param>
        /// <returns>Returns a new vector with values derived from the polar coordinates.</returns>
        public static Vector fromPolar(float radius, float angle)
        {

            return new Vector(radius * (float)Math.Cos(angle), radius * (float)Math.Sin(angle), 0.0f);

        }

        /// <summary>
        /// Produces a new 3D vector from polar coordinates.
        /// </summary>
        /// <param name="radius">The distance of the vector.</param>
        /// <param name="theta">The angle from the X-axis.</param>
        /// <param name="rho">The andle from the XY-plane.</param>
        /// <returns>Returns a new vector with values derived from the polar coordinates.</returns>
        public static Vector fromPolar(float radius, float theta, float rho)
        {
            return new Vector(radius * (float)(Math.Cos(rho) * Math.Cos(theta)), radius * (float)(Math.Cos(rho) * Math.Sin(theta)), radius * (float)Math.Sin(rho));
        }

        #endregion

    }
}
