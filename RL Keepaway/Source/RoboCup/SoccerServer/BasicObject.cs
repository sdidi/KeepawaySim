// -*-C#-*-

/***************************************************************************
                                   BasicObject.cs
                    Class that represents a basic object in RoboCup.
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

namespace RoboCup.Objects
{
    /// <summary>
    /// Class that describes a basic object in the RoboCup server.
    /// </summary>
    public class BasicObject
    {

        #region Instance Variables
        
        /// <summary>
        /// ID number of the object
        /// </summary>
        public int Id;

        /// <summary>
        /// Name of the object
        /// </summary>
        public string Name;

        /// <summary>
        /// Radial size of the object.
        /// </summary>
        internal float Size;

        internal Vector currentPosition;

        /// <summary>
        /// Position of the object
        /// </summary>
        public Vector Position
        {
            get { return currentPosition; }
            set { currentPosition.X = value.X; currentPosition.Y = value.Y; currentPosition.Z = value.Z; }
        }

        /// <summary>
        /// is the object enabled?
        /// </summary>
        internal bool Enable;

        #endregion

        #region Static Variables

        /// <summary>
        /// The number of objects created so far.
        /// </summary>
        static int ObjectCount = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor of the BasicObject class.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        /// <param name="currentPosition">Current position of the object.</param>
        public BasicObject(string name, Vector currentPosition)
        {
            Id = ObjectCount;
            Name = name;
            Size = 1.0f;
            this.currentPosition = new Vector(0,0);
            Position = currentPosition;
            Enable = true;
            ++ObjectCount;
        }

        /// <summary>
        /// Constructor of the BasicObject class.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        public BasicObject(string name)
        {
            Id = ObjectCount;
            Name = name;


            Size = 1.0f;
            currentPosition = new Vector(0,0);
            Enable = true;
            ++ObjectCount;


        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Produces a string representation of the object.
        /// </summary>
        /// <returns>Returns a string #Ob[ID:Name:pos=PosString,size=Size].</returns>
        public override string ToString()
        {
            string objectString = "#Ob[" + Id;
            if (Name != "")
            {
                objectString += ":" + Name + "";
            }
            return objectString + ":pos=" + Position.ToString() + ",size=" + Size + "]";
        }


        #endregion
    }
}
