using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoMethods
    {
        [JsonPropertyName("methods")]
        public IEnumerable<string> Methods { get; set; }
    }
}
