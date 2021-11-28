using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoAddCategory : IPiwigoWebServiceMethod
    {
        private readonly GaleriaDbContext _context;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.categories.add";
        public bool AllowAnonymous => false;

        public PiwigoAddCategory(GaleriaDbContext context, ICategoryPermissionsRepository categoryPermissions)
        {
            _context = context;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var parentId = parameters["parent"].ToUInt32(null);

            Category parentCategory = null;
            if (parentId > 0)
            {
                parentCategory = await _categoryPermissions.EnsureCanAccessCategory(user, parentId, token);
            }

            var category = new Category
            {
                Name = parameters["name"].ToString(null),
                ParentCategory = parentCategory,
                Comment = parameters["comment"].ToString(null),
                CreatedBy = user,
                Users = parentCategory == null ? new[] { user } : Array.Empty<User>()
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync(token);
            return new PiwigoAddedCategoryResponse
            {
                Info = "Album added",
                Id = category.CategoryId
            };
        }
    }
}
