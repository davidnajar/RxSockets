using System;
using System.Buffers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxSockets.Abstractions
{
  public abstract class BaseMessageFormatter<TPayload> : IMessageFormatter<TPayload>
  {
    protected ISubject<TPayload> _whenMessageReceived;

    public IObservable<TPayload> WhenNewMessage
    {
      get
      {
        return _whenMessageReceived.AsObservable();
      }
    }

    public BaseMessageFormatter()
    {
      _whenMessageReceived = new Subject<TPayload>();
    }

    public abstract ValueTask<TPayload> FormatMessageAsync(ReadOnlySequence<byte> buffer);

    public abstract ValueTask<ReadOnlySequence<byte>> GetBytesAsync(TPayload message);
  }
}
