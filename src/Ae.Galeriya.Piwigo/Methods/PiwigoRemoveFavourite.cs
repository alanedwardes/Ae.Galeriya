using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoRemoveFavourite : IPiwigoWebServiceMethod
    {
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IServiceProvider _serviceProvider;

        public bool AllowAnonymous => false;

        public string MethodName => "pwg.users.favorites.remove";

        public PiwigoRemoveFavourite(ICategoryPermissionsRepository categoryPermissions, IServiceProvider serviceProvider)
        {
            _categoryPermissions = categoryPermissions;
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var photo = await _categoryPermissions.EnsureCanAccessPhoto(context, userId.Value, parameters.GetRequired<uint>("image_id"), token);

            var user = await context.Users.Include(x => x.FavouritePhotos).SingleAsync(x => x.Id == userId, token);

            if (user.FavouritePhotos.Contains(photo))
            {
                user.FavouritePhotos.Remove(photo);
                await context.SaveChangesAsync();
            }

            return true;
        }
    }
}
