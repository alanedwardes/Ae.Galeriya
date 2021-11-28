using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoSetCategoryRepresentative : IPiwigoWebServiceMethod
    {
        private readonly GaleriaDbContext _context;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.categories.setRepresentative";
        public bool AllowAnonymous => false;

        public PiwigoSetCategoryRepresentative(GaleriaDbContext context, ICategoryPermissionsRepository categoryPermissions)
        {
            _context = context;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var category = await _categoryPermissions.EnsureCanAccessCategory(user, parameters["category_id"].ToUInt32(null), token);
            var photo = await _categoryPermissions.EnsureCanAccessPhoto(user, parameters["image_id"].ToUInt32(null), token);

            if (!category.Photos.Contains(photo))
            {
                throw new InvalidOperationException($"Category {category} does not contain photo {photo}")
                {
                    HResult = 400
                };
            }

            category.CoverPhoto = photo;
            await _context.SaveChangesAsync(token);
            return null;
        }
    }
}
