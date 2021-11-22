using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoSetCategoryRepresentative : IPiwigoWebServiceMethod
    {
        private readonly GaleriaDbContext _context;

        public string MethodName => "pwg.categories.setRepresentative";

        public PiwigoSetCategoryRepresentative(GaleriaDbContext context)
        {
            _context = context;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var categoryId = parameters["category_id"].ToUInt32(null);
            var imageId = parameters["image_id"].ToUInt32(null);

            var category = await _context.Categories.SingleAsync(x => x.CategoryId == categoryId, token);
            var photo = await _context.Photos.SingleAsync(x => x.PhotoId == imageId, token);

            category.CoverPhoto = photo;
            await _context.SaveChangesAsync(token);

            return null;
        }
    }
}
