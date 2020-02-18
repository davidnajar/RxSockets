using Microsoft.Extensions.DependencyInjection;
using RxSockets.Abstractions;

namespace RxSockets.Factory
{
  public class CanSpecifySocketKind : ICanSpecifySocketKind
  {
    private readonly IServiceCollection _collection;

    public CanSpecifySocketKind(IServiceCollection collection)
    {
      _collection = collection;
    }

    public ICanSpecifyParser WithSocketType<T>() where T : class, ISocket
    {
      _collection.AddSingleton<ISocket, T>();
      return  new CanSpecifyParser(_collection);
    }

    public ICanSpecifyParser WithSocketType<T>(ISocketSettings<T> settings) where T : class, ISocket
    {
      _collection.AddSingleton<ISocket, T>();
      _collection.Add(new ServiceDescriptor(settings.GetType(),  settings));
      return  new CanSpecifyParser(_collection);
    }
  }
}
