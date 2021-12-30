using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Ae.Galeriya.Core.Tables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoSetImageInfo : IPiwigoWebServiceMethod
    {
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.images.setInfo";
        public bool AllowAnonymous => false;

        public PiwigoSetImageInfo(ICategoryPermissionsRepository categoryPermissions, IServiceProvider serviceProvider)
        {
            _categoryPermissions = categoryPermissions;
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, IFormFile> fileParameters, uint? userId, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var photo = await _categoryPermissions.EnsureCanAccessPhoto(context, userId.Value, parameters.GetRequired<uint>("image_id"), token);

            var multipleValueMode = parameters.GetRequired<string>("multiple_value_mode");

            if (parameters.TryGetOptional<string>("name", out var nameString))
            {
                photo.Name = nameString;
            }

            if (parameters.TryGetOptional<string>("comment", out var commentString))
            {
                photo.Comment = commentString;
            }

            if (parameters.TryGetOptional<string>("categories", out var categoriesString))
            {
                var categoryIds = categoriesString.Split(";").Select(uint.Parse).ToArray();

                var categories = await context.Categories.Where(x => categoryIds.Contains(x.CategoryId)).ToArrayAsync(token);

                if (multipleValueMode == "replace")
                {
                    photo.Categories = categories;
                }
                else
                {
                    throw new NotImplementedException($"Mode not implemented: {multipleValueMode}");
                }
            }

            if (parameters.TryGetOptional<string>("tag_ids", out var tagsString))
            {
                var tags = Array.Empty<Tag>();
                if (!string.IsNullOrWhiteSpace(tagsString))
                {
                    var tagIds = tagsString.Split(";").Select(uint.Parse).ToArray();

                    tags = await context.Tags.Where(x => tagIds.Contains(x.TagId)).ToArrayAsync(token);
                }

                if (multipleValueMode == "replace")
                {
                    photo.Tags = tags;
                }
                else
                {
                    throw new NotImplementedException($"Mode not implemented: {multipleValueMode}");
                }
            }

            photo.UpdatedOn = DateTimeOffset.UtcNow;
            photo.UpdatedById = userId;
            await context.SaveChangesAsync(token);
            return null;
        }
    }
}
