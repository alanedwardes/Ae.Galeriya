using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoUpdateCategory : IPiwigoWebServiceMethod
    {
        private readonly GaleriyaDbContext _context;
        private readonly ICategoryPermissionsRepository _categoryPermissions;

        public string MethodName => "pwg.categories.setInfo";
        public bool AllowAnonymous => false;

        public PiwigoUpdateCategory(GaleriyaDbContext context, ICategoryPermissionsRepository categoryPermissions)
        {
            _context = context;
            _categoryPermissions = categoryPermissions;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var category = await _categoryPermissions.EnsureCanAccessCategory(user, parameters["category_id"].ToUInt32(null), token);

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
