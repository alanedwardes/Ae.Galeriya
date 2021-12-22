using Ae.Galeriya.Piwigo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Ae.Galeriya.Core.Tables;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace Ae.Galeriya.Piwigo
{
    internal sealed class PiwigoPhotosPageGenerator : IPiwigoPhotosPageGenerator
    {
        private readonly IPiwigoBaseAddressLocator _baseAddressLocator;
        private readonly IPiwigoImageDerivativesGenerator _derivativesGenerator;

        public PiwigoPhotosPageGenerator(IPiwigoBaseAddressLocator baseAddressLocator,
            IPiwigoImageDerivativesGenerator derivativesGenerator)
        {
            _baseAddressLocator = baseAddressLocator;
            _derivativesGenerator = derivativesGenerator;
        }

        public async Task<PiwigoImages> CreatePage(int page, int perPage, IQueryable<Photo> query, CancellationToken token)
        {
            var photosPage = await query.Skip(page * perPage)
                .Take(perPage)
                .ToArrayAsync(token);

            var response = new PiwigoImages
            {
                Pagination = new PiwigoPagination
                {
                    Page = page,
                    PerPage = perPage,
                    Count = photosPage.Length,
                    TotalCount = await query.CountAsync(token)
                },
                Images = new List<PiwigoImageSummary>()
            };

            foreach (var photo in photosPage)
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
                    CreatedOn = photo.TakenOn ?? photo.CreatedOn,
                    AvailableOn = photo.TakenOn ?? photo.CreatedOn,
                    PageUrl = new Uri("http://www.example.com/"),
                    ElementUrl = new Uri(_baseAddressLocator.GetBaseAddress(), $"/blobs/{photo.PhotoId}.{photo.Extension}"),
                    Derivatives = _derivativesGenerator.GenerateDerivatives(photo.PhotoId),
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
