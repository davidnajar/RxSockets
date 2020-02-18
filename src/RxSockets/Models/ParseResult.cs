using System;
using System.Buffers;

namespace RxSockets.Models
{
  public class ParseResult
  {
    public ReadOnlySequence<byte> CleanMessage { get; set; }

    public SequencePosition EndPosition { get; set; }
  }
}
