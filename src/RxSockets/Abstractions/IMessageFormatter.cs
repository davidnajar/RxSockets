using System;
using System.Buffers;
using System.Threading.Tasks;

namespace RxSockets.Abstractions
{
  public interface IMessageFormatter<T>
  {
    ValueTask<T> FormatMessageAsync(ReadOnlySequence<byte> buffer);

    ValueTask<ReadOnlySequence<byte>> GetBytesAsync(T message);

    IObservable<T> WhenNewMessage { get; }
  }
}
