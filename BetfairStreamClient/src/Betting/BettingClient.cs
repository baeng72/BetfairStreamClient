using BetfairStreamClient.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BetfairStreamClient.Betting
{
    public sealed class BettingClient : IAsyncDisposable
    {
        private readonly string _appKey;
        private readonly string _sessionToken;
        private const string RpcUrl = "https://api.betfair.com/exchange/betting/json-rpc/v1";
        private readonly HttpClient _http;
        private readonly SemaphoreSlim _semaphore;
        private int _idCounter;
        private Logger _logger;
        public BettingClient(HttpClient httpClient,Logger logger, string appKey, string sessionToken, TimeSpan? timeout=null)
        {
            _appKey = appKey;
            _sessionToken = sessionToken;
            _http = httpClient;
            _logger = logger;
            _semaphore = new SemaphoreSlim(20);

        }
        public async ValueTask DisposeAsync()
        {
            //_http.Dispose();            
            await Task.CompletedTask;
        }

        private async Task<TResult> CallAsync<TParams, TResult>(    string method, TParams @params, bool retryOnAuthFailure = true, CancellationToken ct = default)
        {
            if (_sessionToken is null)
                throw new InvalidOperationException("No session token; call LoginAsync() first.");

            var requestBody = new JsonRpcRequest<TParams>
            {
                Method = $"SportsAPING/v1.0/{method}",
                Params = @params,
                Id = Interlocked.Increment(ref _idCounter),
            };
            

            await _semaphore.WaitAsync(ct);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, RpcUrl)
                {
                    Content = JsonContent.Create(requestBody, options: BetfairJson.Options)
                };
                request.Headers.Add("X-Application", _appKey);
                request.Headers.Add("X-Authentication", _sessionToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // ResponseHeadersRead wakes up immediately when headers arrive, letting us stream the body
                using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                if (!response.IsSuccessStatusCode)
                {
                    // Allocate the string ONLY when a network error actually occurs
                    string errorBody = await response.Content.ReadAsStringAsync(ct);
                    throw new BetfairHttpException((int)response.StatusCode, errorBody);
                }

                // Stream raw bytes directly from the network buffer into the JSON deserializer
                using var responseStream = await response.Content.ReadAsStreamAsync(ct);

                var payload = await JsonSerializer.DeserializeAsync<JsonRpcResponse<TResult>>(responseStream, BetfairJson.Options, ct)
                              ?? throw new BetfairHttpException(200, "Received empty response stream from Betfair.");

                if (payload.Error is not null)
                {
                    throw new BetfairApiException(method, payload.Error);
                }

                return payload.Result is null
                    ? throw new BetfairHttpException(200, "Payload result was null.")
                    : payload.Result;
            }
            
            finally
            {
                _semaphore.Release();
            }
        }



        // ------------------------------------------------------------------ //
        // Betting API operations
        // ------------------------------------------------------------------ //
        public Task<List<EventTypeResult>> listEventTypes(MarketFilter marketFilter,string?locale = null, CancellationToken ct = default)
        {
            var @params = new ListEventTypesParams
            {
                Filter = marketFilter,
                Locale = locale
            };
            return CallAsync<ListEventTypesParams, List<EventTypeResult>>("listEventTypes", @params, ct: ct);
        }
        /// <summary>
        /// Return market metadata (names, runners, start times, ...).
        /// maxResults is capped at 10000 by Betfair; keep it as small as
        /// practical since this call is comparatively expensive.
        /// </summary>
        public Task<List<MarketCatalogue>> ListMarketCatalogueAsync(
            MarketFilter filter,
            List<MarketProjection>? marketProjection = null,
            MarketSort? sort = null,
            int maxResults = 100,
            string? locale = null,
            CancellationToken ct = default)
        {
            var @params = new ListMarketCatalogueParams
            {
                Filter = filter,
                MarketProjection = marketProjection,
                Sort = sort,
                MaxResults = maxResults,
                Locale = locale,
            };
            return CallAsync<ListMarketCatalogueParams, List<MarketCatalogue>>("listMarketCatalogue", @params, ct: ct);
        }

        /// <summary>
        /// Return live prices/state for the given markets. Handy for checking
        /// current back/lay prices before placing an order, or order status
        /// (matched/unmatched) after placing one.
        /// </summary>
        public Task<List<MarketBook>> ListMarketBookAsync(
            List<string> marketIds,
            PriceProjection? priceProjection = null,
            string? orderProjection = null,
            string? matchProjection = null,
            CancellationToken ct = default)
        {
            var @params = new ListMarketBookParams
            {
                MarketIds = marketIds,
                PriceProjection = priceProjection,
                OrderProjection = orderProjection,
                MatchProjection = matchProjection,
            };
            return CallAsync<ListMarketBookParams, List<MarketBook>>("listMarketBook", @params, ct: ct);
        }


        /// <summary>
        /// Place one or more orders on a single market. This does NOT throw
        /// just because a bet is rejected -- Betfair reports rejections as
        /// FAILURE/PROCESSED_WITH_ERRORS inside a normal response. Always
        /// check report.Status and each InstructionReports[i].Status.
        /// </summary>
        public Task<PlaceExecutionReport> PlaceOrdersAsync(
            string marketId,
            List<PlaceInstruction> instructions,
            string? customerRef = null,
            string? customerStrategyRef = null,      
            
            bool async =  false,
            CancellationToken ct = default)
        {
            
            var @params = new PlaceOrderParams
            {
                MarketId = marketId,
                Instructions = instructions,
                CustomerRef = customerRef,
                CustomerStrategyRef = customerStrategyRef,                    
                Async = async ?  true : null
            };
            return CallAsync<PlaceOrderParams, PlaceExecutionReport>("placeOrders", @params, ct: ct);
            
            
            
        }

        /// <summary>
        /// Cancel orders.
        /// - Omit both marketId and instructions to cancel ALL open orders across every market.
        /// - Provide marketId alone to cancel all open orders on that market.
        /// - Provide marketId + instructions to cancel/reduce specific bets (by betId) on that market.
        /// </summary>
        public Task<CancelExecutionReport> CancelOrdersAsync(
            string? marketId = null,
            List<CancelInstruction>? instructions = null,
            string? customerRef = null,
            CancellationToken ct = default)
        {
            var @params = new CancelOrdersParams
            {
                MarketId = marketId,
                Instructions = instructions,
                CustomerRef = customerRef,
            };
            return CallAsync<CancelOrdersParams, CancelExecutionReport>("cancelOrders", @params, ct: ct);
        }

        /// <summary>Convenience helper: list currently open/matched orders.</summary>
        public Task<CurrentOrderSummaryReport> ListCurrentOrdersAsync(
            List<string>? betIds = null,
            List<string>? marketIds = null,
            string? orderProjection = null,
            CancellationToken ct = default)
        {
            var @params = new ListCurrentOrdersParams
            {
                BetIds = betIds,
                MarketIds = marketIds,
                OrderProjection = orderProjection,
            };
            return CallAsync<ListCurrentOrdersParams, CurrentOrderSummaryReport>("listCurrentOrders", @params, ct: ct);
        }


        // ------------------------------------------------------------------ //
        // Params DTOs (internal wire shapes for each operation)
        // ------------------------------------------------------------------ //
        private sealed class ListEventTypesParams
        {
            [JsonPropertyName("filter")] public MarketFilter Filter { get; set; } = new();
            [JsonPropertyName("locale")] public string? Locale { get; set; }
        }
        private sealed class ListMarketCatalogueParams
        {
            [JsonPropertyName("filter")] public MarketFilter Filter { get; set; } = new();
            [JsonPropertyName("marketProjection")] public List<MarketProjection>? MarketProjection { get; set; }
            [JsonPropertyName("sort")] public MarketSort? Sort { get; set; }
            [JsonPropertyName("maxResults")] public int MaxResults { get; set; }
            [JsonPropertyName("locale")] public string? Locale { get; set; }
        }

        private sealed class ListMarketBookParams
        {
            [JsonPropertyName("marketIds")] public List<string> MarketIds { get; set; } = new();
            [JsonPropertyName("priceProjection")] public PriceProjection? PriceProjection { get; set; }
            [JsonPropertyName("orderProjection")] public string? OrderProjection { get; set; }
            [JsonPropertyName("matchProjection")] public string? MatchProjection { get; set; }
        }


        private class PlaceOrderParams
        {
            [JsonPropertyName("marketId")] public string MarketId { get; set; } = "";
            [JsonPropertyName("instructions")] public List<PlaceInstruction> Instructions { get; set; } = new();

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("customerRef")] public string? CustomerRef { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("customerStrategyRef")] public string? CustomerStrategyRef { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("async")] public bool? Async { get; set; }
        }
        

        private sealed class CancelOrdersParams
        {
            [JsonPropertyName("marketId")] public string? MarketId { get; set; }
            [JsonPropertyName("instructions")] public List<CancelInstruction>? Instructions { get; set; }
            [JsonPropertyName("customerRef")] public string? CustomerRef { get; set; }
        }

        private sealed class ListCurrentOrdersParams
        {
            [JsonPropertyName("betIds")] public List<string>? BetIds { get; set; }
            [JsonPropertyName("marketIds")] public List<string>? MarketIds { get; set; }
            [JsonPropertyName("orderProjection")] public string? OrderProjection { get; set; }
        }



    }
}
