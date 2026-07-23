using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taipei.Models
{
    public class LogEntry
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("method")]
        public string? Method { get; set; }

        [JsonProperty("client_ip")]
        public string? Client_Ip { get; set; }

        [JsonProperty("host")]
        public string? Host { get; set; }

        [JsonProperty("path")]
        public string? Path { get; set; }

        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

        [JsonProperty("pretty_url")]
        public string? PrettyUrl { get; set; }

        [JsonProperty("is_https")]
        public int IsHttpsRaw { get; set; } = 0;
        public bool IsHttps => IsHttpsRaw == 1;
    }
}
