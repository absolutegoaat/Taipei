using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taipei.Models
{
    class SearchModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("client_ip")]
        public string? Client_Ip { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; } = string.Empty;

        [JsonProperty("host")]
        public string Host { get; set; } = string.Empty;

        [JsonProperty("path")]
        public string? Path { get; set; }

        [JsonProperty("status_code")]
        public uint? StatusCode { get; set; }

        [JsonProperty("is_https")]
        public bool? IsHttps { get; set; }
    }
}
