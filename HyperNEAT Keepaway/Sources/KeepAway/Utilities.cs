
/***************************************************************************
                                   FixedKeepawayPlayer.cs
                    Fixed policy kepaway player for Keepaway domain experiments.
                             -------------------
    begin                : JUL-2009
    credit               : Phillip Verbancsics of the
                            Evolutionary Complexity Research Group
     email                : verb@cs.ucf.edu

  
 Based On: 
/*

Copyright (c) 2004 Gregory Kuhlmann, Peter Stone
University of Texas at Austin
All rights reserved.
 
 Base On:

Copyright (c) 2000-2003, Jelle Kok, University of Amsterdam
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

3. Neither the name of the University of Amsterdam nor the names of its
contributors may be used to endorse or promote products derived from this
software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keepaway
{
    internal static class Utilities
    {
        private static Utilities.Random rnd;

        public static int Seed { get; set; }

        public static Utilities.Random Rnd
        {
            get
            {
                if (Utilities.rnd == null)
                {
                    Utilities.Seed = (int)DateTime.Now.Ticks;
                    Utilities.rnd = new Utilities.Random((uint)Utilities.Seed);
                }
                return Utilities.rnd;
            }
        }

        public class Random
        {
            private static uint[] mag01 = new uint[2]
      {
        0U,
        2567483615U
      };
            private uint[] mt = new uint[624];
            private double z2 = double.NaN;
            private const int N = 624;
            private const int M = 397;
            private const uint MATRIX_A = 2567483615U;
            private const uint UPPER_MASK = 2147483648U;
            private const uint LOWER_MASK = 2147483647U;
            private const uint TEMPERING_MASK_B = 2636928640U;
            private const uint TEMPERING_MASK_C = 4022730752U;
            private short mti;

            public Random(uint seed)
            {
                this.mt[0] = seed & uint.MaxValue;
                for (this.mti = (short)1; (int)this.mti < 624; ++this.mti)
                    this.mt[(int)this.mti] = (uint)(69069 * (int)this.mt[(int)this.mti - 1] & -1);
            }

            public Random()
                : this(4357U)
            {
            }

            private static uint TEMPERING_SHIFT_U(uint y)
            {
                return y >> 11;
            }

            private static uint TEMPERING_SHIFT_S(uint y)
            {
                return y << 7;
            }

            private static uint TEMPERING_SHIFT_T(uint y)
            {
                return y << 15;
            }

            private static uint TEMPERING_SHIFT_L(uint y)
            {
                return y >> 18;
            }

            protected uint GenerateUInt()
            {
                if ((int)this.mti >= 624)
                {
                    short num1;
                    for (num1 = (short)0; (int)num1 < 227; ++num1)
                    {
                        uint num2 = (uint)((int)this.mt[(int)num1] & int.MinValue | (int)this.mt[(int)num1 + 1] & int.MaxValue);
                        this.mt[(int)num1] = this.mt[(int)num1 + 397] ^ num2 >> 1 ^ Utilities.Random.mag01[(num2 & 1U)];//this.mt[(int)num1] = this.mt[(int)num1 + 397] ^ num2 >> 1 ^ Utilities.Random.mag01[(IntPtr)(num2 & 1U)];
                    }
                    for (; (int)num1 < 623; ++num1)
                    {
                        uint num2 = (uint)((int)this.mt[(int)num1] & int.MinValue | (int)this.mt[(int)num1 + 1] & int.MaxValue);
                        this.mt[(int)num1] = this.mt[(int)num1 - 227] ^ num2 >> 1 ^ Utilities.Random.mag01[(num2 & 1U)];//this.mt[(int)num1] = this.mt[(int)num1 - 227] ^ num2 >> 1 ^ Utilities.Random.mag01[(IntPtr)(num2 & 1U)];
                    }
                    uint num3 = (uint)((int)this.mt[623] & int.MinValue | (int)this.mt[0] & int.MaxValue);
                    this.mt[623] = this.mt[396] ^ num3 >> 1 ^ Utilities.Random.mag01[(num3 & 1U)];//this.mt[623] = this.mt[396] ^ num3 >> 1 ^ Utilities.Random.mag01[(IntPtr)(num3 & 1U)];
                    this.mti = (short)0;
                }
                uint y1 = this.mt[(int)this.mti++];
                uint y2 = y1 ^ Utilities.Random.TEMPERING_SHIFT_U(y1);
                uint y3 = y2 ^ Utilities.Random.TEMPERING_SHIFT_S(y2) & 2636928640U;
                uint y4 = y3 ^ Utilities.Random.TEMPERING_SHIFT_T(y3) & 4022730752U;
                return y4 ^ Utilities.Random.TEMPERING_SHIFT_L(y4);
            }

            public virtual uint NextUInt()
            {
                return this.GenerateUInt();
            }

            public virtual uint NextUInt(uint maxValue)
            {
                return (uint)((double)this.GenerateUInt() / (4294967296.0 / (double)maxValue));
            }

            public virtual uint NextUInt(uint minValue, uint maxValue)
            {
                if (minValue >= maxValue)
                    throw new ArgumentOutOfRangeException();
                return (uint)((double)this.GenerateUInt() / (4294967296.0 / (double)(maxValue - minValue)) + (double)minValue);
            }

            public int Next()
            {
                return this.Next(int.MaxValue);
            }

            public int Next(int maxValue)
            {
                if (maxValue > 1)
                    return (int)(this.NextDouble() * (double)maxValue);
                if (maxValue < 0)
                    throw new ArgumentOutOfRangeException();
                return 0;
            }

            public int Next(int minValue, int maxValue)
            {
                if (maxValue < minValue)
                    throw new ArgumentOutOfRangeException();
                if (maxValue == minValue)
                    return minValue;
                return this.Next(maxValue - minValue) + minValue;
            }

            public void NextBytes(byte[] buffer)
            {
                int length = buffer.Length;
                if (buffer == null)
                    throw new ArgumentNullException();
                for (int index = 0; index < length; ++index)
                    buffer[index] = (byte)this.Next(256);
            }

            public double NextDouble()
            {
                return (double)this.GenerateUInt() / 4294967296.0;
            }

            public double NextDouble(double min, double max)
            {
                if (min > max)
                {
                    double num = min;
                    min = max;
                    max = num;
                }
                return min + this.NextDouble() * (max - min);
            }

            public double NextGuassian(double dir, double sigma)
            {
                if (!double.IsNaN(this.z2))
                {
                    double num = dir + sigma * this.z2;
                    this.z2 = double.NaN;
                    return num;
                }
                double num1 = this.NextDouble();
                double num2 = this.NextDouble();
                double num3 = Math.Sqrt(-2.0 * Math.Log(num1 + 4.94065645841247E-324)) * Math.Sin(2.0 * Math.PI * num2);
                this.z2 = Math.Sqrt(-2.0 * Math.Log(num1 + 4.94065645841247E-324)) * Math.Cos(2.0 * Math.PI * num2);
                return dir + num3 * sigma;
            }
        }
    }
}
