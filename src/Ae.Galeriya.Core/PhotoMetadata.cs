using Ae.Geocode.Google.Entities;
using Ae.MediaMetadata.Entities;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Core
{
    public sealed class PhotoMetadata
    {
        [JsonPropertyName("media")]
        public MediaInfo MediaInfo { get; set; }
        [JsonPropertyName("geocode")]
        public GeocodeResponse? Geocode { get; set; }
    }
}
