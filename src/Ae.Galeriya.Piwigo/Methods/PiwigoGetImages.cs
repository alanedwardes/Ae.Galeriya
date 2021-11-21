using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{

    internal sealed class PiwigoGetImages : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _context;
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;

        public string MethodName => "pwg.categories.getImages";

        public PiwigoGetImages(GalleriaDbContext context,
            IPiwigoPhotosPageGenerator pageGenerator)
        {
            _context = context;
            _pageGenerator = pageGenerator;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var page = parameters["page"].ToInt32(null);
            var perPage = parameters["per_page"].ToInt32(null);
            var categoryId = parameters["cat_id"].ToUInt32(null);

            var category = await _context.Categories
                .Include(x => x.Photos)
                .ThenInclude(x => x.Categories)
                .SingleAsync(x => x.CategoryId == categoryId, token);

            return _pageGenerator.CreatePage(page, perPage, category.Photos);
        }
    }
}
