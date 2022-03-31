using RxSockets.Models;
using System.Buffers;
using System.Threading.Tasks;

namespace RxSockets.Abstractions
{
  public interface IParser
  {
    bool CanParse(ReadOnlySequence<byte> bytes);

    ValueTask<ParseResult> ParseAsync(ReadOnlySequence<byte> bytes);

    ValueTask<ReadOnlySequence<byte>> PrepareMessageToBeSent(
      ReadOnlySequence<byte> bytes);
  }
}
