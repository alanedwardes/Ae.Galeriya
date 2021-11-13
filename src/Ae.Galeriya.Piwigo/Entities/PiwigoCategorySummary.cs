using System;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoCategorySummary
    {
        [JsonPropertyName("id")]
        public uint Id { get; set; }
        [JsonPropertyName("url")]
        public Uri Url { get; set; }
        [JsonPropertyName("page_url")]
        public Uri PageUrl { get; set; }
    }
}
