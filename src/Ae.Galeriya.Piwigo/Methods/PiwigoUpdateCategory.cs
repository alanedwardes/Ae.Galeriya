using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoUpdateCategory : IPiwigoWebServiceMethod
    {
        private readonly GaleriaDbContext _context;

        public string MethodName => "pwg.categories.setInfo";
        public bool AllowAnonymous => false;

        public PiwigoUpdateCategory(GaleriaDbContext context)
        {
            _context = context;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var categoryId = parameters["category_id"].ToUInt32(null);

            var category = await _context.Categories.SingleAsync(x => x.CategoryId == categoryId, token);
            
            if (parameters.TryGetValue("name", out var name))
            {
                category.Name = name.ToString(null);
            }

            if (parameters.TryGetValue("comment", out var comment))
            {
                category.Comment = comment.ToString(null);
            }

            await _context.SaveChangesAsync(token);
            return null;
        }
    }
}
