using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Exceptions;
using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IPiwigoConfiguration _piwigoConfiguration;
        private readonly IThumbnailGenerator _thumbnailGenerator;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.images.getThumbnail";
        public bool AllowAnonymous => false;

        public PiwigoGetThumbnail(ILogger<PiwigoGetThumbnail> logger,
            ICategoryPermissionsRepository categoryPermissions,
            IPiwigoConfiguration piwigoConfiguration,
            IThumbnailGenerator thumbnailGenerator,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _categoryPermissions = categoryPermissions;
            _piwigoConfiguration = piwigoConfiguration;
            _thumbnailGenerator = thumbnailGenerator;
            _serviceProvider = serviceProvider;
        }

        private string CacheHash(params object[] items)
        {
            using var md5 = SHA256.Create();
            var input = Encoding.UTF8.GetBytes(string.Join('|', items.Select(x => x.ToString())));
            return string.Concat(md5.ComputeHash(input).Select(x => x.ToString("x2")));
        }

        private async Task<(Stream Stream, string Hash)> GetThubmnail(Photo photo, int width, int height, string type, CancellationToken token)
        {
            var cacheBlobId = CacheHash(width, height, type, photo.PhotoId);

            try
            {
                return (await _piwigoConfiguration.ThumbnailBlobRepository(_serviceProvider).GetBlob(cacheBlobId, token), cacheBlobId);
            }
            catch (BlobNotFoundException)
            {
                _logger.LogWarning("No cached thumbnail for {PhotoId}, generating from source", photo.PhotoId);
            }

            using var stream = await _piwigoConfiguration.PersistentBlobRepository(_serviceProvider).GetBlob(photo.BlobId + (photo.HasThumbnail ? "_thumb" : string.Empty), token);

            var resizeMode = type == "classic" ? ResizeMode.Max : ResizeMode.Crop;

            using (var thumbnail = await _thumbnailGenerator.GenerateThumbnail(stream, photo.Orientation, width, height, resizeMode, token))
            {
                await _piwigoConfiguration.ThumbnailBlobRepository(_serviceProvider).PutBlob(thumbnail, cacheBlobId, token);
            }

            return (await _piwigoConfiguration.ThumbnailBlobRepository(_serviceProvider).GetBlob(cacheBlobId, token), cacheBlobId);
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            var width = parameters.GetRequired<int>("width");
            var height = parameters.GetRequired<int>("height");
            var type = parameters.GetRequired<string>("type");
            var imageId = parameters.GetRequired<uint>("image_id");

            var sw = Stopwatch.StartNew();
            var photo = await _categoryPermissions.EnsureCanAccessPhoto(userId.Value, imageId, token);
            _logger.LogInformation("Got photo in {TotalSeconds} seconds", sw.Elapsed.TotalSeconds);

            sw.Restart();
            var thumbnail = await GetThubmnail(photo, width, height, type, token);
            _logger.LogInformation("Got thumbnail in {TotalSeconds} seconds", sw.Elapsed.TotalSeconds);

            return new FileStreamResult(thumbnail.Stream, "image/jpeg")
            {
                LastModified = photo.UpdatedOn ?? photo.CreatedOn,
                EntityTag = new EntityTagHeaderValue('"' + thumbnail.Hash + '"')
            };
        }
    }
}
