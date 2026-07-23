using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BetfairStreamClient.ExchangeStream
{
    public class BetfairTransportConnection : ITransportConnection
    {
        private readonly TcpClient _tcpClient = new();
        private SslStream? _sslStream;

        public async Task ConnectAndAuthenticateSslAsync(string host, int port, CancellationToken cancellation)
        {
            await _tcpClient.ConnectAsync(host, port, cancellation);
            var rawStream = _tcpClient.GetStream();

            _sslStream = new SslStream(rawStream, false);

            await _sslStream.AuthenticateAsClientAsync(host, null, SslProtocols.Tls12 | SslProtocols.Tls13, true);
        }

        public System.IO.Stream GetStream() => _sslStream ?? throw new InvalidOperationException("Not connected");

        public void Dispose()
        {
            _sslStream?.Dispose();
            _tcpClient?.Dispose();
        }
    }
}
