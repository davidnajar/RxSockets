using RxSockets.Abstractions;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;

namespace RxSockets.Formatters
{
  public class AsciiFormatter : BaseMessageFormatter<string>
  {
    public override Task<string> FormatMessageAsync(ReadOnlySequence<byte> buffer)
    {
      string result = Encoding.ASCII.GetString(buffer.ToArray());
      _whenMessageReceived.OnNext(result);
      return Task.FromResult(result);
    }

    public override Task<ReadOnlySequence<byte>> GetBytesAsync(string message)
    {
      return Task.FromResult(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes(message)));
    }
  }
}
