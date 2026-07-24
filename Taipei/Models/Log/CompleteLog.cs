using System;
using Newtonsoft.Json;

namespace Taipei.Models.Log
{
    public class CompleteLog
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("flow_hash")]
        public string FlowHash { get; set; } = string.Empty;

        [JsonProperty("session_id")]
        public string SessionId { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("session_duration")]
        public decimal? SessionDuration { get; set; }

        [JsonProperty("client_ip")]
        public string? ClientIp { get; set; }

        [JsonProperty("client_port")]
        public uint? ClientPort { get; set; }

        [JsonProperty("server_ip")]
        public string? ServerIp { get; set; }

        [JsonProperty("server_port")]
        public uint? ServerPort { get; set; }

        [JsonProperty("server_sni")]
        public string? ServerSni { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; } = string.Empty;

        [JsonProperty("scheme")]
        public string? Scheme { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; } = string.Empty;

        [JsonProperty("port")]
        public uint? Port { get; set; }

        [JsonProperty("path")]
        public string? Path { get; set; }

        [JsonProperty("pretty_url")]
        public string? PrettyUrl { get; set; }

        [JsonProperty("http_version")]
        public string? HttpVersion { get; set; }

        [JsonProperty("status_code")]
        public uint? StatusCode { get; set; }

        [JsonProperty("status_reason")]
        public string? StatusReason { get; set; }

        [JsonProperty("req_content_length")]
        public uint? ReqContentLength { get; set; }

        [JsonProperty("req_is_binary")]
        public bool? ReqIsBinary { get; set; }

        [JsonProperty("req_encoding")]
        public string? ReqEncoding { get; set; }

        [JsonProperty("req_truncated")]
        public bool? ReqTruncated { get; set; }

        [JsonProperty("resp_content_length")]
        public uint? RespContentLength { get; set; }

        [JsonProperty("resp_is_binary")]
        public bool? RespIsBinary { get; set; }

        [JsonProperty("resp_encoding")]
        public string? RespEncoding { get; set; }

        [JsonProperty("resp_truncated")]
        public bool? RespTruncated { get; set; }

        [JsonProperty("host_category")]
        public string? HostCategory { get; set; }

        [JsonProperty("is_https")]
        public bool? IsHttps { get; set; }

        [JsonProperty("has_error")]
        public bool? HasError { get; set; }

        [JsonProperty("error_message")]
        public string? ErrorMessage { get; set; }

        [JsonProperty("req_start")]
        public decimal? ReqStart { get; set; }

        [JsonProperty("req_end")]
        public decimal? ReqEnd { get; set; }

        [JsonProperty("resp_start")]
        public decimal? RespStart { get; set; }

        [JsonProperty("resp_end")]
        public decimal? RespEnd { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}