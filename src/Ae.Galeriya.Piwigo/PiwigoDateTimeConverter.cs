using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo
{
    internal sealed class PiwigoDateTimeConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTimeOffset.ParseExact(reader.GetString(), "yyyy-MM-dd HH:mm:ss", null);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
