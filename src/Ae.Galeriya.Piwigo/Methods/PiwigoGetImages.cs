using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Ae.Galeriya.Core.Tables;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetImages : IPiwigoWebServiceMethod
    {
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;
        private readonly ICategoryPermissionsRepository _permissionsRepository;

        public string MethodName => "pwg.categories.getImages";
        public bool AllowAnonymous => false;

        public PiwigoGetImages(IPiwigoPhotosPageGenerator pageGenerator,
            ICategoryPermissionsRepository permissionsRepository)
        {
            _pageGenerator = pageGenerator;
            _permissionsRepository = permissionsRepository;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var page = parameters.TryGetValue("page", out var pageValue) ? pageValue.ToInt32(null) : 0;
            var perPage = parameters.TryGetValue("page", out var perPageValue) ? perPageValue.ToInt32(null) : 64;

            var category = await _permissionsRepository.EnsureCanAccessCategory(user, parameters["cat_id"].ToUInt32(null), token);

            var photosQuery = (await _permissionsRepository.GetAccessiblePhotos(user, token))
                .Where(x => x.Categories.Contains(category));

            return await _pageGenerator.CreatePage(page, perPage, photosQuery, token);
        }
    }
}
