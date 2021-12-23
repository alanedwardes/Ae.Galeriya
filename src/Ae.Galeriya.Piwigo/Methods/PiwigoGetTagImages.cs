using Ae.Galeriya.Core;
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

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            var page = parameters.GetOptional<int>("page") ?? 0;
            var perPage = parameters.GetOptional<int>("per_page") ?? 64;

            var tag = await _context.Tags.SingleAsync(x => x.TagId == parameters.GetRequired<uint>("tag_id"), token);

            var photosQuery = _categoryPermissions.GetAccessiblePhotos(userId.Value).Where(x => x.Tags.Contains(tag));

            return await _pageGenerator.CreatePage(page, perPage, photosQuery, token);
        }
    }
}
