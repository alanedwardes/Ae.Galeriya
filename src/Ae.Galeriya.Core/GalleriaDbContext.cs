using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;

namespace Ae.Galeriya.Core
{
    public sealed class GalleriaDbContext : DbContext
    {
        public GalleriaDbContext(DbContextOptions<GalleriaDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
