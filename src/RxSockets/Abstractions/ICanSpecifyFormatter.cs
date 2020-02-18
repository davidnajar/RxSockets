namespace RxSockets.Abstractions
{
  public interface ICanSpecifyFormatter
  {
    ICanBuildSocket<TPayload> WithFormatter<TPayload, TFormatter>() where TFormatter : class, IMessageFormatter<TPayload>;
  }
}
