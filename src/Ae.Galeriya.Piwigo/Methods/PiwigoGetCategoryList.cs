using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetCategoryList : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _context;

        public string MethodName => "pwg.categories.getList";

        public PiwigoGetCategoryList(GalleriaDbContext context)
        {
            _context = context;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var response = new PiwigoCategories { Categories = new List<PiwigoCategory>() };

            foreach (var category in await _context.Categories.Include(x => x.Photos).ToArrayAsync())
            {
                uint firstPhoto = category.Photos.Select(x => x.PhotoId).FirstOrDefault();

                uint? thumbnailId = null;
                Uri thumbnailUri = null;
                if (firstPhoto > 0)
                {
                    var thumb = new PiwigoImageDerivatives(firstPhoto);
                    thumbnailUri = thumb.Thumb.Url;
                    thumbnailId = firstPhoto;
                }

                response.Categories.Add(new PiwigoCategory
                {
                    Id = category.CategoryId,
                    Name = category.Name,
                    Comment = category.Comment,
                    Permalink = null,
                    Status = category.Status,
                    UpperCategories = "1",
                    GlobalRank = "1",
                    UpperCategoryId = null,
                    ImageCount = 1,
                    TotalImageCount = 1,
                    RepresentativePictureId = firstPhoto,
                    LastImageDate = DateTimeOffset.UtcNow,
                    PageLastImageDate = DateTimeOffset.UtcNow,
                    CategoryCount = 0,
                    Url = new Uri("https://www.example.com/"),
                    ThumbnailUrl = thumbnailUri
                });
            }

            return response;
        }
    }
}
