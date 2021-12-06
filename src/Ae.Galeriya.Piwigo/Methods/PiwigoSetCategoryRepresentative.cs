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
        private readonly GaleriyaDbContext _context;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.categories.setRepresentative";
        public bool AllowAnonymous => false;

        public PiwigoSetCategoryRepresentative(GaleriyaDbContext context, ICategoryPermissionsRepository categoryPermissions)
        {
            _context = context;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var category = await _categoryPermissions.EnsureCanAccessCategory(user, parameters.GetRequired<uint>("category_id"), token);
            var photo = await _categoryPermissions.EnsureCanAccessPhoto(user, parameters.GetRequired<uint>("image_id"), token);

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
