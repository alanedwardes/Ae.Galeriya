using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoSessionStatus
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("theme")]
        public string Theme { get; set; }
        [JsonPropertyName("language")]
        public string Language { get; set; }
        [JsonPropertyName("pwg_token")]
        public string Token { get; set; }
        [JsonPropertyName("charset")]
        public string Charset { get; set; }
        [JsonPropertyName("current_datetime")]
        public DateTimeOffset CurrentDatetime { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("available_sizes")]
        public IList<string> AvailableSizes { get; set; }
        [JsonPropertyName("upload_file_types")]
        public string UploadFileTypes { get; set; }
        [JsonPropertyName("upload_form_chunk_size")]
        public int UploadFormChunkSize { get; set; }
    }
}
