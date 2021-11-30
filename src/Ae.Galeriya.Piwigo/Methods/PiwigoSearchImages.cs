using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Ae.Galeriya.Core.Tables;

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

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var order = parameters["order"].ToString(null);
            var page = parameters["page"].ToInt32(null);
            var perPage = parameters["per_page"].ToInt32(null);
            var query = parameters["query"].ToString(null);

            var queryValue = $"%{query}%";

            var photosQuery = (await _categoryPermissions.GetAccessiblePhotos(user, token))
                .Where(photo => EF.Functions.Like(photo.Name, queryValue) ||
                                EF.Functions.Like(photo.FileName, queryValue) ||
                                photo.Tags.Any(tag => EF.Functions.Like(tag.Name, queryValue)) ||
                                photo.Categories.Any(category => EF.Functions.Like(category.Name, queryValue)));

            return await _pageGenerator.CreatePage(page, perPage, photosQuery, token);
        }
    }
}
