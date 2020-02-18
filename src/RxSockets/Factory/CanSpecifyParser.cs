using Microsoft.Extensions.DependencyInjection;
using RxSockets.Abstractions;

namespace RxSockets.Factory
{
  public class CanSpecifyParser : ICanSpecifyParser
  {
    private readonly IServiceCollection _collection;

    public CanSpecifyParser(IServiceCollection collection)
    {
      _collection = collection;
    }

    public ICanSpecifyFormatter WithParser<T>() where T : class, IParser
    {
      _collection.AddSingleton<IParser, T>();
      return  new CanSpecifyFormatter(_collection);
    }

    ICanSpecifyFormatter ICanSpecifyParser.WithParser<T>(
      IParserSettings<T> settings)
    {
     _collection.AddSingleton<IParser, T>();
      _collection.Add(new ServiceDescriptor(settings.GetType(),  settings));
      return  new CanSpecifyFormatter(_collection);
    }
  }
}
