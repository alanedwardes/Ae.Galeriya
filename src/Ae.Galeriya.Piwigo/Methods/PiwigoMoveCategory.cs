using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var allCategories = await _categoryPermissions.GetAccessibleCategories(user, token);

            var category = allCategories.Single(x => x.CategoryId == parameters["category_id"].ToUInt32(null));
            var newParentCategory = newParentCategoryId > 0 ? allCategories.Single(x => x.CategoryId == newParentCategoryId) : null;

            if (newParentCategory == null)
            {
                category.Users = !category.IsTopLevel() ? category.FindTopLevel().Users : new List<User>();
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
