using BetfairStreamClient.Betting;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BetfairStreamClient.src.Nav
{
    public sealed class BetfairAccountsClient : IAsyncDisposable
    {
        private readonly string _appKey;
        private readonly string _sessionToken;
        private readonly HttpClient _http;
        private readonly SemaphoreSlim _semaphore;
        private int _idCounter;
        private const string NAV_URL = "https://api.betfair.com/exchange/betting/rest/v1/{0}/navigation/menu.json";
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

        

        public async Task<T> GetMenuAsync<T>(CancellationToken ct = default)
        {
            // 1. Send the request and read only the headers first (highly efficient)
            try
            {
                using var response = await _http.SendAsync(
                    new HttpRequestMessage(HttpMethod.Get, NAV_URL),
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);

                response.EnsureSuccessStatusCode();

                // 2. Open a direct stream to the incoming network buffer
                using var contentStream = await response.Content.ReadAsStreamAsync(ct);
                System.IO.Stream inputStream = contentStream;

                // 3. Let HttpClient handle compression automatically, or manually unpack it here
                var contentEncoding = response.Content.Headers.ContentEncoding.ToString().ToLower();

                if (contentEncoding.Contains("gzip"))
                {
                    inputStream = new GZipStream(contentStream, CompressionMode.Decompress);
                }
                else if (contentEncoding.Contains("deflate"))
                {
                    inputStream = new DeflateStream(contentStream, CompressionMode.Decompress);
                }

                // 4. Stream bytes directly into the JSON deserializer without allocating an intermediate string
                using (inputStream)
                {
                    var payload = await JsonSerializer.DeserializeAsync<JsonRpcResponse<T>>(inputStream, BetfairJson.Options, ct)
                                  ?? throw new BetfairHttpException(200, "Received empty response stream from Betfair.");

                    if (payload.Error is not null)
                    {
                        throw new BetfairApiException("GET", payload.Error);
                    }

                    return payload.Result is null
                        ? throw new BetfairHttpException(200, "Payload result was null.")
                        : payload.Result;
                }
            }
            finally
            {
                _semaphore.Release();
            }
            //using (var streamReader = new StreamReader(inputStream, Encoding.UTF8))
            //using (var jsonReader = new JsonTextReader(streamReader))
            //{
            //    var serializer = new JsonSerializer();
            //    var jsonResponse = serializer.Deserialize<T>(jsonReader);

            //    return jsonResponse ?? throw new InvalidOperationException("Deserialization returned null.");
            //}
        }

        
    }
}
