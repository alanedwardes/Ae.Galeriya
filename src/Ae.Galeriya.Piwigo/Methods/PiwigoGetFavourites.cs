using Ae.Galeriya.Core;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetFavourites : IPiwigoWebServiceMethod
    {
        private readonly ICategoryPermissionsRepository _permissionsRepository;
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.users.favorites.getList";
        public bool AllowAnonymous => false;

        public PiwigoGetFavourites(ICategoryPermissionsRepository permissionsRepository, IPiwigoPhotosPageGenerator pageGenerator, IServiceProvider serviceProvider)
        {
            _permissionsRepository = permissionsRepository;
            _pageGenerator = pageGenerator;
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, FileMultipartSection> fileParameters, uint? userId, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var user = await context.Users.Include(x => x.FavouritePhotos).SingleAsync(x => x.Id == userId, token);

            var photos = _permissionsRepository.GetAccessiblePhotos(context, userId.Value)
                .Where(x => user.FavouritePhotos.Contains(x));

            return await _pageGenerator.CreatePage(0, 0, photos, token);
        }
    }
}
