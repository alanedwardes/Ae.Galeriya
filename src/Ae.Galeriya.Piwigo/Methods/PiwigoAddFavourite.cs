using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoAddFavourite : IPiwigoWebServiceMethod
    {
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly GaleriaDbContext _dbContext;

        public bool AllowAnonymous => false;

        public string MethodName => "pwg.users.favorites.add";

        public PiwigoAddFavourite(ICategoryPermissionsRepository categoryPermissions, GaleriaDbContext dbContext)
        {
            _categoryPermissions = categoryPermissions;
            _dbContext = dbContext;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var photo = await _categoryPermissions.EnsureCanAccessPhoto(user, parameters["image_id"].ToUInt32(null), token);

            user = await _dbContext.Users.Include(x => x.FavouritePhotos).SingleAsync(x => x == user, token);

            if (!user.FavouritePhotos.Contains(photo))
            {
                user.FavouritePhotos.Add(photo);
                await _dbContext.SaveChangesAsync();
            }

            return true;
        }
    }
}
