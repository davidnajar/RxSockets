using Microsoft.Extensions.DependencyInjection;
using RxSockets.Abstractions;
using RxSockets.Factory;

namespace RxSockets
{
  public class RxSocketBuilder
  {
    public static ICanSpecifySocketKind CreateSocket()
    {
      return  new CanSpecifySocketKind( new ServiceCollection());
    }
  }
}
