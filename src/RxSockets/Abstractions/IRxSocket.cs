using RxSockets.Models;
using System;
using System.Threading.Tasks;

namespace RxSockets.Abstractions
{
  public interface IRxSocket<T>
  {
    IObservable<T> WhenMessageReceived { get; }

    IObservable<T> WhenMessageSent { get; }

    Task SendAsync(T payload);

    IObservable<ConnectionStatus> WhenConnectionStatusChanged { get; }

    void Start();

    void Stop();
  }
}
