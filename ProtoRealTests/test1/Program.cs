using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Protobuf;
using HFAN.Module.Static;

namespace test1
{
    class Program
    {

  
        static void Main(string[] args)
        {

   

            Person john11 = new Person
            {
                Id = 34,
                Name = "John",
                Email = "EmailEEEEE",
                Points = new Google.Protobuf.Collections.RepeatedField<double>() { 45.90, 534, 7.765, 5, 90, 94.8 }


            };

            Person john22 = new Person
            {
                Id = 14,
                Name = "Mat",
                Email = "EmailEEEEE",
                Points = new Google.Protobuf.Collections.RepeatedField<double>() { 45.90, 90, 9, 94.8 },
                Subscribers = new Google.Protobuf.Collections.RepeatedField<Person>() {john11 }

            };



            Person john1 = new Person
            {
                Id = 34,
                Name = "John",
                Email = "EmailEEEEE",
                Points = new Google.Protobuf.Collections.RepeatedField<double>() { 45.90, 534, 94.8, 98.675 },
                Subscribers = new Google.Protobuf.Collections.RepeatedField<Person>() { john22, john11 }

            };

            Person john2 = new Person
            {
                Id = 14,
                Name = "Mat",
                Email = "EmailEEEEE",
                Points = new Google.Protobuf.Collections.RepeatedField<double>() { 45.90, 534.90, 5, 90, 9, 94.8 },
                Subscribers = new Google.Protobuf.Collections.RepeatedField<Person>() { john1, john22 }

            };




            using (Stream file = File.OpenWrite(@"C:\Users\Mateusz\Desktop\ProtoTestData\data1.dat"))
            {
                file.Write(john1.ToByteArray(), 0, john1.ToByteArray().Length);
            }

            using (Stream file = File.OpenWrite(@"C:\Users\Mateusz\Desktop\ProtoTestData\data2.dat"))
            {
                file.Write(john2.ToByteArray(), 0, john2.ToByteArray().Length);
            }
            Console.Read();

        }
    }

}
