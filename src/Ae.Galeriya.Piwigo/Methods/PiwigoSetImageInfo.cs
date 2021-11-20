using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;
using System.Linq;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoSetImageInfo : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _context;

        public string MethodName => "pwg.images.setInfo";

        public PiwigoSetImageInfo(GalleriaDbContext context)
        {
            _context = context;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var imageId = parameters["image_id"].ToUInt32(null);
            var multipleValueMode = parameters["multiple_value_mode"].ToString(null);

            var photo = await _context.Photos.Include(x => x.Categories).SingleAsync(x => x.PhotoId == imageId, token);

            if (parameters.TryGetValue("author", out var authorString))
            {
                photo.Author = authorString.ToString(null);
            }

            if (parameters.TryGetValue("name", out var nameString))
            {
                photo.Name = nameString.ToString(null);
            }

            if (parameters.TryGetValue("comment", out var commentString))
            {
                photo.Comment = commentString.ToString(null);
            }

            if (parameters.TryGetValue("categories", out var categoriesString))
            {
                var categoryIds = categoriesString.ToString(null).Split(";").Select(uint.Parse).ToArray();

                var categories = await _context.Categories.Where(x => categoryIds.Contains(x.CategoryId)).ToArrayAsync(token);

                if (multipleValueMode == "replace")
                {
                    photo.Categories = categories;
                }
                else
                {
                    throw new NotImplementedException($"Mode not implemented: {multipleValueMode}");
                }

            }

            await _context.SaveChangesAsync(token);
            return null;
        }
    }
}
