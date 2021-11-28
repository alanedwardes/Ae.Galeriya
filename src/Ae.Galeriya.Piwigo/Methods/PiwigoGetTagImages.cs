using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetTagImages : IPiwigoWebServiceMethod
    {
        private readonly GaleriyaDbContext _context;
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.tags.getImages";
        public bool AllowAnonymous => false;

        public PiwigoGetTagImages(GaleriyaDbContext context,
            IPiwigoPhotosPageGenerator pageGenerator,
            ICategoryPermissionsRepository categoryPermissions)
        {
            _context = context;
            _pageGenerator = pageGenerator;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var page = parameters["page"].ToInt32(null);
            var perPage = parameters["per_page"].ToInt32(null);

            var tag = await _context.Tags.SingleAsync(x => x.TagId == parameters["tag_id"].ToUInt32(null), token);

            var photosQuery = (await _categoryPermissions.GetAccessiblePhotos(user, token))
                .Where(x => x.Tags.Contains(tag));

            return await _pageGenerator.CreatePage(page, perPage, photosQuery, token);
        }
    }
}
