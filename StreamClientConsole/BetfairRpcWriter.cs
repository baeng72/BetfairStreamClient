using BetfairStreamClient.Betting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using BetfairStreamClient.Logging;

namespace StreamClientConsole
{
    public class BetfairRcpWriter
    {
        private readonly ChannelReader<OutboundCommand> _commandReader;
        private readonly Logger _logger;
        private readonly BetfairAsyncClient _client;
        private const int MaxBetfairBatchSize = 60; // Betfair's hard API limit for placeOrders
        private static int batchCount = 0;

        public BetfairRcpWriter(ChannelReader<OutboundCommand> commandReader, Logger logger, BetfairAsyncClient client)
        {
            _commandReader = commandReader;
            _logger = logger;
            _client = client;
        }

        public async Task StartWriteLoopAsync(CancellationToken cancellationToken)
        {
            List<OutboundCommand> batchBuffer = new List<OutboundCommand>();
            try
            {
                //1. Wait asynchronously until at least on item is available.
                while (await _commandReader.WaitToReadAsync(cancellationToken))
                {
                    batchBuffer.Clear();
                    //2. Read the first item that woke up the thread
                    if (_commandReader.TryRead(out var firstCommand))
                    {
                        batchBuffer.Add(firstCommand);
                        //3. Optimistically drain any other items sitting in the buffer
                        //stop early if we hit Betfair's maximum limit
                        while (batchBuffer.Count < MaxBetfairBatchSize &&
                            _commandReader.TryRead(out var nextCommand))
                        {
                            batchBuffer.Add(nextCommand);
                        }
                        //4. Send the whole batch over the wire in one API call


                        try
                        {
                            _logger.Log($"Batching cancel instructions into JSON-RPC calls.");
                            var cancelInstructions = SerializeCancelBatchToBetfairJsonRpc(batchBuffer);
                            await SendBatchCancelToBetfairAsync(cancelInstructions, cancellationToken, "ST-TENNIS");
                            _logger.Log($"Batching {batchBuffer.Count} bets into a single JSON-RPC call.");
                            var placeInstructions = SerializePlaceBatchToBetfairJsonRpc(batchBuffer);
                            await SendBatchPlaceToBetfairAsync(placeInstructions, cancellationToken, "ST-TENNIS");


                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Failed to transmit outbound bet batch: {ex.Message}");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Outbound write loop gracefully stopped via cancellation token.");
            }
        }

        private Dictionary<string,List<PlaceInstruction>> SerializePlaceBatchToBetfairJsonRpc(List<OutboundCommand> commands)
        {
            Dictionary<string, List<PlaceInstruction>> marketInstructions = new Dictionary<string, List<PlaceInstruction>>();
            
            foreach(var command in commands) {
                var placeBetCommand = command as PlaceBetCommand;
                if (placeBetCommand != null)
                {
                    if (!marketInstructions.ContainsKey(command.MarketId))
                    {
                        marketInstructions[command.MarketId] = new List<PlaceInstruction>();
                    }

                    var limitOrder = new LimitOrder(placeBetCommand.Size, placeBetCommand.Price, placeBetCommand.PersistenceType);
                    var placeInstruction = new PlaceInstruction(placeBetCommand.SelectionId, placeBetCommand.Side, limitOrder, OrderType.LIMIT, placeBetCommand.CustomerStrategyRef);
                    marketInstructions[command.MarketId].Add(placeInstruction);
                }
            }
            return marketInstructions;
        }

        private Dictionary<string,List<CancelInstruction>> SerializeCancelBatchToBetfairJsonRpc(List<OutboundCommand> commands)
        {
            Dictionary<string, List<CancelInstruction>> marketInstructions = new Dictionary<string, List<CancelInstruction>>();
            foreach (var command in commands)
            {
                var cancelCommand = command as CancelBetCommand;
                if(cancelCommand != null)
                {
                    if (!marketInstructions.ContainsKey(cancelCommand.MarketId))
                    {
                        marketInstructions[command.MarketId] = new List<CancelInstruction>();
                    }
                    var cancelInstruction = new CancelInstruction(cancelCommand.BetId);
                    marketInstructions[command.MarketId].Add(cancelInstruction);
                }
            }
            return marketInstructions;
        }

        private async Task SendBatchPlaceToBetfairAsync(Dictionary<string, List<PlaceInstruction>> placeInstructions, CancellationToken cancellationToken, string customerRef)
        {
            // Create a list to hold all our active network tasks
            List<Task<PlaceExecutionReport>> networkTasks = new List<Task<PlaceExecutionReport>>();

            foreach (var kvp in placeInstructions)
            {
                string marketId = kvp.Key;
                List<PlaceInstruction> instructions = kvp.Value;

                _logger.Log($"Firing parallel request with {instructions.Count} instructions to Market: {marketId}");

                // Capture the current batch count safely
                int currentBatch = Interlocked.Increment(ref batchCount);

                // Start the task immediately but DO NOT await it yet
                Task<PlaceExecutionReport> task = _client.PlaceOrdersAsync(marketId, instructions,customerRef, instructions[0].CustomerOrderRef, false, cancellationToken);

                networkTasks.Add(task);
            }

            // Await all network requests in parallel. 
            // This yields the thread once and wakes up when ALL responses are back.
            var resp = await Task.WhenAll(networkTasks);

            foreach(var rep in resp)
            {
                foreach(var inst in rep.InstructionReports)
                _logger.Log($"Place report for {rep.MarketId}, stake {inst.AveragePriceMatched}, price {inst.SizeMatched} is {inst.Status}");
            }
        }

        private async Task SendBatchCancelToBetfairAsync(Dictionary<string, List<CancelInstruction>> cancelInstructions, CancellationToken cancellationToken, string strategyRef)
        {
            // Create a list to hold all our active network tasks
            List<Task> networkTasks = new List<Task>();

            foreach (var kvp in cancelInstructions)
            {
                string marketId = kvp.Key;
                List<CancelInstruction> instructions = kvp.Value;

                _logger.Log($"Firing parallel request with {instructions.Count} instructions to Market: {marketId}");

                // Capture the current batch count safely
                int currentBatch = Interlocked.Increment(ref batchCount);

                // Start the task immediately but DO NOT await it yet
                Task task = _client.CancelOrdersAsync(marketId, instructions, strategyRef, cancellationToken);               

                networkTasks.Add(task);
            }

            // Await all network requests in parallel. 
            // This yields the thread once and wakes up when ALL responses are back.
            await Task.WhenAll(networkTasks);
        }
    }
}
