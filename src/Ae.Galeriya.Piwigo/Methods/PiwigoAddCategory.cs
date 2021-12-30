using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.WebUtilities;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoAddCategory : IPiwigoWebServiceMethod
    {
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.categories.add";
        public bool AllowAnonymous => false;

        public PiwigoAddCategory(ICategoryPermissionsRepository categoryPermissions, IServiceProvider serviceProvider)
        {
            _categoryPermissions = categoryPermissions;
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, FileMultipartSection> fileParameters, uint? userId, CancellationToken token)
        {
            var parentId = parameters.GetRequired<uint>("parent");

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            Category parentCategory = null;
            if (parentId > 0)
            {
                parentCategory = await _categoryPermissions.EnsureCanAccessCategory(context, userId.Value, parentId, token);
            }

            var user = await context.Users.SingleAsync(x => x.Id == userId.Value);

            var category = new Category
            {
                Name = parameters.GetRequired<string>("name"),
                ParentCategory = parentCategory,
                Comment = parameters.GetOptional("comment"),
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedById = userId.Value,
                Users = new[] { user }
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync(token);
            return new PiwigoAddedCategoryResponse
            {
                Info = "Album added",
                Id = category.CategoryId
            };
        }
    }
}
