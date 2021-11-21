using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetTagImages : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _context;
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;

        public string MethodName => "pwg.tags.getImages";

        public PiwigoGetTagImages(GalleriaDbContext context,
            IPiwigoPhotosPageGenerator pageGenerator)
        {
            _context = context;
            _pageGenerator = pageGenerator;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var page = parameters["page"].ToInt32(null);
            var perPage = parameters["per_page"].ToInt32(null);
            var tagId = parameters["tag_id"].ToUInt32(null);

            var tag = await _context.Tags.Include(x => x.Photos).SingleAsync(x => x.TagId == tagId, token);

            return _pageGenerator.CreatePage(page, perPage, tag.Photos.Count, tag.Photos);
        }
    }
}
