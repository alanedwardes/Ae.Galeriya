using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoResponse
    {
        [JsonPropertyName("stat")]
        public string Stat { get; set; } = "ok";
        [JsonPropertyName("result")]
        public object? Result { get; set; }
        [JsonPropertyName("error")]
        public int? Error { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
