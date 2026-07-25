using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Taipei.Models.Log
{
    public class ContentLog
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("flow_id")]
        public long FlowId { get; set; }

        [JsonProperty("content_type")]
        public string? ContentType { get; set; }

        [JsonProperty("text_content")]
        public string? TextContent { get; set; }

        [JsonProperty("binary_content")]
        public string? BinaryContent { get; set; }

        [JsonProperty("is_binary")]
        public bool IsBinary { get; set; }

        [JsonProperty("was_truncated")]
        public bool WasTruncated { get; set; }

        [JsonProperty("original_size")]
        public long OriginalSize { get; set; }
    }
}
