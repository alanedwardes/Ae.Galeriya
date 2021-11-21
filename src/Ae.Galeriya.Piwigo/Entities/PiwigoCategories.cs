using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoCategories
    {
        [JsonPropertyName("categories")]
        public ICollection<PiwigoCategory> Categories { get; set; }
    }

    internal sealed class PiwigoTags
    {
        [JsonPropertyName("tags")]
        public ICollection<PiwigoTag> Tags { get; set; }
    }

    internal sealed class PiwigoTag
    {
        [JsonPropertyName("id")]
        public uint TagId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("url_name")]
        public string Slug { get; set; }
        [JsonPropertyName("lastmodified")]
        public DateTimeOffset LastModified { get; set; }
        [JsonPropertyName("counter")]
        public uint Counter { get; set; }
        [JsonPropertyName("url")]
        public Uri Url { get; set; }
    }
}
