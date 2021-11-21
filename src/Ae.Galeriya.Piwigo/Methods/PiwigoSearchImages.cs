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
        private readonly GalleriaDbContext _context;
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;

        public string MethodName => "pwg.images.search";

        public PiwigoSearchImages(GalleriaDbContext context, IPiwigoPhotosPageGenerator pageGenerator)
        {
            _context = context;
            _pageGenerator = pageGenerator;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var order = parameters["order"].ToString(null);
            var page = parameters["page"].ToInt32(null);
            var perPage = parameters["per_page"].ToInt32(null);
            var query = parameters["query"].ToString(null);

            var queryValue = $"%{query}%";

            var photos = await _context.Photos.Where(photo => EF.Functions.Like(photo.Name, queryValue) ||
                                                              EF.Functions.Like(photo.FileName, queryValue) ||
                                                              photo.Tags.Any(tag => EF.Functions.Like(tag.Name, queryValue)) ||
                                                              photo.Categories.Any(category => EF.Functions.Like(category.Name, queryValue)))
                                              .ToArrayAsync(token);

            return _pageGenerator.CreatePage(page, perPage, photos.Length, photos);
        }
    }
}
