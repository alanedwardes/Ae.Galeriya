using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoSearchImages : IPiwigoWebServiceMethod
    {
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.images.search";
        public bool AllowAnonymous => false;

        public PiwigoSearchImages(IPiwigoPhotosPageGenerator pageGenerator, ICategoryPermissionsRepository categoryPermissions)
        {
            _pageGenerator = pageGenerator;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            var page = parameters.GetOptional<int>("page") ?? 0;
            var perPage = parameters.GetOptional<int>("per_page") ?? 64;

            var order = parameters.GetRequired<string>("order");
            var query = parameters.GetRequired<string>("query");

            var queryValue = $"%{query}%";

            var photosQuery = _categoryPermissions.GetAccessiblePhotos(userId.Value)
                .Where(photo => EF.Functions.Like(photo.Name, queryValue) ||
                                EF.Functions.Like(photo.FileName, queryValue) ||
                                photo.Tags.Any(tag => EF.Functions.Like(tag.Name, queryValue)) ||
                                photo.Categories.Any(category => EF.Functions.Like(category.Name, queryValue)));

            return await _pageGenerator.CreatePage(page, perPage, photosQuery, token);
        }
    }
}
