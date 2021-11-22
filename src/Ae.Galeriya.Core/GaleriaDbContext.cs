using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ae.Galeriya.Core
{
    public sealed class GaleriaDbContext : IdentityDbContext
    {
        public GaleriaDbContext(DbContextOptions<GaleriaDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(category => category.CoverPhoto)
                .WithOne();

            modelBuilder.Entity<Category>()
                .HasOne(category => category.ParentCategory)
                .WithOne();

            modelBuilder.Entity<Category>()
                .HasMany(category => category.Photos)
                .WithMany(photo => photo.Categories);
        }

        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Photo> Photos => Set<Photo>();
        public DbSet<Category> Categories => Set<Category>();
    }
}
