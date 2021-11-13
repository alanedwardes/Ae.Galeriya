using Ae.Galeriya.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetThumbnail : IPiwigoWebServiceMethod
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly GalleriaDbContext _dbContext;
        private readonly IPhotoBlobRepository _blobRepository;

        public string MethodName => "pwg.images.getThumbnail";

        public PiwigoGetThumbnail(IHttpContextAccessor httpContext, GalleriaDbContext dbContext, IPhotoBlobRepository blobRepository)
        {
            _httpContext = httpContext;
            _dbContext = dbContext;
            _blobRepository = blobRepository;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var width = parameters["width"].ToInt32(null);
            var height = parameters["height"].ToInt32(null);
            var type = parameters["type"].ToString(null);
            var imageId = parameters["image_id"].ToUInt32(null);

            var photo = await _dbContext.Photos.SingleAsync(x => x.PhotoId == imageId, token);

            var stream = await _blobRepository.GetPhotoBlob(photo, token);

            var image = await Image.LoadAsync(Configuration.Default, stream, token);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = type == "classic" ? ResizeMode.Max : ResizeMode.Crop,
                Size = new Size(width, height)
            }));

            _httpContext.HttpContext.Response.ContentType = "image/jpeg";
            await image.SaveAsJpegAsync(_httpContext.HttpContext.Response.Body, token);
            await _httpContext.HttpContext.Response.CompleteAsync();

            return Task.FromResult<object>(true);
        }
    }
}
