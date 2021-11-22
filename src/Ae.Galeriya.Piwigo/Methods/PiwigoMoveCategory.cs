using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoMoveCategory : IPiwigoWebServiceMethod
    {
        private readonly GaleriaDbContext _context;

        public string MethodName => "pwg.categories.move";

        public PiwigoMoveCategory(GaleriaDbContext context)
        {
            _context = context;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var categoryId = parameters["category_id"].ToUInt32(null);
            var newParentCategoryId = parameters["parent"].ToUInt32(null);

            var category = await _context.Categories.SingleAsync(x => x.CategoryId == categoryId, token);

            if (newParentCategoryId > 0)
            {
                var newParentCategory = await _context.Categories.SingleAsync(x => x.CategoryId == newParentCategoryId, token);
                category.ParentCategory = newParentCategory;
            }
            else
            {
                category.ParentCategory = null;
                category.ParentCategoryId = null;
            }

            await _context.SaveChangesAsync(token);
            return null;
        }
    }
}
