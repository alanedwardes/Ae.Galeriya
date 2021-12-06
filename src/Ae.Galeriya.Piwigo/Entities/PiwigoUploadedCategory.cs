using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoUploadedCategory
    {
        [JsonPropertyName("id")]
        public uint CategoryId { get; set; }
        [JsonPropertyName("nb_photos")]
        public uint NumberOfPhotos { get; set; }
        [JsonPropertyName("label")]
        public string Name { get; set; }
    }
}
