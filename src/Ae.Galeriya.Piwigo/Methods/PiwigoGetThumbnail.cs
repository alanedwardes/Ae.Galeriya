using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetThumbnail : IPiwigoWebServiceMethod
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly GalleriaDbContext _dbContext;
        private readonly IBlobRepository _blobRepository;

        public string MethodName => "pwg.images.getThumbnail";

        public PiwigoGetThumbnail(IHttpContextAccessor httpContext, GalleriaDbContext dbContext, IBlobRepository blobRepository)
        {
            _httpContext = httpContext;
            _dbContext = dbContext;
            _blobRepository = blobRepository;
        }

        private readonly IReadOnlyDictionary<MediaOrientation, Action<IImageProcessingContext>> _orientationActions = new Dictionary<MediaOrientation, Action<IImageProcessingContext>>
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

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var width = parameters["width"].ToInt32(null);
            var height = parameters["height"].ToInt32(null);
            var type = parameters["type"].ToString(null);
            var imageId = parameters["image_id"].ToUInt32(null);

            var photo = await _dbContext.Photos.SingleAsync(x => x.PhotoId == imageId, token);

            var stream = await _blobRepository.GetBlob(photo.SnapshotBlob ?? photo.Blob, token);

            var image = await Image.LoadAsync(Configuration.Default, stream, token);

            image.Mutate(processor =>
            {
                processor.Resize(new ResizeOptions
                {
                    Mode = type == "classic" ? ResizeMode.Max : ResizeMode.Crop,
                    Size = new Size(width, height)
                });
                _orientationActions[photo.Orientation]?.Invoke(processor);
            });

            var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms, token);
            ms.Position = 0;

            return new FileStreamResult(ms, "image/jpeg")
            {
                LastModified = photo.CreatedOn
            };
        }
    }
}
