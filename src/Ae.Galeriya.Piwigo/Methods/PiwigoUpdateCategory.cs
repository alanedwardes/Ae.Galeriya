﻿using Ae.Galeriya.Core;
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

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            var category = await _categoryPermissions.EnsureCanAccessCategory(userId.Value, parameters.GetRequired<uint>("category_id"), token);

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
            await _context.SaveChangesAsync(token);
            return null;
        }
    }
}
