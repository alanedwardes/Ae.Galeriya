using Ae.Galeriya.Core.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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

        private readonly IReadOnlyDictionary<MediaOrientation, Action<IImageProcessingContext>?> _orientationActions = new Dictionary<MediaOrientation, Action<IImageProcessingContext>?>
        {
            { MediaOrientation.Unknown, null },
            { MediaOrientation.TopLeft, null },
            { MediaOrientation.TopRight, context => context.Flip(FlipMode.Horizontal) },
            { MediaOrientation.BottomRight, context => context.Rotate(RotateMode.Rotate180) },
            { MediaOrientation.BottomLeft, context => context.Flip(FlipMode.Vertical) },
            { MediaOrientation.LeftTop, context => context.RotateFlip(RotateMode.Rotate90, FlipMode.Horizontal) },
            { MediaOrientation.RightTop, context => context.Rotate(RotateMode.Rotate90) },
            { MediaOrientation.RightBottom, context => context.RotateFlip(RotateMode.Rotate270, FlipMode.Vertical) },
            { MediaOrientation.LeftBottom, context => context.Rotate(RotateMode.Rotate270) },
        };

        public async Task<Stream> GenerateThumbnail(Stream stream, MediaOrientation orientation, int width, int height, ResizeMode mode, CancellationToken token)
        {
            await _semaphore.WaitAsync(token);

            try
            {
                using var image = await Image.LoadAsync(Configuration.Default, stream, token);

                image.Mutate(processor =>
                {
                    processor.Resize(new ResizeOptions
                    {
                        Mode = mode,
                        Size = new Size(width, height)
                    });
                    _orientationActions[orientation]?.Invoke(processor);
                });

                var ms = new MemoryStream();
                await image.SaveAsJpegAsync(ms, token);
                ms.Position = 0;
                return ms;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
