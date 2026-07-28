using BetfairStreamClient.Logging;
using BetfairStreamClient.ExchangeStream;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetfairStreamClient.tests.ExchangeStream.Protocol;

namespace BetfairStreamClient.tests
{
    public class BetfairStreamFileReplayTests
    {
        
        
        [Fact]
        public async Task RunLoopAsync_WithRecordedFile_PopulatesCachesCorrectly()
        {
            // 1. Arrange: System components
            var marketCache = new MarketCacheManager<MarketRunnerAtTradedTVLTP, MarketRunnerSnapAtTradedTVLTP>();
            var orderCache = new OrderCacheManager();
            var logger = new Logger();
            var streamDumper = new RawStreamDumper();
            var streamParser = new StreamParser<MarketRunnerAtTradedTVLTP, MarketRunnerSnapAtTradedTVLTP>(marketCache, orderCache, logger);

            // 2. Setup the Two-Way Test Stream
            var networkInputPipe = new Pipe(); // We write file data here; client reads from here
            using var clientOutputMemory = new MemoryStream(); // Client writes JSON requests here

            var duplexStream = new DuplexTestStream(networkInputPipe.Reader.AsStream(), clientOutputMemory);

            var mockTransport = new Mock<ITransportConnection>();
            mockTransport.Setup(t => t.GetStream()).Returns(duplexStream); // Return the two-way stream!

            var client = new StreamClient<MarketRunnerAtTradedTVLTP, MarketRunnerSnapAtTradedTVLTP>(
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
            string filePath = "1_258926623.json";// Path.Combine("C:\\DATA\\BF\\BFRaceBot", "raw_stream-2026-07-22 02-48-22.json");

            //Setup ESA example RequestResponse processor to do comparison
            var requestResponseProcessor = new RequestResponseProcessor(new Action<string>(s => { Console.WriteLine(s); }));
            var clientCache = new ExchangeStream.ClientCache();
            requestResponseProcessor.ChangeHandler = clientCache;
            int lineCount = 0;
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (lineCount == 6)
                    {
                        int q = 0;
                    }
                    byte[] lineBytes = Encoding.UTF8.GetBytes(line + "\n");

                    // Note: We write to networkInputPipe.Writer now
                    await networkInputPipe.Writer.WriteAsync(lineBytes);
                    await networkInputPipe.Writer.FlushAsync();
                    //feed line into request response processor
                    requestResponseProcessor.ReceiveLine(line);
                    
                    await Task.Delay(10);
                    var snap = clientCache.MarketCache.Markets.FirstOrDefault(x => x.MarketId == "1.258926623").Snap;
                    var newSnap = marketCache.GetMarketSnap("1.258926623");
                    if (snap.MarketRunners.Count == newSnap.RunnerCount)
                    {
                        for (int i = 0; i < snap.MarketRunners.Count; i++)
                        {
                            var oldPrices = snap.MarketRunners[i].Prices;
                            var newPrices = newSnap.RunnerPrices[i].RunnerData;
                            int backCount = 0;
                            if (oldPrices.AvailableToBack.Count == newPrices.AvailableToBackCount)
                            {

                                for (int c = 0; c < oldPrices.AvailableToBack.Count; c++)
                                {
                                    var oldPrice = oldPrices.AvailableToBack[c];
                                    var newPrice = newPrices.AvailableToBack[c];
                                    if (oldPrice.Price == newPrice.Price && oldPrice.Size == newPrice.Size)
                                    {
                                        backCount++;
                                    }
                                    else
                                    {
                                        int x = 0;
                                    }
                                }
                            }
                            Assert.Equal(backCount, oldPrices.AvailableToBack.Count);
                            int layCount = 0;
                            if (oldPrices.AvailableToLay.Count == newPrices.AvailableToLayCount)
                            {

                                for (int c = 0; c < oldPrices.AvailableToLay.Count; c++)
                                {
                                    var oldPrice = oldPrices.AvailableToLay[c];
                                    var newPrice = newPrices.AvailableToLay[c];
                                    if (oldPrice.Price == newPrice.Price && oldPrice.Size == newPrice.Size)
                                    {
                                        layCount++;
                                    }
                                    else
                                    {
                                        int x = 0;
                                    }
                                }
                            }
                            Assert.Equal(layCount, oldPrices.AvailableToLay.Count);
                            int tradedCount = 0;
                            if (oldPrices.Traded.Count == newPrices.TradedCount)
                            {

                                for (int c = 0; c < oldPrices.Traded.Count; c++)
                                {
                                    var oldPrice = oldPrices.Traded[c];
                                    var newPrice = newPrices.Traded[c];
                                    if (oldPrice.Price == newPrice.Price && oldPrice.Size == newPrice.Size)
                                    {
                                        tradedCount++;
                                    }
                                    else
                                    {
                                        int x = 0;
                                    }
                                }
                            }
                            Assert.Equal(tradedCount, oldPrices.Traded.Count);
                        }
                    }
                    lineCount++;
                }
            }

            await Task.Delay(1200);

            // 5. Teardown
            cts.Cancel();
            await networkInputPipe.Writer.CompleteAsync();
            await runLoopTask;

            // 6. Assert

            var targetMarket = marketCache.GetMarketSnap("1.2589266233");
            var targetOrder = orderCache.GetOrderMarketSnap("1.258926623");

            Assert.NotNull(targetMarket);
            
        }
    }

}
