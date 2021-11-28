using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Entities;
using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IBlobRepository _blobRepository;

        public string MethodName => "pwg.images.getThumbnail";
        public bool AllowAnonymous => false;

        public PiwigoGetThumbnail(ICategoryPermissionsRepository categoryPermissions, IBlobRepository blobRepository)
        {
            _categoryPermissions = categoryPermissions;
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

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var width = parameters["width"].ToInt32(null);
            var height = parameters["height"].ToInt32(null);
            var type = parameters["type"].ToString(null);

            var photo = await _categoryPermissions.EnsureCanAccessPhoto(user, parameters["image_id"].ToUInt32(null), token);

            using var stream = await _blobRepository.GetBlob(photo.SnapshotBlob ?? photo.Blob, token);

            using var image = await Image.LoadAsync(Configuration.Default, stream, token);

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
