using Pipelines.Sockets.Unofficial;
using RxSockets.Abstractions;
using RxSockets.Exceptions;
using RxSockets.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace RxSockets
{
  public class TcpSocketServer : ISocket
  {
    private List<IDuplexPipe> _connectedClients = new List<IDuplexPipe>();
    private Socket _internalSocket;
    private EndPoint _endpoint;
    private TcpSocketServerSettings _settings;
    private readonly IParser _parser;
    private CancellationTokenSource _cancellationTokenSource;
    private ISubject<ReadOnlySequence<byte>> _whenMessageParsed;

    public IObservable<ReadOnlySequence<byte>> WhenMessageParsed
    {
      get
      {
        return _whenMessageParsed.AsObservable();
      }
    }

    private TcpSocketServer()
    {
      _cancellationTokenSource = new CancellationTokenSource();
      _whenMessageParsed = new Subject<ReadOnlySequence<byte>>();
    }

    public TcpSocketServer(TcpSocketServerSettings settings, IParser parser)
      : this()
    {
      _settings = settings;
      _parser = parser;
    }

    public void Start()
    {
      BuildSocket();
      _internalSocket.Bind(this._endpoint);
      _internalSocket.Listen(this._settings.MaxConnections);
      Task.Factory.StartNew(async () => await ConnectionHandler(_cancellationTokenSource.Token));
    }

        private async Task ConnectionHandler(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Socket socket = await this._internalSocket.AcceptAsync();
                IDuplexPipe pipe = StreamConnection.GetDuplex(new NetworkStream(socket), (PipeOptions)null, (string)null);
                _connectedClients.Add(pipe);
                Task.Factory.StartNew(async () => await ReadPipeAsync(pipe.Input, cancellationToken));

            }
        }

    public Task StartAsync()
    {
      return Task.Factory.StartNew(() => Start());
    }

    public void Stop()
    {
      _cancellationTokenSource.Cancel();
      _internalSocket.Shutdown(SocketShutdown.Both);
    }

    public Task StopAsync()
    {
      return Task.Factory.StartNew(() => this.Stop());
    }

    private async Task ProcessLinesAsync(Socket socket, CancellationToken cancellationToken)
    {
      Pipe pipe = new Pipe();
      Task writing = this.FillPipeAsync(socket, pipe.Writer, cancellationToken);
      Task reading = this.ReadPipeAsync(pipe.Reader, cancellationToken);
      await Task.WhenAll(reading, writing);
    }

        private async Task FillPipeAsync(Socket socket, PipeWriter writer, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Memory<byte> memory = writer.GetMemory(512);
                try
                {
                    int bytesRead = await socket.ReceiveAsync(((ReadOnlyMemory<byte>)memory).GetArray(), SocketFlags.None);
                    if (bytesRead != 0)
                    {
                        writer.Advance(bytesRead);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {

                    break;
                }
               var result = await writer.FlushAsync(cancellationToken);


                if (result.IsCompleted)
                {
                    break;
                }
            }
            writer.Complete();
        }

        private async Task ReadPipeAsync(PipeReader reader, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await reader.ReadAsync(cancellationToken);


                ReadOnlySequence<byte> buffer = result.Buffer;
                bool canParse = false;
                do
                {
                    canParse = _parser.CanParse(buffer);
                    if (canParse)
                    {
                        ParseResult messageParsed = await _parser.ProcessAsync(buffer);
                        _whenMessageParsed.OnNext(messageParsed.CleanMessage);
                        buffer = buffer.Slice(buffer.GetPosition(1, messageParsed.EndPosition));

                    }
                }
                while (canParse);
                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
            reader.Complete();
        }

        private void BuildSocket()
        {
            if (_internalSocket != null)
            {
                throw new SocketAlreadyStartedException("socket");
            }
            _internalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _endpoint = new IPEndPoint(_settings.Ip, _settings.Port);
        }

        public async Task SendMessageAsync(ReadOnlySequence<byte> message)
        {
         
            _connectedClients.ForEach(async p =>  await p.Output.WriteAsync(new ReadOnlyMemory<byte>(message.ToArray<byte>()), _cancellationTokenSource.Token));
        }
  }
}
