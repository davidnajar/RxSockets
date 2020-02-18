
namespace RxSockets.Abstractions
{
  public interface ICanBuildSocket<T>
  {
    IRxSocket<T> Build();
  }
}
