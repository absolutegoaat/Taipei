using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Taipei.Models.Log
{
    public class CookieLog
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("flow_id")]
        public long FlowId { get; set; }
        
        [JsonProperty("cookie_type")]
        public string? CookieType { get; set; }

        [JsonProperty("cookie_name")]
        public string? CookieName { get; set; }

        [JsonProperty("cookie_value")]
        public string? CookieValue { get; set; }
    }
}
