
using RxSockets.Abstractions;
using RxSockets.Models;
using System;
using System.Buffers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxSockets
{
  public class RxSocket<TPayload> : IRxSocket<TPayload>
  {
    private readonly ISocket _socket;
    private readonly IParser _parser;
    private readonly IMessageFormatter<TPayload> _formatter;
    private ISubject<TPayload> _whenMessageReceived;
    private ISubject<TPayload> _whenMessageSent;
    private TaskPoolScheduler _factoryScheduler;

    public RxSocket(ISocket socket, IParser parser, IMessageFormatter<TPayload> formatter)
    {
      _socket = socket;
      _parser = parser;
      _formatter = formatter;
      
      _socket.WhenMessageParsed.Subscribe(bytes => _formatter.FormatMessageAsync(bytes));
      _whenMessageReceived = new Subject<TPayload>();
      _whenMessageSent =  new Subject<TPayload>();
      _factoryScheduler = new TaskPoolScheduler(Task.Factory);
      _formatter.WhenNewMessage.Subscribe( message => _whenMessageReceived.OnNext(message));
    }

    public IObservable<TPayload> WhenMessageReceived
    {
      get
      {
        return _whenMessageReceived.AsObservable().ObserveOn(_factoryScheduler);
      }
    }

    public IObservable<TPayload> WhenMessageSent
    {
      get
      {
        return _whenMessageSent.AsObservable().ObserveOn(_factoryScheduler);
      }
    }

    public IObservable<ConnectionStatus> WhenConnectionStatusChanged
    {
      get
      {
        return _socket.WhenConnectionStatusChanged.ObserveOn(_factoryScheduler);
      }
    }

    public IObservable<ReadOnlySequence<byte>> WhenMessageParsed
    {
      get
      {
        return _socket.WhenMessageParsed;
      }
    }

        public void Restart()
        {
            _socket.Restart();
        }

        public async Task SendAsync(TPayload payload)
    {
      ReadOnlySequence<byte> formattedMessage = await _formatter.GetBytesAsync(payload);

      ReadOnlySequence<byte> message = await _parser.PrepareMessageToBeSent(formattedMessage);

      
      await this._socket.SendMessageAsync(message);
    }

    public void Start()
    {
      _socket.Start();
    }

    public Task StartAsync()
    {
      return _socket.StartAsync();
    }

    public void Stop()
    {
      _socket.Stop();
    }

    public Task StopAsync()
    {
      return _socket.StopAsync();
    }
  }
}
