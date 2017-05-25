using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace LANHelper
{
    public static class LAN
    {
        private static int _port;
        private static int _receiveWindowSize;
        private static Socket _listenSocket;
        private static int _lanComputerCount;

        private static Dictionary<string, Action> _hooks = new Dictionary<string, Action>();

        public static void Init(int pPort = 1337, int pLANComputerCount = 20, int pReceiveWindowSize = 262144)
        {
            _port = pPort;
            _receiveWindowSize = pReceiveWindowSize;
            _lanComputerCount = pLANComputerCount;

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 1337);
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _listenSocket.Bind(serverEndPoint);

            Thread t = new Thread(RunServer);
            t.Start();
        }

        private static byte[] _receivedBuffer;
        private static int _bufferHead;
        private static void RunServer()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)(sender);

            while (true)
            {
                // Receive the window.
                _receivedBuffer = new byte[_receiveWindowSize];
                int recv = _listenSocket.ReceiveFrom(_receivedBuffer, ref remote);

                // get identifier.
                _bufferHead = 0;
                string identifier = ReadString();
                _hooks[identifier]();
            }
        }

        public static byte[] ReadBytes()
        {
            int bytesLength = BitConverter.ToInt32(_receivedBuffer.SubArray(_bufferHead, sizeof(int)), 0);
            _bufferHead += sizeof(int);

            byte[] bytes = _receivedBuffer.SubArray(_bufferHead, bytesLength);
            _bufferHead += bytesLength;

            return bytes;
        }

        public static int ReadInt()
        {
            int intValue = BitConverter.ToInt32(_receivedBuffer.SubArray(_bufferHead, sizeof(int)), 0);
            _bufferHead += sizeof(int);

            return intValue;
        }

        public static int[] ReadInts()
        {
            int intsLength = BitConverter.ToInt32(_receivedBuffer.SubArray(_bufferHead, sizeof(int)), 0);
            _bufferHead += sizeof(int);

            int[] ints = Utilities.Bits.ByteArrayToInts(_receivedBuffer.SubArray(_bufferHead, intsLength));
            _bufferHead += intsLength;

            return ints;
        }

        public static float ReadFloat()
        {
            float floatValue = BitConverter.ToSingle(_receivedBuffer.SubArray(_bufferHead, sizeof(float)), 0);
            _bufferHead += sizeof(float);

            return floatValue;
        }

        public static float[] ReadFloats()
        {
            int floatsLength = BitConverter.ToInt32(_receivedBuffer.SubArray(_bufferHead, sizeof(float)), 0);
            _bufferHead += sizeof(float);

            float[] floats = Utilities.Bits.ByteArrayToFloats(_receivedBuffer.SubArray(_bufferHead, floatsLength));
            _bufferHead += floatsLength;

            return floats;
        }


        public static string ReadString()
        {
            int stringLength = BitConverter.ToInt32(_receivedBuffer.SubArray(_bufferHead, sizeof(int)), 0);
            _bufferHead += sizeof(int);

            string str = Utilities.Bits.ByteArrayToString(_receivedBuffer.SubArray(_bufferHead, stringLength), 0);
            _bufferHead += stringLength;

            return str;
        }

        public static void On(string pIdentifer, Action pCallback)
        {
            _hooks.Add(pIdentifer, pCallback);
        }


        private static List<byte> _data = new List<byte>();
        private static bool _excludeSelf;
        public static void Start(string pIdentifier, bool pExcludeSelf = true)
        {
            _data.Clear();
            _excludeSelf = pExcludeSelf;
            WriteString(pIdentifier);
        }

        public static void WriteBytes(byte[] pByte)
        {
            _data.AddRange(BitConverter.GetBytes(pByte.Length));
            _data.AddRange(pByte);
        }

        public static void WriteInt(int pInt)
        {
            _data.AddRange(BitConverter.GetBytes(pInt));
        }

        public static void WriteInts(int[] pInts)
        {
            _data.AddRange(BitConverter.GetBytes(pInts.Length * sizeof(int)));
            _data.AddRange(Utilities.Bits.IntsToBytes(pInts));
        }

        public static void WriteFloat(float pFloat)
        {
            _data.AddRange(BitConverter.GetBytes(pFloat));
        }

        public static void WriteFloats(float[] pFloats)
        {
            _data.AddRange(BitConverter.GetBytes(pFloats.Length * sizeof(float)));
            _data.AddRange(Utilities.Bits.FloatsToBytes(pFloats));
        }

        public static void WriteString(string pString)
        {
            _data.AddRange(BitConverter.GetBytes(pString.Length * sizeof(char)));
            _data.AddRange(Utilities.Bits.StringToBytes(pString));
        }

        public static void Broadcast()
        {
            string baseIp = "192.168.0.";
            List<string> ips = new List<string>();
            for (int i = 0; i < _lanComputerCount; i++)
            {
                string ip = baseIp + i.ToString();

                if (_excludeSelf)
                {
                    if (Utilities.Networking.GetLocalAddress().ToString() != ip)
                    {
                        ips.Add(ip);
                    }
                }
                else
                {
                    ips.Add(ip);
                }
            }

            foreach (string ip in ips)
            {
                try
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), _port);
                    Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    int senv = server.SendTo(_data.ToArray(), SocketFlags.None, remoteEndPoint);
                }
                catch (System.Net.Sockets.SocketException exception)
                {
                    throw exception;
                    continue;
                }
            }
        }
    }
}
