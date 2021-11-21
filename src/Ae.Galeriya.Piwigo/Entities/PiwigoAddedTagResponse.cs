using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoAddedTagResponse
    {
        [JsonPropertyName("info")]
        public string Info { get; set; }
        [JsonPropertyName("id")]
        public uint Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("url_name")]
        public string Slug { get; set; }
    }
}
