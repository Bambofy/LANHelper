using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;

namespace LANHelper
{
    namespace Utilities
    {
        static class Bits
        {
            public static byte[] StringToBytes(string pString)
            {
                char[] strChars = pString.ToCharArray();
                byte[] data = new byte[strChars.Length * 2];
                for (int i = 0; i < strChars.Length; i++)
                {
                    byte[] charValue = BitConverter.GetBytes(strChars[i]);
                    if (BitConverter.IsLittleEndian) charValue.Reverse();

                    data[2 * i] = charValue[0];
                    data[2 * i + 1] = charValue[1];
                }
                return data;
            }

            public static byte[] IntsToBytes(int[] pInts)
            {
                byte[] data = new byte[pInts.Length * 4];
                for (int i = 0; i < pInts.Length; i++)
                {
                    byte[] intValue = BitConverter.GetBytes(pInts[i]);
                    if (BitConverter.IsLittleEndian) intValue.Reverse();
                    
                    data[4 * i] = intValue[0];
                    data[4 * i + 1] = intValue[1];
                    data[4 * i + 2] = intValue[2];
                    data[4 * i + 3] = intValue[3];
                }
                return data;
            }

            public static byte[] FloatsToBytes(float[] pFloats)
            {
                byte[] data = new byte[pFloats.Length * 4];
                for (int i = 0; i < pFloats.Length; i++)
                {
                    byte[] intValue = BitConverter.GetBytes(pFloats[i]);
                    if (BitConverter.IsLittleEndian) intValue.Reverse();

                    data[4 * i] = intValue[0];
                    data[4 * i + 1] = intValue[1];
                    data[4 * i + 2] = intValue[2];
                    data[4 * i + 3] = intValue[3];
                }
                return data;
            }

            public static byte[] FileToBytes(string pFilename)
            {
                List<byte> _bytes = new List<byte>();

                using (StreamReader sr = new StreamReader(pFilename))
                {
                    BinaryReader br = new BinaryReader(sr.BaseStream);

                    for (int i = 0; i < sr.BaseStream.Length; i++)
                    {
                        _bytes.Add(br.ReadByte());
                    }

                    br.Close();
                }

                return _bytes.ToArray();
            }

            public static string ByteArrayToString(byte[] pBytes, int pByteOffset)
            {
                int byteCount = pBytes.Length;
                char[] data = new char[byteCount / sizeof(char)];
                for (int i = 0; i < byteCount; i += sizeof(char))
                {
                    byte msb = pBytes[pByteOffset + i];
                    byte lsb = pBytes[pByteOffset + i + 1];
                    char value = BitConverter.ToChar(new byte[] { msb, lsb }, 0);
                    data[i / sizeof(char)] = value;
                }

                string rstring = "";
                foreach (char c in data) rstring += c;
                return rstring;
            }

            public static int[] ByteArrayToInts(byte[] pBytes)
            {
                int byteCount = pBytes.Length;
                int[] data = new int[byteCount / sizeof(int)];
                for (int i = 0; i < byteCount; i += sizeof(int))
                {
                    byte b1 = pBytes[i];
                    byte b2 = pBytes[i + 1];
                    byte b3 = pBytes[i + 2];
                    byte b4 = pBytes[i + 3];
                    int value = BitConverter.ToInt32(new byte[] { b1, b2, b3, b4 }, 0);
                    data[i / sizeof(int)] = value;
                }

                return data;
            }
            public static float[] ByteArrayToFloats(byte[] pBytes)
            {
                int byteCount = pBytes.Length;
                float[] data = new float[byteCount / sizeof(float)];
                for (int i = 0; i < byteCount; i += sizeof(float))
                {
                    byte b1 = pBytes[i];
                    byte b2 = pBytes[i + 1];
                    byte b3 = pBytes[i + 2];
                    byte b4 = pBytes[i + 3];
                    float value = BitConverter.ToSingle(new byte[] { b1, b2, b3, b4 }, 0);
                    data[i / sizeof(float)] = value;
                }

                return data;
            }
        }

        static class Networking
        {
            /// <summary>
            /// DO not close the application while this is running or else you will crash your computer.
            /// </summary>
            /// <param name="maxIpCount"></param>
            /// <returns></returns>
            public static List<IPAddress> GetLANAddresses(int maxIpCount = 10)
            {
                List<IPAddress> openIps = new List<IPAddress>();
                string baseIpAddress = "192.168.0.";

                Ping ping = new Ping();
                PingReply reply;

                for (int ipCount = 0; ipCount < maxIpCount; ipCount++)
                {
                    reply = ping.Send(baseIpAddress + ipCount);
                    if (reply.Status == IPStatus.Success)
                    {
                        openIps.Add(reply.Address);
                    }
                }
                return openIps;
            }

            public static IPAddress GetLocalAddress()
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip;
                    }
                }
                return IPAddress.Loopback;
            }
        }
    }

    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }

}
