using Ae.MediaMetadata.Entities;
using ImageMagick;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    internal sealed class ThumbnailGenerator : IThumbnailGenerator
    {
        private readonly ILogger<ThumbnailGenerator> _logger;

        public ThumbnailGenerator(ILogger<ThumbnailGenerator> logger)
        {
            _logger = logger;
        }

        private readonly IReadOnlyDictionary<MediaOrientation, Action<MagickImage>?> _orientationActions = new Dictionary<MediaOrientation, Action<MagickImage>?>
        {
            { MediaOrientation.Unknown, null },
            { MediaOrientation.TopLeft, null },
            { MediaOrientation.TopRight, context => context.Flop() },
            { MediaOrientation.BottomRight, context => context.Rotate(180) },
            { MediaOrientation.BottomLeft, context => context.Flip() },
            { MediaOrientation.LeftTop, context => { context.Rotate(90); context.Flop(); } },
            { MediaOrientation.RightTop, context => context.Rotate(90) },
            { MediaOrientation.RightBottom, context => { context.Rotate(270); context.Flip(); } },
            { MediaOrientation.LeftBottom, context => context.Rotate(270) },
        };

        public async Task GenerateThumbnail(Stream stream, FileInfo fileInfo, MediaOrientation orientation, int width, int height, CancellationToken token)
        {
            using MagickImage image = CreateImage(stream);
            image.Format = MagickFormat.Jpeg;
            image.Quality = 50;
            image.Strip();
            image.Resize(width, height);
            _orientationActions[orientation]?.Invoke(image);
            await image.WriteAsync(fileInfo, token);
        }

        private MagickImage CreateImage(Stream stream)
        {
            if (stream is FileStream fs)
            {
                var fi = new FileInfo(fs.Name);

                using (fs)
                {
                }

                return new MagickImage(fi);
            }

            _logger.LogWarning("Not able to load image from disc for thumbnail, this may be slower");
            return new MagickImage(stream);
        }
    }
}
