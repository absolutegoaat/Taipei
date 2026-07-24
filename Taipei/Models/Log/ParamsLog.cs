using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Taipei.Models.Log
{
    public class ParamsLog
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("flow_id")]
        public long FlowId { get; set; }

        [JsonProperty("param_name")]
        public string? ParamName { get; set; }

        [JsonProperty("param_value")]
        public string? ParamValue { get; set; }
    }
}
