using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoImages
    {
        [JsonPropertyName("paging")]
        public PiwigoPagination Pagination { get; set; }
        [JsonPropertyName("images")]
        public ICollection<PiwigoImageSummary> Images { get; set; }
    }
}
