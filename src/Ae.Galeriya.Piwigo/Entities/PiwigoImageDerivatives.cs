using System;
using System.Text.Json.Serialization;

namespace Ae.Galeriya.Piwigo.Entities
{
    internal sealed class PiwigoImageDerivatives
    {
        public PiwigoImageDerivatives()
        {
        }

        public PiwigoImageDerivatives(uint imageId)
        {
            Square = new PiwigoThumbnail
            {
                Width = 120,
                Height = 120,
                Url = CreateThumbnailUri(imageId, 120, 120, "square")
            };
            Thumb = new PiwigoThumbnail
            {
                Width = 144,
                Height = 144,
                Url = CreateThumbnailUri(imageId, 144, 144, "square")
            };
            Smallest = new PiwigoThumbnail
            {
                Width = 240,
                Height = 240,
                Url = CreateThumbnailUri(imageId, 240, 240, "square")
            };
            ExtraSmall = new PiwigoThumbnail
            {
                Width = 432,
                Height = 324,
                Url = CreateThumbnailUri(imageId, 432, 324, "square")
            };
            Small = new PiwigoThumbnail
            {
                Width = 576,
                Height = 432,
                Url = CreateThumbnailUri(imageId, 576, 432, "square")
            };
            Medium = new PiwigoThumbnail
            {
                Width = 792,
                Height = 594,
                Url = CreateThumbnailUri(imageId, 792, 594, "square")
            };
            Large = new PiwigoThumbnail
            {
                Width = 1008,
                Height = 756,
                Url = CreateThumbnailUri(imageId, 1008, 756, "square")
            };
            ExtraLarge = new PiwigoThumbnail
            {
                Width = 1224,
                Height = 918,
                Url = CreateThumbnailUri(imageId, 1224, 918, "square")
            };
            Largest = new PiwigoThumbnail
            {
                Width = 1656,
                Height = 1242,
                Url = CreateThumbnailUri(imageId, 1656, 1242, "square")
            };
        }

        public Uri CreateThumbnailUri(uint imageId, int width, int height, string type)
        {
            return new Uri($"http://192.168.178.21:5000/ws.php?method=pwg.images.getThumbnail&image_id={imageId}&width={width}&height={height}&type={type}", UriKind.Absolute);
        }

        [JsonPropertyName("square")]
        public PiwigoThumbnail Square { get; set; }
        [JsonPropertyName("thumb")]
        public PiwigoThumbnail Thumb { get; set; }
        [JsonPropertyName("2small")]
        public PiwigoThumbnail Smallest { get; set; }
        [JsonPropertyName("xsmall")]
        public PiwigoThumbnail ExtraSmall { get; set; }
        [JsonPropertyName("small")]
        public PiwigoThumbnail Small { get; set; }
        [JsonPropertyName("medium")]
        public PiwigoThumbnail Medium { get; set; }
        [JsonPropertyName("large")]
        public PiwigoThumbnail Large { get; set; }
        [JsonPropertyName("xlarge")]
        public PiwigoThumbnail ExtraLarge { get; set; }
        [JsonPropertyName("xxlarge")]
        public PiwigoThumbnail Largest { get; set; }
    }
}
