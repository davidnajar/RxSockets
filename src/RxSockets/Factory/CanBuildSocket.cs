using Microsoft.Extensions.DependencyInjection;
using RxSockets.Abstractions;
using System;

namespace RxSockets.Factory
{
  public class CanBuildSocket<T> : ICanBuildSocket<T>
  {
    private IServiceCollection _collection;

    public CanBuildSocket(IServiceCollection collection)
    {
      this._collection = collection;
      this._collection.AddSingleton<IRxSocket<T>, RxSocket<T>>();
    }

    public IRxSocket<T> Build()
    {
      return  _collection.BuildServiceProvider().GetRequiredService<IRxSocket<T>>();
    }
  }
}
