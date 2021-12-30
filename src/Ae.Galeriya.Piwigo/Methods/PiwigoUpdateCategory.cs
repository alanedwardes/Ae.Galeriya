using Ae.Galeriya.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoUpdateCategory : IPiwigoWebServiceMethod
    {
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.categories.setInfo";
        public bool AllowAnonymous => false;

        public PiwigoUpdateCategory(ICategoryPermissionsRepository categoryPermissions, IServiceProvider serviceProvider)
        {
            _categoryPermissions = categoryPermissions;
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, IFormFile> fileParameters, uint? userId, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var category = await _categoryPermissions.EnsureCanAccessCategory(context, userId.Value, parameters.GetRequired<uint>("category_id"), token);

            if (parameters.TryGetOptional<string>("name", out var name))
            {
                category.Name = name;
            }

            if (parameters.TryGetOptional<string>("comment", out var comment))
            {
                category.Comment = comment;
            }

            category.UpdatedById = userId;
            category.UpdatedOn = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync(token);
            return null;
        }
    }
}
