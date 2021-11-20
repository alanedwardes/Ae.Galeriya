using Ae.Galeriya.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetFile : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _dbContext;
        private readonly IBlobRepository _blobRepository;

        public string MethodName => "pwg.images.getFile";

        public PiwigoGetFile(GalleriaDbContext dbContext, IBlobRepository blobRepository)
        {
            _dbContext = dbContext;
            _blobRepository = blobRepository;
        }

        private async Task<Stream> BufferIfNotSeekable(Stream stream, CancellationToken token)
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

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var imageId = parameters["image_id"].ToUInt32(null);

            var photo = await _dbContext.Photos.SingleAsync(x => x.PhotoId == imageId, token);

            var stream = await BufferIfNotSeekable(await _blobRepository.GetBlob(photo.Blob, token), token);

            return new FileStreamResult(stream, "application/octet-stream")
            {
                EnableRangeProcessing = true,
                LastModified = photo.CreatedOn
            };
        }
    }
}
