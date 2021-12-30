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
    internal sealed class PiwigoSetCategoryRepresentative : IPiwigoWebServiceMethod
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.categories.setRepresentative";
        public bool AllowAnonymous => false;

        public PiwigoSetCategoryRepresentative(IServiceProvider serviceProvider, ICategoryPermissionsRepository categoryPermissions)
        {
            _serviceProvider = serviceProvider;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, IFormFile> fileParameters, uint? userId, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var category = await _categoryPermissions.EnsureCanAccessCategory(context, userId.Value, parameters.GetRequired<uint>("category_id"), token);
            var photo = await _categoryPermissions.EnsureCanAccessPhoto(context, userId.Value, parameters.GetRequired<uint>("image_id"), token);

            if (!category.Photos.Contains(photo))
            {
                throw new InvalidOperationException($"Category {category} does not contain photo {photo}")
                {
                    HResult = 400
                };
            }

            category.UpdatedById = userId;
            category.UpdatedOn = DateTimeOffset.UtcNow;
            category.CoverPhoto = photo;
            await context.SaveChangesAsync(token);
            return null;
        }
    }
}
