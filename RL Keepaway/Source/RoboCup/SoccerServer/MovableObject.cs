// -*-C#-*-

/***************************************************************************
                                   MPObject.cs
                    Class that represents a basic movable object in RoboCup.
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
using RoboCup.Geometry;
using RoboCup.Environment;
using RoboCup.Parameters;

namespace RoboCup.Objects
{
    /// <summary>
    /// Basic class that represents a movable object.
    /// </summary>
    public class MovingObject : BasicObject
    {

        #region Instance Variables

        /// <summary>
        /// Reference to the stadium containing this movable object.
        /// </summary>
        internal Stadium Stadium;

        internal Vector objectVelocity;

        /// <summary>
        /// The velocity of this object.
        /// </summary>
        internal Vector Velocity
        {
            get { return objectVelocity; }
            set { objectVelocity.X = value.X; objectVelocity.Y = value.Y; objectVelocity.Z = value.Z; }
        }

        internal Vector objectAcceleration = new Vector(0,0);

        /// <summary>
        /// The acceleration of this object.
        /// </summary>
        internal Vector Acceleration
        {
            get { return objectAcceleration; }
            set { objectAcceleration.X = value.X; objectAcceleration.Y = value.Y; objectAcceleration.Z = value.Z; }
        }

        /// <summary>
        /// Position to move the object to.
        /// </summary>
        internal Vector MoveTo
        {
            set { Position = value; }
        }

        /// <summary>
        /// Decay associated with the object.
        /// </summary>
        internal float Decay;

        /// <summary>
        /// Randomness associated with object.
        /// </summary>
        internal float Random;

        /// <summary>
        /// Weight of the object.
        /// </summary>
        internal float Weight;

        /// <summary>
        /// Maximum speed of the object.
        /// </summary>
        internal float MaximumSpeed;
        
        /// <summary>
        /// Maximum acceleration of the object.
        /// </summary>
        internal float MaximumAcceleration;

        /// <summary>
        /// Position post collision.
        /// </summary>
        internal Vector PostCollisionPosition = new Vector(0, 0);

        /// <summary>
        /// Number of collisions.
        /// </summary>
        internal int CollisionCount;

        /// <summary>
        /// Has it collided?
        /// </summary>
        internal bool WasThereCollision;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for a Movable object.
        /// </summary>
        /// <param name="stadium">The stadium which contains this object.</param>
        /// <param name="name">The name of this object.</param>
        public MovingObject(Stadium stadium, string name)
            : base(name)
        {
            Stadium = stadium;
            objectVelocity = new Vector(0, 0);
            objectAcceleration = new Vector(0, 0);
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Produces a string representation of the movable object.
        /// </summary>
        /// <returns>Returns a string representing the object.</returns>
        public override string ToString()
        {
            string objectString = "#Ob[" + Id;

            if (Name != "")
            {
                objectString += ":" + Name + "";
            }
            return objectString + ":pos=" + this.Position.ToString() + ",size=" + Size + ",vel=" + Velocity.ToString() + ",acc=" + Acceleration.ToString() + ",decay=" + Decay + ",randp=" + Random + "]";

        }

        /// <summary>
        /// Moves this object to a particular position, speed and velocity.
        /// </summary>
        /// <param name="newPosition">Position to move to.</param>
        /// <param name="newVelocity">Velocity to make the object have.</param>
        /// <param name="newAcceleration">Acceeration to make the object have.</param>
        public void moveTo(Vector newPosition, Vector newVelocity, Vector newAcceleration)
        {
            Position = newPosition;
            Velocity = newVelocity;
            Acceleration = newAcceleration;
        }

        /// <summary>
        /// Increases the acceleration of the object.
        /// </summary>
        /// <param name="force">The vector to accelerate by.</param>
        internal void accelerateObject(Vector force)
        {
            objectAcceleration.X += force.X;
            objectAcceleration.Y += force.Y;
            objectAcceleration.Z += force.Z;
        }

        /// <summary>
        /// Clears the collisions of the object
        /// </summary>
        internal void clearCollide()
        {
            PostCollisionPosition.assign(0.0f, 0.0f);
            CollisionCount = 0;
        }

        /// <summary>
        /// Performs a collision on the object at the given position
        /// </summary>
        /// <param name="collisionPosition">The position of the collision.</param>
        internal void collide(Vector collisionPosition)
        {

            PostCollisionPosition.assign(PostCollisionPosition.X + collisionPosition.X, PostCollisionPosition.Y + collisionPosition.Y, PostCollisionPosition.Z + collisionPosition.Z);
            ++CollisionCount;
            WasThereCollision = true;
        }

        /// <summary>
        /// Updates the velocity after a collision.
        /// </summary>
        internal void updateVelocityAfterCollision()
        {
            if (WasThereCollision)
            {
                objectVelocity.X = objectVelocity.X  * -0.1f;
                objectVelocity.Y = objectVelocity.Y * -0.1f;
                objectVelocity.Z = objectVelocity.Z * -0.1f;
                this.WasThereCollision = false;
            }
        }

        /// <summary>
        /// Generates a noise vector based off of velocity..
        /// </summary>
        /// <returns>Returns a vector representing noise of the object's velocity.</returns>
        internal Vector generateNoise()
        {
            double maxRandomness = Random * Velocity.Radius;

            return Vector.fromPolar((float)Utilities.CustomRandom.NextDouble(0.0, maxRandomness), (float)Utilities.CustomRandom.NextDouble(-Math.PI, Math.PI));
        }

        /// <summary>
        /// Determines the relative vector of the wind to the object.
        /// </summary>
        /// <returns>Returns a vector representing the relative wind to the object.</returns>
        internal Vector getRelativeWindDirection()
        {
            Weather wind = Stadium.Weather;

            if (wind.WindRandomness < Defines.Epsilon)
            {
                return new Vector(0.0f, 0.0f);
            }

            float objectSpeed = (float)objectVelocity.Radius;

            return new Vector(objectSpeed * (wind.WindVector.X + (float)Utilities.CustomRandom.NextDouble(-wind.WindRandomness, wind.WindRandomness) / (Weight * ServerParam.WIND_WEIGHT)),
                                objectSpeed * (wind.WindVector.Y + (float)Utilities.CustomRandom.NextDouble(-wind.WindRandomness, wind.WindRandomness) / (Weight * ServerParam.WIND_WEIGHT)));
        }

        /// <summary>
        /// Increments the object (applies movement, etc.)
        /// </summary>
        internal void incrementObjectState()
        {
            if (objectAcceleration.X != 0.0 || objectAcceleration.Y != 0.0)
            {
                double maxmumAcceleration = MaximumAcceleration;
                double maximumSpeed = MaximumSpeed;

                double tempValue = objectAcceleration.Radius;
                if (tempValue > maxmumAcceleration)
                {
                    Acceleration = objectAcceleration * (float)(maxmumAcceleration / tempValue);
                }

                objectVelocity.assign(objectVelocity.X + Acceleration.X, objectVelocity.Y + Acceleration.Y, objectVelocity.Z + Acceleration.Z);
                tempValue = Velocity.Radius;
                if (tempValue > maximumSpeed)
                {
                    Velocity = objectVelocity * (float)(maximumSpeed / tempValue);
                }
            }

            updateAngle();
            Vector tempVector = generateNoise();
            Velocity.X += tempVector.X;
            Velocity.Y += tempVector.Y;
            Velocity.Z += tempVector.Z;

            tempVector = getRelativeWindDirection();
            Velocity.X += tempVector.X;
            Velocity.Y += tempVector.Y;
            Velocity.Z += tempVector.Z;


            Circle post = Utilities.nearestPost(Position, Size);
            Vector positionDifference = tempVector;
            while ((Position - post.Center).Radius < post.Radius)
            {
                positionDifference.X = Position.X - post.Center.X;
                positionDifference.Y = Position.Y - post.Center.Y;
                positionDifference.Z = Position.Z - post.Center.Z;
                if (positionDifference.X == 0.0f && positionDifference.Y == 0.0f)
                {
                    positionDifference = Vector.fromPolar(post.Radius,
                                               (float)Utilities.CustomRandom.NextDouble(-Math.PI, Math.PI));
                }
                else
                {
                    positionDifference.normalize(post.Radius);
                }

                Position.X = post.Center.X + positionDifference.X;
                Position.Y = post.Center.Y + positionDifference.Y;
                Position.Z = post.Center.Z + positionDifference.Z;

                while ((Position - post.Center).Radius < post.Radius)
                {
                    // noise keeps it inside the post, move it a bit further out
                    positionDifference.normalize((float)(positionDifference.Radius * 1.01));
                    Position.X = post.Center.X + positionDifference.X;
                    Position.Y = post.Center.Y + positionDifference.Y;
                    Position.Z = post.Center.Z + positionDifference.Z;
                }
                Vector positionToCenter = new Vector(0.0f, 0.0f);
                if (objectVelocity.X != 0.0 || objectVelocity.Y != 0.0)
                {
                    positionToCenter.X = post.Center.X - Position.X;
                    positionToCenter.Y = post.Center.Y - Position.Y;
                    positionToCenter.Z = post.Center.Z - Position.Z;
                    objectVelocity.rotate(-(float)positionToCenter.Theta);
                    objectVelocity.X = -objectVelocity.X;
                    objectVelocity.rotate((float)positionToCenter.Theta);
                }

                post = Utilities.nearestPost(Position, Size);

                collidedWithPost();
            }

            Vector newPosition = tempVector;
            newPosition.X = Position.X + objectVelocity.X;
            newPosition.Y = Position.Y + objectVelocity.Y;
            newPosition.Z = Position.Z + objectVelocity.Z;

            Circle secondPost = Utilities.nearestPost(newPosition, Size);

            Vector intersectionPoint = new Vector(0, 0);

            bool second = false;

            Vector secondPositionDifference = new Vector(0.0f, 0f);
            Vector collisionToCircle = new Vector(0.0f, 0.0f);

            while (Position != newPosition &&
                ((Utilities.intersect(Position, newPosition, post, intersectionPoint)) ||
                 (post != secondPost &&
                 (second = Utilities.intersect(Position, newPosition, secondPost, intersectionPoint))
                              )
                         )
                     && intersectionPoint.Radius != 0)
            {

                Position = intersectionPoint;

                secondPositionDifference.X = newPosition.X - Position.X;
                secondPositionDifference.Y = newPosition.Y - Position.Y;
                secondPositionDifference.Z = newPosition.Z - Position.Z;

                if (second)
                {
                    collisionToCircle.X = secondPost.Center.X - Position.X;
                    collisionToCircle.Y = secondPost.Center.Y - Position.Y;
                    collisionToCircle.Z = secondPost.Center.Z - Position.Z;
                }
                else
                {

                    collisionToCircle.X = post.Center.X - Position.X;
                    collisionToCircle.Y = post.Center.Y - Position.Y;
                    collisionToCircle.Z = post.Center.Z - Position.Z;
                }
                tempVector = Vector.fromPolar(Defines.Epsilon, (float)(collisionToCircle.Theta + 180.0));
                Position.X += tempVector.X;
                Position.Y += tempVector.Y;
                Position.Z += tempVector.Z;

                secondPositionDifference.rotate(-(float)collisionToCircle.Theta);
                secondPositionDifference.X = -secondPositionDifference.X;
                secondPositionDifference.rotate((float)collisionToCircle.Theta);

                newPosition.X = Position.X + secondPositionDifference.X;
                newPosition.Y = Position.Y + secondPositionDifference.Y;
                newPosition.Z = Position.Z + secondPositionDifference.Z;
                post = Utilities.nearestPost(Position, Size);
                secondPost = Utilities.nearestPost(newPosition, Size);

                Velocity = Vector.fromPolar((float)objectVelocity.Radius, (float)secondPositionDifference.Theta);

                second = false;

                collidedWithPost();
            }

            Position = newPosition;
            Velocity.X *= Decay;
            Velocity.Y *= Decay;
            Velocity.Z *= Decay;
            Acceleration.X = 0.0f;
            Acceleration.Y = 0.0f;
            Acceleration.Z = 0.0f;
        }

        /// <summary>
        /// Moves object to a post collision position.
        /// </summary>
        internal void moveToCollisionPosition()
        {
            if (this.CollisionCount > 0)
            {
                Vector temporaryCollisionPosition = this.PostCollisionPosition / this.CollisionCount;
                this.PostCollisionPosition.assign(temporaryCollisionPosition.X, temporaryCollisionPosition.Y, temporaryCollisionPosition.Z);
                this.Position = this.PostCollisionPosition;

            }

            this.PostCollisionPosition.assign(0,0,0);
            this.CollisionCount = 0;
        }

        /// <summary>
        /// Sets parameters associated with the object.
        /// </summary>
        /// <param name="objectSize">Size of the object.</param>
        /// <param name="objectDecay">Decay of the object.</param>
        /// <param name="objectRandomness">Randomness of the object.</param>
        /// <param name="obejctWeight">Weight of the object.</param>
        /// <param name="objectMaximumSpeed">Maximum speed of the object.</param>
        /// <param name="objectMaximumAcceleration">Maxmimum acceleration of the object.</param>
        internal void setConstants(float objectSize, float objectDecay, float objectRandomness, float obejctWeight, float objectMaximumSpeed, float objectMaximumAcceleration)
        {
            Size = objectSize;
            Decay = objectDecay;
            Random = objectRandomness;
            Weight = obejctWeight;
            MaximumSpeed = objectMaximumSpeed;
            MaximumAcceleration = objectMaximumAcceleration;
        }

        #region Virtual Methods

        /// <summary>
        /// (Virtual) Turns the object.
        /// </summary>
        internal virtual void turnObject() { }

        /// <summary>
        /// (Virtual) Updates the angle of the object.
        /// </summary>
        internal virtual void updateAngle() { }

        /// <summary>
        /// (Virtual) Performs collided with post logic.
        /// </summary>
        internal virtual void collidedWithPost() { }

        /// <summary>
        /// (Virtual) Gets the maximum acceleration.
        /// </summary>
        /// <returns>Returns maximum acceleration.</returns>
        internal virtual float getMaximumAcceleration() { return 0.0f; }

        /// <summary>
        /// (Virtual) get the maximum speed.
        /// </summary>
        /// <returns>Returns the maximum speed.</returns>
        internal virtual float getMaximumSpeed() { return 0.0f; }

        #endregion

        #endregion

    }
}
