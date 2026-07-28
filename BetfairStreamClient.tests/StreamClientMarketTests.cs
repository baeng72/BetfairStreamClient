using BetfairStreamClient.Logging;
using BetfairStreamClient.ExchangeStream;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests
{
    public class StreamClientMarketTests
    {
        private static StreamClient<T, TSnap> CreateTestClient<T, TSnap>(ITransportConnection transport) where T : struct, IDisposable, IClearable
    where TSnap : struct, IDisposable, IClearable
        {
            
            var mockLogger = new Mock<Logger>();
            
            var mockDumper = new Mock<RawStreamDumper>();
            var mockMarketCache = new Mock<MarketCacheManager<T, TSnap>>();
            var mockOrderCache = new Mock<OrderCacheManager>();
            var mockStreamParser = new Mock<StreamParser<T, TSnap>>(mockMarketCache.Object, mockOrderCache.Object, mockLogger.Object);
            return new StreamClient<T, TSnap>("stream-api.betfair.com", 443, "AppKey", "SessionToken",
                mockLogger.Object, mockDumper.Object, transport, mockMarketCache.Object, mockOrderCache.Object, mockStreamParser.Object);
        }
        [Fact]
        public async Task ChangeMarketsAsync_WhenNotConnected_ThrowsInvalidOperationException()
        {
            // Arrange: Build the client but never call ConnectAndAuthenticateAsync (activeStream stays null)
            var mockTransport = new Mock<ITransportConnection>();
            
            var client = CreateTestClient<MarketRunnerAt, MarketRunnerSnapAt>(mockTransport.Object);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                client.ChangeMarketsAsync(new List<string> { "1.12345" }, CancellationToken.None));

            Assert.Contains("Client is not connected", exception.Message);
        }
    }
}
