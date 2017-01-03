using System;



namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server listening....");
            AsynchronousSocketListener.StartListening();
        }
    }
}