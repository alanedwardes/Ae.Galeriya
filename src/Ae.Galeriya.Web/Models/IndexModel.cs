using Ae.Galeriya.Core.Tables;
using System.Collections.Generic;

namespace Ae.Galeriya.Web.Models
{
    public class IndexModel
    {
        public IReadOnlyList<User> Users { get; set; }
        public IReadOnlyList<Category> Categories { get; set; }
    }
}
