using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetImages : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _context;

        public string MethodName => "pwg.categories.getImages";

        public PiwigoGetImages(GalleriaDbContext context)
        {
            _context = context;
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

            var response = new PiwigoImages
            {
                Pagination = new PiwigoPagination
                {
                    Page = page,
                    PerPage = perPage,
                    Count = 1,
                    TotalCount = 1
                },
                Images = new List<PiwigoImageSummary>()
            };

            foreach (var photo in category.Photos)
            {
                var image = new PiwigoImageSummary
                {
                    Id = photo.PhotoId,
                    Width = photo.Width,
                    Height = photo.Height,
                    Hit = 1,
                    File = photo.FileName,
                    Name = photo.Name,
                    Comment = photo.Comment,
                    CreatedOn = photo.CreatedOn,
                    AvailableOn = photo.CreatedOn,
                    PageUrl = new Uri("http://www.example.com/"),
                    ElementUrl = new Uri("http://www.example.com/"),
                    Derivatives = new PiwigoImageDerivatives(photo.PhotoId),
                    Categories = photo.Categories.Select(x => new PiwigoCategorySummary
                    {
                        Id = x.CategoryId,
                        Url = new Uri("/category.php", UriKind.Relative),
                        PageUrl = new Uri("/category.php", UriKind.Relative),
                    })
                };

                response.Images.Add(image);
            }

            return response;
        }
    }
}
