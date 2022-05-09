using RxSockets;
using RxSockets.Formatters;
using RxSockets.Parsers.MessageParser;
using RxSockets.Tcp.Client;
using System;
using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace BasicDemo
{
    class Program
    {
        static void Main(string[] args)
        {


            var socketBuilder = RxSocketBuilder.CreateSocket()
              .WithSocketType<TcpSocketClient>(new TcpSocketClientSettings()
              {
                  Ip = IPAddress.Loopback,
                  Reconnect = true,
                  ReconnectionDelay = TimeSpan.FromSeconds(10),
                  Port = 2145
              })
              .WithParser<MessageParser>(new MessageParserSettings()
              {
                  EndDelimiter = new[] { (byte)(03), (byte)(13) },
                  StartDelimiter = new[] { (byte)(02) }
              })
              .WithFormatter<string, AsciiFormatter>();

            var client = socketBuilder.Build();
             

            client.WhenMessageReceived.Subscribe(message =>
            {

                Console.WriteLine($"Client < {message}");
            });
            client.WhenConnectionStatusChanged.Subscribe(status =>
            {
                Console.WriteLine($"Client connection status: {Enum.GetName(status.State)}");
            });





                Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe((_) => Debugger.Break());

            IDisposable subscription = client.WhenMessageReceived
                 .Merge(client.WhenConnectionStatusChanged
                     .Where(ot => ot.State == RxSockets.Models.State.Connected)
                     .Select(ot => ot.State.ToString()))
                 .Throttle(TimeSpan.FromSeconds(45))
                 .Subscribe(async (_) => {

                     client.Restart();

                
                    
                 }) ;

            client.Start();
            
            var server = RxSocketBuilder.CreateSocket()
                .WithSocketType<TcpSocketServer>(
                  new TcpSocketServerSettings(){
                      Ip = IPAddress.Any, 
                      Port = 2145,
                      MaxConnections=1
                      
                      }).WithParser<MessageParser>(new MessageParserSettings()
                      {
                          EndDelimiter = new[] { (byte)(03), (byte)(13) },
                          StartDelimiter = new[] { (byte)(02) }
                      })
              .WithFormatter<string, AsciiFormatter>()
              .Build();

            server.WhenMessageReceived.Subscribe(message =>
            {

                Console.WriteLine($"Server < {message}");
            });

            server.WhenConnectionStatusChanged.Subscribe(status =>
            {
                Console.WriteLine($"Server connection status: {Enum.GetName(status.State)}");
            });
            server.Start();
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await client.SendAsync(DateTime.Now.ToString());
                    await Task.Delay(1500);
                }
            });
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
                client.SendAsync(line);
            }

            client.Stop();
        }
    }
}
