using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Ae.Galeriya.Web.Models
{
    public class IndexModel
    {
        public IReadOnlyList<IdentityUser> Users { get; set; }
        public IReadOnlyList<Category> Categories { get; set; }
    }
}
