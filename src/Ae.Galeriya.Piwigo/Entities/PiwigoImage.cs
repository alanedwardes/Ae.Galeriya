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
        public PiwigoImageDerivatives Derivatives { get; set; }

        //[JsonPropertyName("rates")]
        //public IDictionary<string, object> Rates { get; set; } = new Dictionary<string, object>
        //{
        //    { "score", null },
        //    { "count", 0 },
        //    { "average", null }
        //};
        [JsonPropertyName("categories")]
        public ICollection<PiwigoCategorySlim> Categories { get; set; }
        //public IDictionary<string, object>[] Categories { get; set; } = new[]
        //{
        //    new Dictionary<string, object>
        //    {
        //        { "id", 1 },
        //        { "name", "Test" },
        //        { "permalink", null },
        //        { "uppercats", "1" },
        //        { "global_rank", "1" },
        //        { "url", new Uri("/wibble3", UriKind.Relative) },
        //        { "page_url", new Uri("/wibble3", UriKind.Relative) }
        //    }
        //};
        //[JsonPropertyName("tags")]
        //public object[] Tags { get; set; } = Array.Empty<object>();
        //[JsonPropertyName("comments_paging")]
        //public IDictionary<string, object> CommentsPaging { get; set; } = new Dictionary<string, object>
        //{
        //    { "page", 0 },
        //    { "per_page", "10" },
        //    { "count", 0 },
        //    { "total_count", 0 }
        //};
        //[JsonPropertyName("comments")]
        //public object[] Comments { get; set; } = Array.Empty<object>();
    }

    internal sealed class PiwigoCategorySlim
    {
        [JsonPropertyName("id")]
        public uint CategoryId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("permalink")]
        public Uri Permalink { get; set; }
        [JsonPropertyName("uppercats")]
        public string UpperCategories { get; set; }
        [JsonPropertyName("global_rank")]
        public string GlobalRank { get; set; }
        [JsonPropertyName("url")]
        public Uri Url { get; set; }
        [JsonPropertyName("page_url")]
        public Uri PageUrl { get; set; }
    }
}
