using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Ae.Galeriya.Core.Tables;
using Microsoft.Extensions.Logging;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetImages : IPiwigoWebServiceMethod
    {
        private readonly ILogger<PiwigoGetImages> _logger;
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;
        private readonly ICategoryPermissionsRepository _permissionsRepository;

        public string MethodName => "pwg.categories.getImages";
        public bool AllowAnonymous => false;

        public PiwigoGetImages(ILogger<PiwigoGetImages> logger,
            IPiwigoPhotosPageGenerator pageGenerator,
            ICategoryPermissionsRepository permissionsRepository)
        {
            _logger = logger;
            _pageGenerator = pageGenerator;
            _permissionsRepository = permissionsRepository;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var page = parameters.GetOptional<int>("page") ?? 0;
            var perPage = parameters.GetOptional<int>("per_page") ?? 64;
            var categoryId = parameters.GetOptional<uint>("cat_id");
            var order = parameters.GetOptional("order") ?? "date_creation asc";

            _logger.LogWarning("Parameters: {Parameters}", string.Join(",", parameters.Select(x => $"{x.Key}={x.Value}")));

            var photosQuery = await _permissionsRepository.GetAccessiblePhotos(user, token);

            if (categoryId.HasValue)
            {
                var category = await _permissionsRepository.EnsureCanAccessCategory(user, categoryId.Value, token);
                photosQuery = photosQuery.Where(x => x.Categories.Contains(category));
            }

            switch (order)
            {
                case "date_creation asc":
                    photosQuery = photosQuery.OrderBy(x => x.CreatedOn.ToString());
                    break;
                case "date_creation desc":
                    photosQuery = photosQuery.OrderByDescending(x => x.CreatedOn.ToString());
                    break;
            }

            return await _pageGenerator.CreatePage(page, perPage, photosQuery, token);
        }
    }
}
