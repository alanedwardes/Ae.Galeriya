using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetFile : IPiwigoWebServiceMethod
    {
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IPiwigoConfiguration _piwigoConfiguration;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.images.getFile";
        public bool AllowAnonymous => false;

        public PiwigoGetFile(ICategoryPermissionsRepository categoryPermissions, IPiwigoConfiguration piwigoConfiguration, IServiceProvider serviceProvider)
        {
            _categoryPermissions = categoryPermissions;
            _piwigoConfiguration = piwigoConfiguration;
            _serviceProvider = serviceProvider;
        }

        private static async Task<Stream> BufferIfNotSeekable(Stream stream, CancellationToken token)
        {
            if (stream.CanSeek)
            {
                return stream;
            }

            Stream ms = new MemoryStream();

            using (stream)
            {
                await stream.CopyToAsync(ms, token);
            }

            ms.Position = 0;
            return ms;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, IFormFile> fileParameters, uint? userId, CancellationToken token)
        {
            Photo photo;
            using (var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>())
            {
                photo = await _categoryPermissions.EnsureCanAccessPhoto(context, userId.Value, parameters.GetRequired<uint>("image_id"), token);
            }

            var stream = await BufferIfNotSeekable(await _piwigoConfiguration.PersistentBlobRepository(_serviceProvider).GetBlob(photo.BlobId, token), token);

            return new FileStreamResult(stream, "application/octet-stream")
            {
                EnableRangeProcessing = true,
                LastModified = photo.UpdatedOn ?? photo.CreatedOn
            };
        }
    }
}
