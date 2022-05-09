using RxSockets.Models;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace RxSockets.Abstractions
{
  public interface ISocket
  {
    Task StartAsync();

    Task StopAsync();

    void Start();

    void Stop();

        void Restart();
    IObservable<ReadOnlySequence<byte>> WhenMessageParsed { get; }
     
    IObservable<ConnectionStatus> WhenConnectionStatusChanged { get; }

    Task SendMessageAsync(ReadOnlySequence<byte> message);
  }
}
