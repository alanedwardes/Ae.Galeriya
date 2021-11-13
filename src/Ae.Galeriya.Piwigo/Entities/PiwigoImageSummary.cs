using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoImageSummary
    {
        [JsonPropertyName("id")]
        public uint Id { get; set; }
        [JsonPropertyName("width")]
        public uint Width { get; set; }
        [JsonPropertyName("height")]
        public uint Height { get; set; }
        [JsonPropertyName("hit")]
        public int Hit { get; set; }
        [JsonPropertyName("file")]
        public string File { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("comment")]
        public string Comment { get; set; }
        [JsonPropertyName("date_creation")]
        public DateTimeOffset CreatedOn { get; set; }
        [JsonPropertyName("date_available")]
        public DateTimeOffset AvailableOn { get; set; }
        [JsonPropertyName("page_url")]
        public Uri PageUrl { get; set; }
        [JsonPropertyName("element_url")]
        public Uri ElementUrl { get; set; }
        [JsonPropertyName("derivatives")]
        public PiwigoImageDerivatives Derivatives { get; set; }
        [JsonPropertyName("category")]
        public IEnumerable<PiwigoCategorySummary> Categories { get; set; }
    }
}
