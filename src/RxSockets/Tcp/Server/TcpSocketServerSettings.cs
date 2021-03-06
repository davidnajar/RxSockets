﻿using RxSockets.Abstractions;
using System.Net;

namespace RxSockets
{
  public class TcpSocketServerSettings : ISocketSettings<TcpSocketServer>
  {
    public IPAddress Ip { get; set; }

    public short Port { get; set; }

    public int MaxConnections { get; set; }
  }
}
