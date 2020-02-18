using System;
using System.Buffers;
using System.Threading.Tasks;

namespace RxSockets.Abstractions
{
  public interface IMessageFormatter<T>
  {
    Task<T> FormatMessageAsync(ReadOnlySequence<byte> buffer);

    Task<ReadOnlySequence<byte>> GetBytesAsync(T message);

    IObservable<T> WhenNewMessage { get; }
  }
}
