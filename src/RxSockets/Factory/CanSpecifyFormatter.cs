using Microsoft.Extensions.DependencyInjection;
using RxSockets.Abstractions;
using System;

namespace RxSockets.Factory
{
  public class CanSpecifyFormatter : ICanSpecifyFormatter
  {
    private readonly IServiceCollection _collection;
 

    public CanSpecifyFormatter(IServiceCollection collection)
    {
      _collection = collection;
    }

    public ICanBuildSocket<TPayload> WithFormatter<TPayload, TFormatter>() where TFormatter : class, IMessageFormatter<TPayload>
    {
      _collection.AddSingleton<IMessageFormatter<TPayload>, TFormatter>();
      return new CanBuildSocket<TPayload>(this._collection);
    }
  }
}
