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

        [JsonProperty("text_content")]
        public string? FlowName { get; set; } // just rail my twink goat ass

        [JsonProperty("binary_content")]
        public string? BinaryContent { get; set; }

        [JsonProperty("original_size")]
        public int OriginalSize { get; set; }
    }
}
