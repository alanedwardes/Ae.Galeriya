using Ae.Galeriya.Piwigo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Ae.Galeriya.Core.Tables;

namespace Ae.Galeriya.Piwigo
{
    internal sealed class PiwigoPhotosPageGenerator : IPiwigoPhotosPageGenerator
    {
        private readonly IPiwigoConfiguration _configuration;
        private readonly IPiwigoImageDerivativesGenerator _derivativesGenerator;

        public PiwigoPhotosPageGenerator(IPiwigoConfiguration configuration,
            IPiwigoImageDerivativesGenerator derivativesGenerator)
        {
            _configuration = configuration;
            _derivativesGenerator = derivativesGenerator;
        }

        public PiwigoImages CreatePage(int page, int perPage, int total, ICollection<Photo> photos)
        {
            var response = new PiwigoImages
            {
                Pagination = new PiwigoPagination
                {
                    Page = page,
                    PerPage = perPage,
                    Count = photos.Count,
                    TotalCount = total
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
                    CreatedOn = photo.CreatedOn,
                    AvailableOn = photo.CreatedOn,
                    PageUrl = new Uri("http://www.example.com/"),
                    ElementUrl = new Uri(_configuration.BaseAddress, $"/blobs/{photo.PhotoId}.{photo.Extension}"),
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
