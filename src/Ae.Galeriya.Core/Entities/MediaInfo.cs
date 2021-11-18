using System;

namespace Ae.Galeriya.Core.Entities
{
    public sealed class MediaInfo
    {
        public (int Width, int Height) Size { get; set; }
        public float Duration { get; set; }
        public MediaOrientation Orientation { get; set; }
        public DateTimeOffset? CreationTime { get; set; }
        public (string Make, string Model, string Software) Camera { get; set; }
        public (float Latitude, float Longitude)? Location { get; set; }
    }
}
