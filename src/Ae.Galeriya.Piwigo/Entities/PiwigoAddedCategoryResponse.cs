using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoAddedCategoryResponse
    {
        [JsonPropertyName("info")]
        public string Info { get; set; }
        [JsonPropertyName("id")]
        public uint Id { get; set; }
    }
}
