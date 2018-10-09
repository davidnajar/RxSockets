using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace RxSockets
{
    public class RxSocket
    {
        Socket socket;
        public RxSocket()
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("localhost", 9002);
        }
       public async Task Start()
        {
            await ProcessLinesAsync(socket);
        }
        async Task ProcessLinesAsync(Socket socket)
        {
            var pipe = new Pipe();
            Task writing = FillPipeAsync(socket, pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);

             await Task.WhenAll(reading, writing);
        }

        async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                try
                {
#if NETCOREAPP2_1
            int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
#else
                    int bytesRead = await socket.ReceiveAsync(memory.GetArray(), SocketFlags.None);
#endif
                   
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    // Tell the PipeWriter how much was read from the Socket
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                   // LogError(ex);
                    break;
                }

                // Make the data available to the PipeReader
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Tell the PipeReader that there's no more data coming
            writer.Complete();
        }

        async Task ReadPipeAsync(PipeReader reader)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync();

                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    // Look for a EOL in the buffer
                    position = buffer.PositionOf((byte)'\n');

                    if (position != null)
                    {
                        // Process the line
                        ProcessLine(buffer.Slice(0, position.Value));

                        // Skip the line + the \n character (basically position)
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while (position != null);

                // Tell the PipeReader how much of the buffer we have consumed
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete
            reader.Complete();
        }

        void ProcessLine(ReadOnlySequence<byte> bytes)
        {
            Console.WriteLine(bytes.ToArray().UnsafeAsciiBytesToString(0, (int)bytes.Length));
        }


    }
}
