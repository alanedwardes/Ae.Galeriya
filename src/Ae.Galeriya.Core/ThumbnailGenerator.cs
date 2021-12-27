using Ae.MediaMetadata.Entities;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    internal sealed class ThumbnailGenerator : IThumbnailGenerator
    {
        private readonly SemaphoreSlim _semaphore = new(4, 4);

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
            await _semaphore.WaitAsync(token);

            try
            {
                using (var image = new MagickImage(stream))
                {
                    image.Format = MagickFormat.Jpeg;
                    image.Quality = 50;
                    image.Strip();
                    image.Resize(width, height);
                    _orientationActions[orientation]?.Invoke(image);
                    image.Write(fileInfo);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
