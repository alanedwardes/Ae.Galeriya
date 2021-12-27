using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;
using Microsoft.Extensions.DependencyInjection;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetImageInfo : IPiwigoWebServiceMethod
    {
        private readonly IPiwigoImageDerivativesGenerator _derivativesGenerator;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IPiwigoBaseAddressLocator _baseAddressLocator;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.images.getInfo";
        public bool AllowAnonymous => false;

        public PiwigoGetImageInfo(IPiwigoImageDerivativesGenerator derivativesGenerator, ICategoryPermissionsRepository categoryPermissions, IPiwigoBaseAddressLocator baseAddressLocator, IServiceProvider serviceProvider)
        {
            _derivativesGenerator = derivativesGenerator;
            _categoryPermissions = categoryPermissions;
            _baseAddressLocator = baseAddressLocator;
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            Photo photo;
            using (var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>())
            {
                photo = await _categoryPermissions.EnsureCanAccessPhoto(context, userId.Value, parameters.GetRequired<uint>("image_id"), token);
            }

            return new PiwigoImage
            {
                Id = photo.PhotoId,
                Derivatives = _derivativesGenerator.GenerateDerivatives(photo.PhotoId),
                FileSize = photo.FileSize,
                Author = photo.CreatedBy.UserName,
                AvailableOn = photo.TakenOn ?? photo.CreatedOn,
                LastModified = photo.UpdatedOn,
                CreatedOn = photo.TakenOn ?? photo.CreatedOn,
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
                Tags = photo.Tags.Select(x => new PiwigoTagSummary
                {
                    TagId = x.TagId,
                    LastModified = x.UpdatedOn ?? x.CreatedOn,
                    Name = x.Name,
                    PageUrl = new Uri("https://www.example.com/"),
                    Url = new Uri("https://www.example.com/"),
                    Slug = x.GenerateSlug()
                }).ToArray(),
                PageUrl = new Uri("/wibble1", UriKind.Relative),
                ElementUrl = new Uri(_baseAddressLocator.GetBaseAddress(), $"/blobs/{photo.PhotoId}.{photo.Extension}"),
            };
        }
    }
}
