using System;

namespace RxSockets.Exceptions
{
  public class SocketAlreadyStartedException : Exception
  {
    public SocketAlreadyStartedException(string socketName)
      : base("The socket " + socketName + " is already started. Dispose the ISocket before changing parameters")
    {
    }
  }
}
