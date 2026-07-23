using System;
using System.IO;

namespace BetfairStreamClient.ExchangeStream
{
    //For testing, use an interface
    public interface ITransportConnection : IDisposable
    {
        Task ConnectAndAuthenticateSslAsync(string host, int port, CancellationToken cancellationToken);
        System.IO.Stream GetStream();
    }
}
