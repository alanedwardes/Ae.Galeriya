using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetImages : IPiwigoWebServiceMethod
    {
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;
        private readonly ICategoryPermissionsRepository _permissionsRepository;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.categories.getImages";
        public bool AllowAnonymous => false;

        public PiwigoGetImages(IPiwigoPhotosPageGenerator pageGenerator,
            ICategoryPermissionsRepository permissionsRepository,
            IServiceProvider serviceProvider)
        {
            _pageGenerator = pageGenerator;
            _permissionsRepository = permissionsRepository;
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            var page = parameters.GetOptional<int>("page");
            var perPage = parameters.GetOptional<int>("per_page");
            var categoryId = parameters.GetOptional<uint>("cat_id");
            var order = parameters.GetOptional("order") ?? "date_creation asc";

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var photosQuery = _permissionsRepository.GetAccessiblePhotos(context, userId.Value);

            if (categoryId.HasValue)
            {
                photosQuery = photosQuery.Where(x => x.Categories.Select(x => x.CategoryId).Contains(categoryId.Value));
            }

            switch (order)
            {
                case "date_available asc":
                    photosQuery = photosQuery.OrderBy(x => x.CreatedOn);
                    break;
                case "date_available desc":
                    photosQuery = photosQuery.OrderByDescending(x => x.CreatedOn);
                    break;
                case "date_creation asc":
                    photosQuery = photosQuery.OrderBy(x => x.TakenOn ?? x.FileCreatedOn);
                    break;
                case "date_creation desc":
                    photosQuery = photosQuery.OrderByDescending(x => x.TakenOn ?? x.FileCreatedOn);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, "Order is not supported");
            }

            return await _pageGenerator.CreatePage(page, perPage, photosQuery, token);
        }
    }
}
