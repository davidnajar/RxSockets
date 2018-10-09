using RxSockets;
using System;

namespace RxSocketTestHarness
{
    class Program
    {
         static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var s = new RxSocket();
            s.Start();

            Console.ReadLine();
        }
    }
}
