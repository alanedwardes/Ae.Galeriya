using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetImageInfo : IPiwigoWebServiceMethod
    {
        private readonly IPiwigoConfiguration _configuration;
        private readonly IPiwigoImageDerivativesGenerator _derivativesGenerator;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.images.getInfo";
        public bool AllowAnonymous => false;

        public PiwigoGetImageInfo(IPiwigoConfiguration configuration, IPiwigoImageDerivativesGenerator derivativesGenerator, ICategoryPermissionsRepository categoryPermissions)
        {
            _configuration = configuration;
            _derivativesGenerator = derivativesGenerator;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var photo = await _categoryPermissions.EnsureCanAccessPhoto(user, parameters["image_id"].ToUInt32(null), token);

            return new PiwigoImage
            {
                Id = photo.PhotoId,
                Derivatives = _derivativesGenerator.GenerateDerivatives(photo.PhotoId),
                FileSize = photo.FileSize,
                Author = photo.CreatedBy.UserName,
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
