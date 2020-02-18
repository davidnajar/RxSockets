using RxSockets.Abstractions;

namespace RxSockets.Parsers.MessageParser
{
  public class MessageParserSettings : IParserSettings<MessageParser>
  {
    public byte[] StartDelimiter { get; set; }

    public byte[] EndDelimiter { get; set; }
  }
}
