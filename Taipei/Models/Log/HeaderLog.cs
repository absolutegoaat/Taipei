using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Taipei.Models.Log
{
    public class HeaderLog
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("flow_id")]
        public long FlowId { get; set; }

        [JsonProperty("header_type")]
        public string? HeaderType { get; set; }

        [JsonProperty("header_name")]
        public string? HeaderName { get; set; }

        [JsonProperty("header_value")]
        public string? HeaderValue { get; set; }

        [JsonProperty("is_sensitive")]
        public int IsSensitive { get; set; }
    }
}
