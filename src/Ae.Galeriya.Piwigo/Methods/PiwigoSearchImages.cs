using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoSearchImages : IPiwigoWebServiceMethod
    {
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.images.search";
        public bool AllowAnonymous => false;

        public PiwigoSearchImages(IPiwigoPhotosPageGenerator pageGenerator, ICategoryPermissionsRepository categoryPermissions, IServiceProvider serviceProvider)
        {
            _pageGenerator = pageGenerator;
            _categoryPermissions = categoryPermissions;
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, IFormFile> fileParameters, uint? userId, CancellationToken token)
        {
            var page = parameters.GetOptional<int>("page");
            var perPage = parameters.GetOptional<int>("per_page");

            var order = parameters.GetRequired<string>("order");
            var query = parameters.GetRequired<string>("query");

            var queryValue = $"%{query}%";

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var photosQuery = _categoryPermissions.GetAccessiblePhotos(context, userId.Value)
                .Where(photo => EF.Functions.Like(photo.Name, queryValue) ||
                                EF.Functions.Like(photo.FileName, queryValue) ||
                                photo.Tags.Any(tag => EF.Functions.Like(tag.Name, queryValue)) ||
                                photo.Categories.Any(category => EF.Functions.Like(category.Name, queryValue)));

            return await _pageGenerator.CreatePage(page, perPage, photosQuery, token);
        }
    }
}
