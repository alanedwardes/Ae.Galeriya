using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Ae.Galeriya.Core
{
    public sealed class MediaInfo
    {
        public (int Width, int Height) Size { get; set; }
        public float Duration { get; set; }
        public DateTimeOffset? CreationTime { get; set; }
        public (string Make, string Model, string Software) Camera { get; set; }
        public (float Latitude, float Longitude)? Location { get; set; }
    }

    public sealed class MediaInfoExtractor
    {
        private IReadOnlyList<(string, string)> GetPacketTags(JsonDocument probeResultDocument)
        {
            var tags = new List<(string, string)>();

            var packetsElement = probeResultDocument.RootElement.GetProperty("packets_and_frames");
            foreach (var packet in packetsElement.EnumerateArray())
            {
                if (packet.TryGetProperty("tags", out JsonElement packetTagsElement))
                {
                    foreach (var item in packetTagsElement.EnumerateObject())
                    {
                        tags.Add((item.Name, item.Value.GetString().Trim()));
                    }
                }
            }

            return tags;
        }

        private ((int Width, int Height) Size, float? Duration) GetVideoStreamInfo(JsonDocument probeResultDocument)
        {
            var streamsElement = probeResultDocument.RootElement.GetProperty("streams");

            (int Width, int Height)? size = null;
            float? duration = null;
            foreach (var stream in streamsElement.EnumerateArray())
            {
                var codecType = stream.GetProperty("codec_type").GetString();
                if (codecType == "video")
                {
                    size = (stream.GetProperty("width").GetInt32(), stream.GetProperty("height").GetInt32());
                    duration = float.Parse(stream.GetProperty("duration").GetString());
                }
            }

            if (!size.HasValue)
            {
                throw new InvalidOperationException("No media with or height found");
            }

            return (size.Value, duration);
        }

        private IReadOnlyList<(string, string)> GetFormatTags(JsonDocument probeResultDocument)
        {
            var format = probeResultDocument.RootElement.GetProperty("format");

            var tags = new List<(string, string)>();

            if (format.TryGetProperty("tags", out var tagsElement))
            {
                foreach (var item in tagsElement.EnumerateObject())
                {
                    tags.Add((item.Name, item.Value.GetString().Trim()));
                }
            }

            return tags;
        }

        private (string Make, string Model, string Software) GetCamera(IEnumerable<(string, string)> tags)
        {
            var make = tags.Where(x => x.Item1 == "Make" || x.Item1 == "com.apple.quicktime.make").Select(x => x.Item2).FirstOrDefault();
            var model = tags.Where(x => x.Item1 == "Model" || x.Item1 == "com.apple.quicktime.model").Select(x => x.Item2).FirstOrDefault();
            var software = tags.Where(x => x.Item1 == "Software" || x.Item1 == "com.apple.quicktime.software").Select(x => x.Item2).FirstOrDefault();
            return (make, model, software);
        }

        private (float Latitude, float Longitude) GetLocation(IEnumerable<(string, string)> tags)
        {
            var latitude = tags.Where(x => x.Item1 == "GPSLatitude").Select(x => x.Item2).FirstOrDefault();
            var latitudeRef = tags.Where(x => x.Item1 == "GPSLatitudeRef").Select(x => x.Item2).FirstOrDefault();
            var longitude = tags.Where(x => x.Item1 == "GPSLongitude").Select(x => x.Item2).FirstOrDefault();
            var longitudeRef = tags.Where(x => x.Item1 == "GPSLongitudeRef").Select(x => x.Item2).FirstOrDefault();
            var iso6709 = tags.Where(x => x.Item1 == "com.apple.quicktime.location.ISO6709").Select(x => x.Item2).FirstOrDefault();

            var coordinate = new Coordinate();

            if (!string.IsNullOrWhiteSpace(iso6709))
            {
                coordinate.ParseIsoString(iso6709);
                return (coordinate.Latitude, coordinate.Longitude);
            }

            static float[] ParseLatLongValue(string value)
            {
                return value.Split(',').Select(x => x.Trim()).Select(x =>
                {
                    var parts = x.Split(':').Select(float.Parse).ToArray();
                    return parts[0] / parts[1];
                }).ToArray();
            }

            var latitudeValue = ParseLatLongValue(latitude);
            var longitudeValue = ParseLatLongValue(longitude);

            coordinate.SetDMS(latitudeValue[0], latitudeValue[1], latitudeValue[2], latitudeRef == "N", longitudeValue[0], longitudeValue[1], longitudeValue[2], !(longitudeRef == "W"));

            return (coordinate.Latitude, coordinate.Longitude);
        }

        public async Task<MediaInfo> Extract(FileInfo fileInfo, CancellationToken token)
        {
            string probeResult = await Probe.New().Start($"-print_format json -show_frames -show_streams -show_format -show_packets \"{fileInfo}\"", token);

            var probeResultDocument = JsonDocument.Parse(probeResult);

            var packetTags = GetPacketTags(probeResultDocument);
            var videoStreamInfo = GetVideoStreamInfo(probeResultDocument);
            var formatTags = GetFormatTags(probeResultDocument);

            var tags = packetTags.Concat(formatTags).ToArray();

            var make = GetCamera(tags);
            var location = GetLocation(tags);

            return new MediaInfo
            {
                Duration = videoStreamInfo.Duration.Value,
                CreationTime = DateTimeOffset.UtcNow,
                Size = videoStreamInfo.Size,
                Camera = make,
                Location = location
            };
        }
    }
}
