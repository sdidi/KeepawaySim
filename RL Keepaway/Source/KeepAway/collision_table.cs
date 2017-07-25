using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Keepaway
{
    [Serializable]
    public class collision_table
    {
        public int m;
        public int[] data;
        public int safe;
        public int calls;
        public int clearhits;
        public int collisions;

        public collision_table(int size, int safety)
        {
            Console.WriteLine("There is collision noted");  // troubleshooting display
            Console.WriteLine("example data retrieved is size ={0} and safety = {1} ",size,safety); 
            int tmp = size;
            while (tmp > 2)
            {
                if (tmp % 2 != 0)
                {
                    Console.WriteLine("\nSize of collision table must be power of {0}", size);
                }
                tmp /= 2;
            }
            data = new int[size];
            m = size;
            safe = safety;
            reset();
        }



        public void reset()
        {
            for (int i = 0; i < m; i++)
                data[i] = -1;
            calls = 0;
            clearhits = 0;
            collisions = 0;
        }


        int usage()
        {
            int count = 0;
            for (int i = 0; i < m; i++)
                if (data[i] != -1)
                    count++;
            return count;
        }


        public void Save(string filename)
        {
            SaveLoad.SaveFile(this, filename);
        }


        public collision_table restore(string filename)
        {
            return SaveLoad.LoadFile(filename);
        }

    } 
}
