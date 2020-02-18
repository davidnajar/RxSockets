namespace RxSockets.Abstractions
{
  public interface ICanSpecifySocketKind
  {
    ICanSpecifyParser WithSocketType<T>() where T : class, ISocket;

    ICanSpecifyParser WithSocketType<T>(ISocketSettings<T> settings) where T : class, ISocket;
  }
}
