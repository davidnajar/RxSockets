namespace RxSockets.Abstractions
{
  public interface ICanSpecifyParser
  {
    ICanSpecifyFormatter WithParser<T>() where T : class, IParser;

    ICanSpecifyFormatter WithParser<T>(IParserSettings<T> settings) where T : class, IParser;
  }
}
