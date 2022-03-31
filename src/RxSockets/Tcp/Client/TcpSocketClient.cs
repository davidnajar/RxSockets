
using Pipelines.Sockets.Unofficial;
using RxSockets.Abstractions;
using RxSockets.Exceptions;
using RxSockets.Models;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RxSockets.Tcp.Client
{
    public class TcpSocketClient : ISocket
    {
        private IDuplexPipe _clientPipe;
        private Socket _internalSocket;
        private EndPoint _endpoint;
        private TcpSocketClientSettings _settings;
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

        private TcpSocketClient()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _whenMessageParsed = new Subject<ReadOnlySequence<byte>>();
        }

        public TcpSocketClient(TcpSocketClientSettings settings, IParser parser)
          : this()
        {
            _settings = settings;
            _parser = parser;
        }

        public void Start()
        {
            BuildSocket();
            Task.Factory.StartNew(async () => await ConnectionHandler(_cancellationTokenSource.Token));
        }

        private async Task ConnectionHandler(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _internalSocket.ConnectAsync(_endpoint);

                    
                    IDuplexPipe pipe = StreamConnection.GetDuplex(new NetworkStream(_internalSocket), (PipeOptions)null, (string)null);
                    _clientPipe = pipe;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Factory.StartNew(async () => await ReadPipeAsync(pipe.Input, cancellationToken));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    await CheckConnectionAsync(_internalSocket, cancellationToken);
                    _internalSocket.Shutdown(SocketShutdown.Both);
                    _internalSocket.Disconnect(true);

                }
                catch (SocketException sex)
                {


                }
                catch   (Exception ex)
                {

                }
                if (_settings.Reconnect)
                {
                    await Task.Delay(_settings.ReconnectionDelay);
                }
                else
                {
                    return;
                }
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
        private async Task CheckConnectionAsync(Socket socket, CancellationToken cancellationToken)
        {
            bool connected = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    connected = !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
                }
                catch (SocketException) { connected = false; }

                if (!connected)
                {
                    return;
                }

                await Task.Delay(1000, cancellationToken);
            }

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
            try
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
                            ParseResult messageParsed = await _parser.ParseAsync(buffer);
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
            catch (Exception ex)
            {

                throw;
            }
        }

        private void BuildSocket()
        {
            if (_internalSocket != null)
            {
                throw new SocketAlreadyStartedException("socket");
            }
            _internalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _internalSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _endpoint = new IPEndPoint(_settings.Ip, _settings.Port);
        }

        public async Task SendMessageAsync(ReadOnlySequence<byte> message)
        {
            if (_clientPipe != null)
            {
                await _clientPipe.Output.WriteAsync(new ReadOnlyMemory<byte>(message.ToArray<byte>()), _cancellationTokenSource.Token);
            }
        }

    }
}
