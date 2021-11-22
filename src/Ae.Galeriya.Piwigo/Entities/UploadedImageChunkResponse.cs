using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoUploadedChunkResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
