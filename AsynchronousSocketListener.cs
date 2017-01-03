using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketServer
{
    class AsynchronousSocketListener
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static Dictionary<IntPtr, Socket> connections = new Dictionary<IntPtr, Socket>();

        public static void StartListening(IPEndPoint localEndPoint)
        {
            byte[] bytes = new Byte[1024];


            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            Console.WriteLine($"New Connection as ({handler.Handle})");
            state.workSocket = handler;
            connections.Add(handler.Handle, handler);
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            List<uint> content;
            int bytesRead = 0;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (Exception e) {
                DisconnectClient(handler);
            }

            if (bytesRead > 0)
            {
                StateReader reader = new StateReader(state);
                content = reader.data;
                if (content.Count > 1)
                {
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Count, reader);

                    foreach (KeyValuePair<IntPtr, Socket> client in connections)
                    {
                        if (client.Key != handler.Handle)
                          Send(client.Value, content);
                    }
                    state = new StateObject();
                    state.workSocket = handler;

                }
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }

        private static void Send(Socket handler, List<uint> data)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            foreach (var dataPoint in data)
            {
                bw.Write(dataPoint);
            }
            byte[] byteData = ms.ToArray();
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void DisconnectClient(Socket handler)
        {
            Console.WriteLine($"Client {handler.Handle} Disconnected");
            connections.Remove(handler.Handle);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes to client. {handler.Handle}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
