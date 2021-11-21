using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;
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

            var photos = await _context.Photos.Where(photo => photo.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                                              photo.FileName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                                              photo.Tags.Any(tag => tag.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                                              .ToArrayAsync(token);

            return _pageGenerator.CreatePage(page, perPage, photos);
        }
    }
}
