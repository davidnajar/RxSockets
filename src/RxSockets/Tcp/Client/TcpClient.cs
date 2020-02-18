
using RxSockets.Abstractions;
using System;
using System.Buffers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxSockets.Tcp.Client
{
  public class TcpClient : ISocket
  {
    private readonly TcpSocketServerSettings _settings;
    private ISubject<ReadOnlySequence<byte>> _whenMessageParsed;

    public IObservable<ReadOnlySequence<byte>> WhenMessageParsed
    {
      get
      {
        return this._whenMessageParsed.AsObservable<ReadOnlySequence<byte>>();
      }
    }

    public Task SendMessageAsync(ReadOnlySequence<byte> message)
    {
      throw new NotImplementedException();
    }

    public void Start()
    {
      throw new NotImplementedException();
    }

    public Task StartAsync()
    {
      throw new NotImplementedException();
    }

    public void Stop()
    {
      throw new NotImplementedException();
    }

    public Task StopAsync()
    {
      throw new NotImplementedException();
    }
  }
}
