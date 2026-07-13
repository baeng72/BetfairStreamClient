using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StreamTest.InPlayerService.Model
{
    public class EventScore
    {
        [JsonPropertyName("eventTypeId")]
        public int EventTypeId { get; set; }

        [JsonPropertyName("eventId")]
        public int  EventId { get; set; }

        [JsonPropertyName("score")]
        public Score score { get; set; }

        [JsonPropertyName("currentSet")]
        public int CurrentSet { get; set; }

        [JsonPropertyName("currentGame")]
        public int CurrentGame { get; set; }

        [JsonPropertyName("fullTimeElapsed")]
        public FullTimeElapsed FullTimeElapsed { get; set; }
    }
}
