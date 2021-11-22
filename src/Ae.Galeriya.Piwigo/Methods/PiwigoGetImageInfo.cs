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
        private readonly GaleriaDbContext _dbContext;
        private readonly IPiwigoConfiguration _configuration;
        private readonly IPiwigoImageDerivativesGenerator _derivativesGenerator;

        public string MethodName => "pwg.images.getInfo";

        public PiwigoGetImageInfo(GaleriaDbContext dbContext, IPiwigoConfiguration configuration, IPiwigoImageDerivativesGenerator derivativesGenerator)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _derivativesGenerator = derivativesGenerator;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var imageId = parameters["image_id"].ToUInt32(null);

            var photo = await _dbContext.Photos.Include(x => x.Categories)
                .SingleAsync(x => x.PhotoId == imageId, token);

            return new PiwigoImage
            {
                Id = photo.PhotoId,
                Derivatives = _derivativesGenerator.GenerateDerivatives(photo.PhotoId),
                FileSize = photo.FileSize,
                AvailableOn = photo.CreatedOn,
                LastModified = photo.UpdatedOn,
                CreatedOn = photo.CreatedOn,
                MetadataUpdatedOn = photo.UpdatedOn,
                File = photo.FileName,
                Name = photo.Name,
                Width = photo.Width,
                Height = photo.Height,
                Categories = photo.Categories.Select(x => new PiwigoCategory
                {
                    CategoryId = x.CategoryId,
                    Name = x.Name,
                    GlobalRank = "1",
                    Permalink = new Uri("/wibble1", UriKind.Relative),
                    UpperCategories = x.CategoryId.ToString(),
                    Url = new Uri("/wibble1", UriKind.Relative)
                }).ToArray(),
                PageUrl = new Uri("/wibble1", UriKind.Relative),
                ElementUrl = new Uri(_configuration.BaseAddress, $"/blobs/{photo.PhotoId}.{photo.Extension}"),
            };
        }
    }
}
