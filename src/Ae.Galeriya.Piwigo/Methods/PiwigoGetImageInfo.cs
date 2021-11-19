using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetImageInfo : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _dbContext;

        public string MethodName => "pwg.images.getInfo";

        public PiwigoGetImageInfo(GalleriaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var imageId = parameters["image_id"].ToUInt32(null);

            var photo = await _dbContext.Photos.Include(x => x.Categories)
                .SingleAsync(x => x.PhotoId == imageId, token);

            return new PiwigoImage
            {
                Id = photo.PhotoId,
                Derivatives = new PiwigoImageDerivatives(photo.PhotoId),
                FileSize = photo.FileSize,
                AvailableOn = DateTimeOffset.UtcNow,
                LastModified = DateTimeOffset.UtcNow,
                CreatedOn = photo.CreatedOn,
                MetadataUpdatedOn = DateTimeOffset.UtcNow,
                File = photo.FileName,
                Name = photo.Name,
                Width = photo.Width,
                Height = photo.Height,
                Categories = photo.Categories.Select(x => new PiwigoCategorySlim
                {
                    CategoryId = x.CategoryId,
                    Name = x.Name,
                    GlobalRank = "1",
                    Permalink = new Uri("/wibble1", UriKind.Relative),
                    PageUrl = new Uri("/wibble1", UriKind.Relative),
                    UpperCategories = x.CategoryId.ToString(),
                    Url = new Uri("/wibble1", UriKind.Relative)
                }).ToArray(),
                PageUrl = new Uri("/wibble1", UriKind.Relative),
                ElementUrl = new Uri($"http://192.168.178.21:5000/blobs/{photo.PhotoId}{photo.Extension}", UriKind.Absolute),
            };
        }
    }
}
