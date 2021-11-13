using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoCategories
    {
        [JsonPropertyName("categories")]
        public ICollection<PiwigoCategory> Categories { get; set; }
    }
}
