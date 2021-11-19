using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoAddCategory : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _context;

        public string MethodName => "pwg.categories.add";

        public PiwigoAddCategory(GalleriaDbContext context)
        {
            _context = context;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var parentId = parameters["parent"].ToUInt32(null);

            Category parentCategory = null;
            if (parentId > 0)
            {
                parentCategory = await _context.Categories.SingleAsync(x => x.CategoryId == parentId, token);
            }

            var category = new Category
            {
                Name = parameters["name"].ToString(null),
                ParentCategory = parentCategory,
                Comment = parameters["comment"].ToString(null),
                //Visible = parameters["visible"].ToBoolean(null),
                Status = parameters["status"].ToString(null),
                //Commentable = parameters["commentable"].ToBoolean(null)
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
