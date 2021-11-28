using Ae.Galeriya.Core.Tables;
using System;
using System.Collections.Generic;

namespace Ae.Galeriya.Web.Models
{
    public class EditCategoryModel
    {
        public Category Category { get; set; }
        public IReadOnlyList<User> Users { get; set; } = Array.Empty<User>();
    }
}
