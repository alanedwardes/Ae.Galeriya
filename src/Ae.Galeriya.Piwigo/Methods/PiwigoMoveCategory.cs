using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoMoveCategory : IPiwigoWebServiceMethod
    {
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.categories.move";
        public bool AllowAnonymous => false;

        public PiwigoMoveCategory(ICategoryPermissionsRepository categoryPermissions, IServiceProvider serviceProvider)
        {
            _categoryPermissions = categoryPermissions;
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var newParentCategoryId = parameters.GetRequired<uint>("parent");

            var allCategories = await _categoryPermissions.GetAccessibleCategories(context, userId.Value).ToArrayAsync(token);

            var category = allCategories.Single(x => x.CategoryId == parameters.GetRequired<uint>("category_id"));
            var newParentCategory = newParentCategoryId > 0 ? allCategories.Single(x => x.CategoryId == newParentCategoryId) : null;

            if (newParentCategory == null)
            {
                category.ParentCategoryId = null;
                // Users stay the same
            }
            else
            {
                category.ParentCategory = newParentCategory;
                category.Users = newParentCategory.Users;
            }

            await context.SaveChangesAsync(token);
            return null;
        }
    }
}
