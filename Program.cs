using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LANHelper;
using System.Threading.Tasks;

namespace LANHelperTest
{
    class Program
    {
        static void Main(string[] args)
        {
            LAN.Init();

            LAN.On("testLAN", TestLAN);

            LAN.Start("testLAN", false);
                LAN.WriteString("Hello LANHelper");
                LAN.WriteBytes(new byte[] { 1, 2, 3 });
                LAN.WriteInt(100);
                LAN.WriteFloat(float.MinValue);
                LAN.WriteFloats(new float[] { 0.1f, 0.2f, 0.3f });
                LAN.WriteInts(new int[] { 4, 5, 6 });
            LAN.Broadcast();

            LAN.On("SimpleChat", SimpleChat);

            bool looping = true;
            while (looping)
            {
                string line = Console.ReadLine();
                if (line == "") looping = false;

                LAN.Start("SimpleChat", false);
                LAN.WriteString(line);
                LAN.Broadcast();
            }
        }

        static void TestLAN()
        {
            string test = LAN.ReadString();
            byte[] test2 = LAN.ReadBytes();
            int test3 = LAN.ReadInt();
            float test4 = LAN.ReadFloat();
            float[] test5 = LAN.ReadFloats();
            int[] test6 = LAN.ReadInts();

            Console.WriteLine(test);
            Console.WriteLine("{0}, {1}, {2}", test2[0], test2[1], test2[2]);
            Console.WriteLine(test3);
            Console.WriteLine(test4);
            Console.WriteLine("{0}, {1}, {2}", test5[0], test5[1], test5[2]);
            Console.WriteLine("{0}, {1}, {2}", test6[0], test6[1], test6[2]);
        }

        static void SimpleChat()
        {
            string message = LAN.ReadString();

            Console.WriteLine(message);
        }
    }
}
