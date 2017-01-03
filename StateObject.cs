using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace SocketServer
{

    class StateReader
    {

        private BinaryReader reader;

        private Dictionary<int, string[]> bufferMap = new Dictionary<int, string[]>()
        {
            {1, new string[]{"u16", "u16" } },
            {2, new string[]{"u16", "u16", "u16", "u16" } }
        };
        public List<uint> data;

        public StateReader(StateObject state)
        {
            Stream stream = new MemoryStream(state.buffer);
            reader = new BinaryReader(stream);
            byte command = reader.ReadByte();
            string[] format = bufferMap[command];
            data = new List<uint>();
            data.Add(command);
            foreach (string formatter in format)
            {
                switch (formatter)
                {
                    case "u8":
                        data.Add(reader.ReadByte());
                        break;
                    case "u16":
                        data.Add(reader.ReadUInt16());
                        break;
                }
            }
        }
        public override string ToString()
        {
            return $"[{string.Join(", ", data)}]";
        }
    }

    class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
    }
}
