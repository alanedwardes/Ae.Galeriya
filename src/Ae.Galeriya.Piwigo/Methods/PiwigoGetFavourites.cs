using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
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
        private readonly GaleriyaDbContext _dbContext;
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;

        public string MethodName => "pwg.users.favorites.getList";
        public bool AllowAnonymous => false;

        public PiwigoGetFavourites(ICategoryPermissionsRepository permissionsRepository, GaleriyaDbContext dbContext, IPiwigoPhotosPageGenerator pageGenerator)
        {
            _permissionsRepository = permissionsRepository;
            _dbContext = dbContext;
            _pageGenerator = pageGenerator;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            user = await _dbContext.Users.Include(x => x.FavouritePhotos).SingleAsync(x => x == user, token);

            var photos = (await _permissionsRepository.GetAccessiblePhotos(user, token))
                .Where(x => user.FavouritePhotos.Contains(x));

            return await _pageGenerator.CreatePage(0, 0, photos, token);
        }
    }
}
