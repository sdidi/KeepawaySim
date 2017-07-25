using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Keepaway
{
    public static class SaveLoad
    {
        public static void SaveFile(collision_table ct,string filename)
        {
            IFormatter bf = new BinaryFormatter();
            Stream stream = new FileStream(filename,FileMode.Create,FileAccess.Write, FileShare.None);
            bf.Serialize(stream, ct);
            stream.Close();
            Console.WriteLine("Collision data saves");
           

        }

        public static collision_table LoadFile(string fileToLoad)
        {
            if (File.Exists(fileToLoad)) {
                IFormatter bf = new BinaryFormatter();
                Stream stream = new FileStream(fileToLoad,FileMode.Open, FileAccess.Read, FileShare.Read);
                collision_table ct = (collision_table)bf.Deserialize(stream);
                stream.Close();

                Console.WriteLine("Collision data saves");
                Console.WriteLine("example data retrieved is {0}", ct.m);

                return ct;
              } else { return null; }
            
        }

     }

 }

        

    
