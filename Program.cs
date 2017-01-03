using System;
using System.Net;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Console.WriteLine($"Server listening....\n{ipAddress}:11000");
            AsynchronousSocketListener.StartListening(localEndPoint);
        }
    }
}