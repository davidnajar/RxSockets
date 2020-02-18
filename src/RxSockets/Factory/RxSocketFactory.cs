using Microsoft.Extensions.DependencyInjection;
using RxSockets.Abstractions;
using RxSockets.Factory;

namespace RxSockets
{
  public class RxSocketFactory
  {
    public static ICanSpecifySocketKind CreateSocket()
    {
      return  new CanSpecifySocketKind( new ServiceCollection());
    }
  }
}
