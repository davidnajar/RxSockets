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

    IObservable<ReadOnlySequence<byte>> WhenMessageParsed { get; }

    Task SendMessageAsync(ReadOnlySequence<byte> message);
  }
}
