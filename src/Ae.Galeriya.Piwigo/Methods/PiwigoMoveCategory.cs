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
    internal sealed class PiwigoMoveCategory : IPiwigoWebServiceMethod
    {
        private readonly GaleriyaDbContext _context;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.categories.move";
        public bool AllowAnonymous => false;

        public PiwigoMoveCategory(GaleriyaDbContext context, ICategoryPermissionsRepository categoryPermissions)
        {
            _context = context;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            var newParentCategoryId = parameters.GetRequired<uint>("parent");

            var allCategories = await _categoryPermissions.GetAccessibleCategories(userId.Value).ToArrayAsync(token);

            var category = allCategories.Single(x => x.CategoryId == parameters.GetRequired<uint>("category_id"));
            var newParentCategory = newParentCategoryId > 0 ? allCategories.Single(x => x.CategoryId == newParentCategoryId) : null;

            if (newParentCategory == null)
            {
                category.Users = category.Users;
                category.ParentCategoryId = null;
            }
            else
            {
                category.ParentCategory = newParentCategory;
                category.Users = new List<User>();
            }

            await _context.SaveChangesAsync(token);
            return null;
        }
    }
}
