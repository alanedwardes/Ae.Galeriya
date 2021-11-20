using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoImage
    {
        [JsonPropertyName("id")]
        public uint Id { get; set; }
        [JsonPropertyName("file")]
        public string File { get; set; }
        [JsonPropertyName("date_available")]
        public DateTimeOffset AvailableOn { get; set; }
        [JsonPropertyName("date_creation")]
        public DateTimeOffset CreatedOn { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("comment")]
        public string Comment { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("hit")]
        public int Hit { get; set; }
        [JsonPropertyName("filesize")]
        public ulong FileSize { get; set; }
        [JsonPropertyName("width")]
        public uint Width { get; set; }
        [JsonPropertyName("height")]
        public uint Height { get; set; }
        [JsonPropertyName("coi")]
        public string Coi { get; set; }
        [JsonPropertyName("representative_ext")]
        public string RepresentativeExt { get; set; }
        [JsonPropertyName("date_metadata_update")]
        public DateTimeOffset? MetadataUpdatedOn { get; set; }
        [JsonPropertyName("rating_score")]
        public string RatingScore { get; set; }
        [JsonPropertyName("level")]
        public string Level { get; set; }
        [JsonPropertyName("md5sum")]
        public string Md5Checksum { get; set; }
        [JsonPropertyName("added_by")]
        public string AddedBy { get; set; }
        [JsonPropertyName("rotation")]
        public string Rotation { get; set; }
        [JsonPropertyName("latitude")]
        public string Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public string Longitude { get; set; }
        [JsonPropertyName("lastmodified")]
        public DateTimeOffset? LastModified { get; set; }
        [JsonPropertyName("page_url")]
        public Uri PageUrl { get; set; }
        [JsonPropertyName("element_url")]
        public Uri ElementUrl { get; set; }
        [JsonPropertyName("derivatives")]
        public IReadOnlyDictionary<string, PiwigoThumbnail> Derivatives { get; set; }
        [JsonPropertyName("categories")]
        public ICollection<PiwigoCategory> Categories { get; set; }
    }
}
