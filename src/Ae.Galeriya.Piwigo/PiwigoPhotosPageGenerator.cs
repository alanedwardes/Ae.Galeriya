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

        public async Task<PiwigoImages> CreatePage(int? page, int? perPage, IQueryable<Photo> query, CancellationToken token)
        {
            var photoPage = page ?? 0;
            var photosPerPage = Math.Clamp(perPage ?? 0, 1_000, 10_000);

            var photos = await query.Skip(photoPage * photosPerPage).Take(photosPerPage).ToArrayAsync(token);

            var response = new PiwigoImages
            {
                Pagination = new PiwigoPagination
                {
                    Page = photoPage,
                    PerPage = photosPerPage,
                    Count = photos.Length,
                    TotalCount = await query.CountAsync(token)
                },
                Images = new List<PiwigoImageSummary>()
            };

            foreach (var photo in photos)
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
                    CreatedOn = photo.TakenOn ?? photo.FileCreatedOn,
                    AvailableOn = photo.CreatedOn,
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
