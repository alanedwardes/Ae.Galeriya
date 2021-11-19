using Ae.Galeriya.Core;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContext;
        private readonly GalleriaDbContext _dbContext;
        private readonly IBlobRepository _blobRepository;

        public string MethodName => "pwg.images.getFile";

        public PiwigoGetFile(IHttpContextAccessor httpContext, GalleriaDbContext dbContext, IBlobRepository blobRepository)
        {
            _httpContext = httpContext;
            _dbContext = dbContext;
            _blobRepository = blobRepository;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var imageId = parameters["image_id"].ToUInt32(null);

            var photo = await _dbContext.Photos.SingleAsync(x => x.PhotoId == imageId, token);

            var stream = await _blobRepository.GetBlob(photo, false, token);

            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            return new FileStreamResult(ms, "application/octet-stream")
            {
                EnableRangeProcessing = true,
                LastModified = photo.CreatedOn
            };
        }
    }
}
