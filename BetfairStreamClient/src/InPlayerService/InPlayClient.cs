using BetfairStreamClient.InPlayerService.Model;
using BetfairStreamClient.Stream;
using BetfairStreamClient.Logging;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BetfairStreamClient.InPlayerService
{
    public class InPlayClient
    {
        private readonly HttpClient _client;
        private const string IpsBaseUrl = "https://ips.betfair.com/inplayservice/v1.1/";
        private static readonly string GET_SCORES = "scores";
        private static readonly string EVENT_IDS = "eventIds";
        private List<string> eventIds = new List<string>();
        private Logger _logger;
        private object lockObj = new object();

        public event EventHandler<List<EventScore>> EventScoreHandler;

        public InPlayClient(HttpClient client,string sessionToken,Logger logger)
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookieContainer };
            cookieContainer.Add(new Uri("https://ips.betfair.com"), new Cookie("ssoid",sessionToken));            
            _client = client;            
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
            
        }

        

        public void AddEventId(string eventId)
        {
            lock (lockObj)
            {
                if(!eventIds.Contains(eventId))
                    eventIds.Add(eventId);
            }
        }
        public void AddEventIds(List<string> newEventIds)
        {
            lock (lockObj)
            {
                foreach (var eventId in newEventIds)
                {
                    if(!eventIds.Contains(eventId))
                        eventIds.Add(eventId);
                }
            }
        }
        public void RemoveEventId(string eventId)
        {
            lock (lockObj)
            {
                eventIds.Remove(eventId);
            }
        }

        public async Task MonitorMatchScoreAsync(CancellationToken cancellationToken)
        {
            

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {

                    if (eventIds.Count > 0)
                    {


                        string req = $"{EVENT_IDS}=";
                        for (int i = 0; i < eventIds.Count; i++)
                        {
                            var eventId = eventIds[i];
                            req += eventId;
                            req += i < eventIds.Count - 1 ? "," : "";
                        }
                        req += "&locale=en_GB";
                        string url = $"{IpsBaseUrl}{GET_SCORES}?{req}";
                        var response = await _client.GetStringAsync(url);
                        if (response != null){
                            _logger.Log($"[INPLAY] response: {response}");
                            using (JsonDocument doc = JsonDocument.Parse(response))
                            {
                                JsonElement root = doc.RootElement;
                                List<EventScore>? scores = JsonSerializer.Deserialize<List<EventScore>>(root);
                                if(scores != null)
                                EventScoreHandler(this, scores);
                                
                            }
                        }
                    }



                }
                catch (Exception ex)

                {
                    _logger.Log($"[INPLAY] Exception: {ex.Message}");
                    throw;
                }
            }

        }
    }
}
