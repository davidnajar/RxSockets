using RxSockets.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RxSockets.Tcp.Client
{
    public class TcpSocketClientSettings : ISocketSettings<TcpSocketClient>
    {
        public IPAddress Ip { get; set; }

        public short Port { get; set; }

        public bool Reconnect { get; set; }
        public TimeSpan ReconnectionDelay { get; set; } = TimeSpan.FromSeconds(5);
    }
}
