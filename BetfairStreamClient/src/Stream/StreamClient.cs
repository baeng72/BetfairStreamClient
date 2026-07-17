using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BetfairStreamClient.Logging;

namespace BetfairStreamClient.Stream
{
    public class StreamClient
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _appKey;
        private readonly string _sessionToken;
        
        
        private long _lastMessageTicks = DateTime.UtcNow.Ticks;
        private int _heartbeatIdCounter = 1000; // Distinct ID range for heartbeats
        private ConcurrentDictionary<string, long> runnerMap = new ConcurrentDictionary<string, long>();
        //Semaphore prevents concurrent writes to the newtork socket from different threads
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        private Logger _logger;
        private RawStreamDumper _streamDumper;
        private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;
        private readonly Channel<(byte[] Buffer, int Length)> _messageChannel = Channel.CreateUnbounded<(byte[], int)>(
                        new UnboundedChannelOptions { SingleWriter = true, SingleReader = true });

        private readonly StreamParser _streamParser;   //shift parsing to separate class

        private readonly MarketCacheManager _marketCache;

        private readonly OrderCacheManager _orderCache;
        private readonly TcpClient tcpClient = new TcpClient();
        private SslStream sslStream;
        private PipeReader pipeReader;
        private int _nextId;
        private Task _processingTask;
        private Task _heartbeatTask;
        



        public StreamClient(string host, int port, string appKey, string sessionToken, Logger logger, RawStreamDumper streamDumper)
        {
            _host = host;
            _port = port;
            _appKey = appKey;
            _sessionToken = sessionToken;

            _marketCache = new MarketCacheManager();
            _orderCache = new OrderCacheManager();
            _streamParser = new StreamParser(_marketCache, _orderCache, logger);
            _logger = logger;
            _streamDumper = streamDumper;
            _nextId = 0;
            
        }

        private int NextId()
        {
            int id = Interlocked.Increment(ref _nextId);
            return id;
        }

        // Notice we no longer pass initial marketIds here; it simply opens the connection infrastructure

        public async Task ConnectAndAuthenticateAsync(CancellationToken cancellationToken)
        {
            _logger.Log("[NETWORK] Connecting to Betfair Exchange Stream API...");
            await tcpClient.ConnectAsync(_host, _port, cancellationToken);
            var rawStream = tcpClient.GetStream();
            sslStream = new SslStream(rawStream, false);
            await sslStream.AuthenticateAsClientAsync(_host, null,
                    SslProtocols.Tls12 | SslProtocols.Tls13, true);


            
            pipeReader = PipeReader.Create(sslStream);

            _processingTask = Task.Run(() => ProcessChannelMessagesAsync(cancellationToken), cancellationToken);
            
            _heartbeatTask = Task.Run(() => MonitorHeartbeatAsync(cancellationToken), cancellationToken);
            _logger.Log("[NETWORK] Connected. Submitting authentication handshake token payload...");

            try
            {
                await SendJsonAsync(new { op = "authentication", id = 0, appKey = _appKey, session = _sessionToken }, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                var err = $"Authentication failed: {ex.Message}";
                _logger.Log(err);
                throw new HttpRequestException($"Authentication failed: {ex.Message}", ex);
            }
            //// 2. Subscribe to Orders once (This stays active for the lifetime of the socket)
            // var orderSubscription = new OrderSubscripton
            // {
            //     Op = "orderSubscription",
            //     Id = NextId(),
            //     OrderFilter = new OrderFilter
            //     {
            //         CustomerStrategyRefs = new List<string>
            //             {
            //                 "ST-TENNIS"
            //             }
            //     }

            // };
            // try
            // {
            //     await SendJsonAsync(orderSubscription, cancellationToken);
            // }
            // catch (HttpRequestException ex)
            // {
            //     var err = $"OrderSubscription failed: {ex.Message}";
            //     _logger.Log(err);
            //     throw new HttpRequestException(err, ex);
            // }

            
            _logger.Log($"Stream Client setup and subscribed");

        }
        public async Task RunLoopAsync(CancellationToken cancellationToken)
        {
            _logger.Log($"Starting Stream Client loop");            
           
            try
            {
                
                // Loop 1: Core Socket Reader
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await pipeReader.ReadAsync(cancellationToken);
                    
                    var buffer = result.Buffer;

                    bool messageReceived = false;

                    while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                    {
                        messageReceived = true;
                        int lineLength = (int)line.Length;
                        byte[] sharedBuffer = _pool.Rent(lineLength);
                        line.CopyTo(sharedBuffer);
                        _streamDumper.EnqueueRawChunk(sharedBuffer, lineLength);
                        if (!_messageChannel.Writer.TryWrite((sharedBuffer, lineLength)))
                        {
                            _pool.Return(sharedBuffer);
                        }
                    }

                    // High-performance thread-safe timestamp update
                    if (messageReceived)
                    {
                        Interlocked.Exchange(ref _lastMessageTicks, DateTime.UtcNow.Ticks);
                    }

                    pipeReader.AdvanceTo(buffer.Start, buffer.End);
                    if (result.IsCompleted) break;
                    
                }
            }
            catch(Exception ex)
            {
                _logger.Log($"Exception in StreamClient RunAsync {ex.Message}");
            }
            finally
            {
                await pipeReader.CompleteAsync();
                _messageChannel.Writer.Complete();
                await Task.WhenAll(_processingTask,_heartbeatTask);
                //await _heartbeatTask;
                sslStream.Dispose();
                
            }
        }

        public MarketCacheManager MarketCacheManager{get {return _marketCache;}}
        public OrderCacheManager OrderCacheManager{get {return _orderCache;}}

        public async Task ChangeMarketsAsync(List<string> newMarketIds, CancellationToken cancellationToken)
        {
            _logger.Log("ChangeMarketsAsync");
            if (sslStream == null)
                throw new InvalidOperationException("Client is not connected. Call RunAsync first.");
            
            var marketFilter = new BetfairStreamClient.Betting.MarketFilter
            {
                MarketIds = newMarketIds,
            };
            var marketDataFilter = new MarketDataFilter
            {
                Fields = new List<MarketDataFilter.FieldsEnum?> { MarketDataFilter.FieldsEnum.ExBestOffersDisp, MarketDataFilter.FieldsEnum.ExTraded, MarketDataFilter.FieldsEnum.ExTradedVol, MarketDataFilter.FieldsEnum.ExMarketDef}
            };
            var marketSubscription = new MarketSubscription
            {
                Op = "marketSubscription",
                ConflateMs = 0,
                HeartbeatMs = 5000,
                MarketFilter = marketFilter,
                MarketDataFilter = marketDataFilter
            };
            

            // Reuse the existing active TCP pipeline
            await SendJsonAsync(marketSubscription, cancellationToken);
            _logger.Log($"[NETWORK] Dispatched subscription requests for {newMarketIds.Count} tennis markets.");
        
        }

        private async Task SendJsonAsync(object payload, CancellationToken cancellationToken)
        {
            if (sslStream == null) return;
            var options = new JsonSerializerOptions
            {
                // Excludes all null properties across the entire object
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var jsonString = JsonSerializer.Serialize(payload,options) + "\r\n";
            var bytes = Encoding.UTF8.GetBytes(jsonString);

            await _writeLock.WaitAsync(cancellationToken);
            try
            {
                await sslStream.WriteAsync(bytes, cancellationToken);
                await sslStream.FlushAsync(cancellationToken);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            var position = buffer.PositionOf((byte)'\n');
            if (position == null)
            {
                line = default;
                return false;
            }
            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }

        private async Task ProcessChannelMessagesAsync(CancellationToken cancellationToken)
        {
            var reader = _messageChannel.Reader;
            try
            {
                await foreach (var (buffer, length) in reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                                                
                        _streamParser.ParseMessageNoAllocations(buffer, length);
                        
                    }
                    catch(Exception ex)
                    {
                        _logger.Log($"ProcessChannelMessagesAsync exception: {ex.Message}");
                    }
                    finally
                    {
                        _pool.Return(buffer);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        

        private async Task MonitorHeartbeatAsync(CancellationToken cancellationToken)
        {
            var heartbeatInterval = TimeSpan.FromSeconds(5);

            while (!cancellationToken.IsCancellationRequested)
            {
                // Thread-safe atomic read of the last message timestamp
                long lastTicks = Interlocked.Read(ref _lastMessageTicks);
                var lastMessageTime = new DateTime(lastTicks, DateTimeKind.Utc);
                TimeSpan timeSinceLastMessage = DateTime.UtcNow - lastMessageTime;

                if (timeSinceLastMessage >= heartbeatInterval)
                {
                    try
                    {
                        int id = Interlocked.Increment(ref _heartbeatIdCounter);

                        // Construct the strict Betfair JSON heartbeat sequence
                        var heartbeatPayload = new { op = "heartbeat", id = id };

                        // Write directly over the active SSL Stream using your existing writer helper
                        await SendJsonAsync(heartbeatPayload, cancellationToken);

                        _logger.Log($"[HEARTBEAT] Sent heartbeat frame (id: {id}) due to 5s socket silence.");
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"[HEARTBEAT] Failed sending heartbeat payload: {ex.Message}");
                        // Break out to let connection management handle the faulting socket
                        break;
                    }

                    // Reset our tracking marker so we don't spam if SendJsonAsync took time
                    Interlocked.Exchange(ref _lastMessageTicks, DateTime.UtcNow.Ticks);
                    timeSinceLastMessage = TimeSpan.Zero;
                }

                // Dynamically sleep for only the time remaining in the 5-second window
                TimeSpan nextCheck = heartbeatInterval - timeSinceLastMessage;
                if (nextCheck <= TimeSpan.Zero) nextCheck = heartbeatInterval;

                try
                {
                    await Task.Delay(nextCheck, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break; // Clean shutdown requested
                }
            }
        }


        
    }
}
