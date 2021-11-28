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
            var photo = await _categoryPermissions.EnsureCanAccessPhoto(user, parameters["image_id"].ToUInt32(null), token);

            var multipleValueMode = parameters["multiple_value_mode"].ToString(null);

            if (parameters.TryGetValue("name", out var nameString))
            {
                photo.Name = nameString.ToString(null);
            }

            if (parameters.TryGetValue("comment", out var commentString))
            {
                photo.Comment = commentString.ToString(null);
            }

            if (parameters.TryGetValue("categories", out var categoriesString))
            {
                var categoryIds = categoriesString.ToString(null).Split(";").Select(uint.Parse).ToArray();

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

            if (parameters.TryGetValue("tag_ids", out var tagsRaw))
            {
                var tagsString = tagsRaw.ToString(null);

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
