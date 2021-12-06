using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoUploadedResponse
    {
        [JsonPropertyName("image_id")]
        public uint ImageId { get; set; }

        [JsonPropertyName("category")]
        public PiwigoUploadedCategory Category { get; set; }
    }
}
