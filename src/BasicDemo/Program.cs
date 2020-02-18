using System;
using System.Net;
using RxSockets;
using RxSockets.Formatters;
using RxSockets.Parsers.MessageParser;
using RxSockets.Parsers;

namespace BasicDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var socket = RxSocketFactory.CreateSocket()
                .WithSocketType<TcpSocketServer>(new TcpSocketServerSettings()
                {
                    Ip = IPAddress.Any,
                    MaxConnections = 1,
                    Port = 2145
                })
                .WithParser<MessageParser>(new MessageParserSettings()
                {
                    EndDelimiter= new[] { (byte)(03), (byte)(13) },
                    StartDelimiter =new[] { (byte)(02) }
                })
                .WithFormatter<string, AsciiFormatter>()
                .Build();

            socket.WhenMessageReceived.Subscribe(message =>
            {

                Console.WriteLine(message);
            });
            socket.Start();

            Console.WriteLine("Press :q + enter to stop");

          
            bool quit = false;
            while (!quit)
            {
                var line = Console.ReadLine();
                if (line == ":q")
                {
                    quit = true;
                    break;
                }
                socket.SendAsync(line);
            }

            socket.Stop();
        }
    }
}
