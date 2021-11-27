using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Ae.Galeriya.Web.Models
{
    public class EditCategoryModel
    {
        public Category Category { get; set; }
        public IReadOnlyList<IdentityUser> Users { get; set; } = Array.Empty<IdentityUser>();
    }
}
