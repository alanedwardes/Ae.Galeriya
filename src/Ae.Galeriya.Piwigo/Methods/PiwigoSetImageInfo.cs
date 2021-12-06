using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Ae.Galeriya.Core.Tables;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoSetImageInfo : IPiwigoWebServiceMethod
    {
        private readonly GaleriyaDbContext _context;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.images.setInfo";
        public bool AllowAnonymous => false;

        public PiwigoSetImageInfo(GaleriyaDbContext context, ICategoryPermissionsRepository categoryPermissions)
        {
            _context = context;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var photo = await _categoryPermissions.EnsureCanAccessPhoto(user, parameters.GetRequiredValue<uint>("image_id"), token);

            var multipleValueMode = parameters.GetRequiredValue<string>("multiple_value_mode");

            if (parameters.TryGetOptionalValue<string>("name", out var nameString))
            {
                photo.Name = nameString;
            }

            if (parameters.TryGetOptionalValue<string>("comment", out var commentString))
            {
                photo.Comment = commentString;
            }

            if (parameters.TryGetOptionalValue<string>("categories", out var categoriesString))
            {
                var categoryIds = categoriesString.Split(";").Select(uint.Parse).ToArray();

                var categories = await _context.Categories.Where(x => categoryIds.Contains(x.CategoryId)).ToArrayAsync(token);

                if (multipleValueMode == "replace")
                {
                    photo.Categories = categories;
                }
                else
                {
                    throw new NotImplementedException($"Mode not implemented: {multipleValueMode}");
                }
            }

            if (parameters.TryGetOptionalValue<string>("tag_ids", out var tagsString))
            {
                var tags = Array.Empty<Tag>();
                if (!string.IsNullOrWhiteSpace(tagsString))
                {
                    var tagIds = tagsString.Split(";").Select(uint.Parse).ToArray();

                    tags = await _context.Tags.Where(x => tagIds.Contains(x.TagId)).ToArrayAsync(token);
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

            await _context.SaveChangesAsync(token);
            return null;
        }
    }
}
