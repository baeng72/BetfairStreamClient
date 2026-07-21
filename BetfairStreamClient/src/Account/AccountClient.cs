using BetfairStreamClient.Betting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetfairStreamClient.src.Account
{
    public sealed class BetfairAccountsClient : IAsyncDisposable
    {
        private readonly string _appKey;
        private readonly string _sessionToken;
        private const string ACCOUNTS_URL = "https://api.betfair.com/exchange/account/json-rpc/v1";
        private static readonly string GET_ACCOUNT_DETAILS = "getAccountDetails";
        private static readonly string GET_ACCOUNT_FUNDS = "getAccountFunds";
        private static readonly string GET_ACCOUNT_STATEMENT = "getAccountStatement";
        private static readonly string LIST_CURRENCY_RATES = "listCurrencyRates";
        private static readonly string TRANSFER_FUNDS = "transferFunds";
        private static readonly string WALLET = "wallet";
        private readonly HttpClient _http;
        private readonly SemaphoreSlim _semaphore;
        private int _idCounter;

        public BetfairAccountsClient(HttpClient httpClient, string appKey, string sessionToken)
        {
            _http = httpClient;
            _appKey = appKey;
            _sessionToken = sessionToken;
            _semaphore = new SemaphoreSlim(20);

        }

        public async ValueTask DisposeAsync()
        {
            //_http.Dispose();
            await Task.CompletedTask;
        }

        private async Task<TResult> CallAsync<TParams, TResult>(string method, TParams @params, bool retryOnAuthFailure = true, CancellationToken ct = default)
        {
            if (_sessionToken is null)
                throw new InvalidOperationException("No session token; call LoginAsync() first.");

            var requestBody = new JsonRpcRequest<TParams>
            {
                Method = $"AccountAPING/v1.0/{method}",
                Params = @params,
                Id = Interlocked.Increment(ref _idCounter),
            };

            await _semaphore.WaitAsync(ct);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, ACCOUNTS_URL)
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

        public Task<AccountDetailsResponse> GetAccountDetails(CancellationToken cancellationToken)
        {
            return CallAsync<object, AccountDetailsResponse>(GET_ACCOUNT_DETAILS,new object(),ct: cancellationToken);
            
        }

        public Task<AccountFundsResponse> GetAccountFunds(Wallet wallet, CancellationToken cancellationToken)
        {
            
            var @params = new GetAccountFundsParams
            {
                Wallet = wallet,
            };
            return CallAsync<GetAccountFundsParams,AccountFundsResponse>(GET_ACCOUNT_FUNDS,@params,ct:cancellationToken);
        }


        private sealed class GetAccountFundsParams
        {
            [JsonPropertyName("wallet")] public Wallet Wallet { get; set; }

        }
    }

    

}
