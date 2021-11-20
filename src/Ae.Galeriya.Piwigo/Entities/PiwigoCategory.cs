using System;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoCategory
    {
        [JsonPropertyName("id")]
        public uint Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("comment")]
        public string Comment { get; set; }
        [JsonPropertyName("permalink")]
        public string Permalink { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("uppercats")]
        public string UpperCategories { get; set; }
        [JsonPropertyName("global_rank")]
        public string GlobalRank { get; set; }
        [JsonPropertyName("id_uppercat")]
        public uint? UpperCategoryId { get; set; }
        [JsonPropertyName("nb_images")]
        public int ImageCount { get; set; }
        [JsonPropertyName("total_nb_images")]
        public int TotalImageCount { get; set; }
        [JsonPropertyName("representative_picture_id")]
        public uint? RepresentativePictureId { get; set; }
        [JsonPropertyName("date_last")]
        public DateTimeOffset? PageLastImageDate { get; set; }
        [JsonPropertyName("max_date_last")]
        public DateTimeOffset? LastImageDate { get; set; }
        [JsonPropertyName("nb_categories")]
        public int CategoryCount { get; set; }
        [JsonPropertyName("url")]
        public Uri Url { get; set; }
        [JsonPropertyName("tn_url")]
        public Uri ThumbnailUrl { get; set; }
    }
}
