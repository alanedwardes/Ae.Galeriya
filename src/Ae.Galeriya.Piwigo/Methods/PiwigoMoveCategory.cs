using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoMoveCategory : IPiwigoWebServiceMethod
    {
        private readonly GaleriaDbContext _context;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.categories.move";
        public bool AllowAnonymous => false;

        public PiwigoMoveCategory(GaleriaDbContext context, ICategoryPermissionsRepository categoryPermissions)
        {
            _context = context;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var newParentCategoryId = parameters["parent"].ToUInt32(null);

            var category = await _categoryPermissions.EnsureCanAccessCategory(user, parameters["category_id"].ToUInt32(null), token);
            var newParentCategory = newParentCategoryId > 0 ? await _categoryPermissions.EnsureCanAccessCategory(user, newParentCategoryId, token) : null;

            if (newParentCategory == null)
            {
                category.ParentCategory = null;
                category.ParentCategoryId = null;
            }
            else
            {
                category.ParentCategory = newParentCategory;
            }

            await _context.SaveChangesAsync(token);
            return null;
        }
    }
}
