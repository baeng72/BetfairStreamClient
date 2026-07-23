using BetfairStreamClient.Logging;
using BetfairStreamClient.Stream;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests
{
    public class BetfairStreamFileReplayTests
    {
        
        
        [Fact]
        public async Task RunLoopAsync_WithRecordedFile_PopulatesCachesCorrectly()
        {
            // 1. Arrange: System components
            var marketCache = new MarketCacheManager();
            var orderCache = new OrderCacheManager();
            var logger = new Logger();
            var streamDumper = new RawStreamDumper();
            var streamParser = new StreamParser(marketCache, orderCache, logger);

            // 2. Setup the Two-Way Test Stream
            var networkInputPipe = new Pipe(); // We write file data here; client reads from here
            using var clientOutputMemory = new MemoryStream(); // Client writes JSON requests here

            var duplexStream = new DuplexTestStream(networkInputPipe.Reader.AsStream(), clientOutputMemory);

            var mockTransport = new Mock<ITransportConnection>();
            mockTransport.Setup(t => t.GetStream()).Returns(duplexStream); // Return the two-way stream!

            var client = new StreamClient(
                host: "://betfair.com", port: 443,
                appKey: "TEST_APP_KEY", sessionToken: "TEST_SESSION",
                logger, streamDumper,
                transportConnection: mockTransport.Object,
                marketCache: marketCache,
                orderCache: orderCache,
                streamParser: streamParser
            );

            var cts = new CancellationTokenSource();

            // 3. Act: Boot up infrastructure (This will now successfully write handshake JSON to clientOutputMemory!)
            await client.ConnectAndAuthenticateAsync(cts.Token);
            Task runLoopTask = client.RunLoopAsync(cts.Token);

            // 4. Replay: Stream file data into the networkInputPipe
            string filePath = Path.Combine("C:\\DATA\\BF\\BFRaceBot", "raw_stream-2026-07-22 02-48-22.json");

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    byte[] lineBytes = Encoding.UTF8.GetBytes(line + "\n");

                    // Note: We write to networkInputPipe.Writer now
                    await networkInputPipe.Writer.WriteAsync(lineBytes);
                    await networkInputPipe.Writer.FlushAsync();

                    await Task.Delay(10);
                }
            }

            await Task.Delay(1200);

            // 5. Teardown
            cts.Cancel();
            await networkInputPipe.Writer.CompleteAsync();
            await runLoopTask;

            // 6. Assert

            var targetMarket = marketCache.GetMarketSnap("1.260226043");
            var targetOrder = orderCache.GetMarketSnap("1.260226043");

            Assert.NotNull(targetMarket);
            Assert.Equal("1.260226043", targetMarket.Value.MarketId);
        }
    }

}
