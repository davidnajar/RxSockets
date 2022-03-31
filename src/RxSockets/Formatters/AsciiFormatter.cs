using RxSockets.Abstractions;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;

namespace RxSockets.Formatters
{
  public class AsciiFormatter : BaseMessageFormatter<string>
  {
    public override ValueTask<string> FormatMessageAsync(ReadOnlySequence<byte> buffer)
    {
      string result = Encoding.ASCII.GetString(buffer.ToArray());
      _whenMessageReceived.OnNext(result);
      return new ValueTask<string>(result);
    }

    public override ValueTask<ReadOnlySequence<byte>> GetBytesAsync(string message)
    {
      return new ValueTask<ReadOnlySequence<byte>>(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes(message)));
    }
  }
}
