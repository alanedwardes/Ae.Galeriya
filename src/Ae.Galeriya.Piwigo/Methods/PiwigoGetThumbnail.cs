using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Entities;
using Ae.Galeriya.Core.Exceptions;
using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetThumbnail : IPiwigoWebServiceMethod
    {
        private readonly ILogger<PiwigoGetThumbnail> _logger;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IBlobRepository _blobRepository;
        private readonly IPiwigoConfiguration _piwigoConfiguration;

        public string MethodName => "pwg.images.getThumbnail";
        public bool AllowAnonymous => false;

        public PiwigoGetThumbnail(ILogger<PiwigoGetThumbnail> logger, ICategoryPermissionsRepository categoryPermissions, IBlobRepository blobRepository, IPiwigoConfiguration piwigoConfiguration)
        {
            _logger = logger;
            _categoryPermissions = categoryPermissions;
            _blobRepository = blobRepository;
            _piwigoConfiguration = piwigoConfiguration;
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

        private Guid CacheHash(params object[] items)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(string.Join('|', items.Select(x => x.ToString()))));
                return new Guid(hash);
            }
        }

        private async Task<Stream> GetThubmnail(Photo photo, int width, int height, string type, CancellationToken token)
        {
            var cacheBlobId = CacheHash(width, height, type, photo.PhotoId);

            try
            {
                return await _piwigoConfiguration.FileBlobRepository.GetBlob(cacheBlobId, token);
            }
            catch (BlobNotFoundException)
            {
                _logger.LogWarning("No cached thumbnail for {PhotoId}, generating from source", photo.PhotoId);
            }

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

            using (var ms = new MemoryStream())
            {
                await image.SaveAsJpegAsync(ms, token);
                ms.Position = 0;
                await _piwigoConfiguration.FileBlobRepository.PutBlob(ms, cacheBlobId, token);
            }

            return await _piwigoConfiguration.FileBlobRepository.GetBlob(cacheBlobId, token);
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var width = parameters["width"].ToInt32(null);
            var height = parameters["height"].ToInt32(null);
            var type = parameters["type"].ToString(null);
            var imageId = parameters["image_id"].ToUInt32(null);

            var photo = await _categoryPermissions.EnsureCanAccessPhoto(user, imageId, token);

            var thumbnailStream = await GetThubmnail(photo, width, height, type, token);

            return new FileStreamResult(thumbnailStream, "image/jpeg")
            {
                LastModified = photo.CreatedOn
            };
        }
    }
}
