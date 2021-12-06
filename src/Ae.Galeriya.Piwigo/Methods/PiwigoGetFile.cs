using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetFile : IPiwigoWebServiceMethod
    {
        private readonly IBlobRepository _blobRepository;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.images.getFile";
        public bool AllowAnonymous => false;

        public PiwigoGetFile(IBlobRepository blobRepository, ICategoryPermissionsRepository categoryPermissions)
        {
            _blobRepository = blobRepository;
            _categoryPermissions = categoryPermissions;
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

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var photo = await _categoryPermissions.EnsureCanAccessPhoto(user, parameters.GetRequired<uint>("image_id"), token);

            var stream = await BufferIfNotSeekable(await _blobRepository.GetBlob(photo.Blob, token), token);

            return new FileStreamResult(stream, "application/octet-stream")
            {
                EnableRangeProcessing = true,
                LastModified = photo.CreatedOn
            };
        }
    }
}
