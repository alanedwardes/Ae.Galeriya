using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ae.Galeriya.Core
{
    public sealed class GaleriaDbContext : IdentityDbContext<User, Role, uint>
    {
        public GaleriaDbContext(DbContextOptions<GaleriaDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(category => category.CreatedBy);

            modelBuilder.Entity<Category>()
                .HasOne(category => category.UpdatedBy);

            modelBuilder.Entity<User>()
                .HasMany(user => user.CreatedCategories)
                .WithOne(category => category.CreatedBy);

            modelBuilder.Entity<Photo>()
                .HasOne(photo => photo.CreatedBy);

            modelBuilder.Entity<Photo>()
                .HasOne(photo => photo.UpdatedBy);

            modelBuilder.Entity<User>()
                .HasMany(user => user.CreatedPhotos)
                .WithOne(photo => photo.CreatedBy);

            modelBuilder.Entity<Tag>()
                .HasOne(tag => tag.CreatedBy);

            modelBuilder.Entity<Tag>()
                .HasOne(tag => tag.UpdatedBy);

            modelBuilder.Entity<User>()
                .HasMany(user => user.CreatedTags)
                .WithOne(tag => tag.CreatedBy);

            modelBuilder.Entity<Category>()
                .HasOne(category => category.CoverPhoto)
                .WithOne();

            modelBuilder.Entity<Category>()
                .HasOne(category => category.ParentCategory)
                .WithOne();

            modelBuilder.Entity<Category>()
                .HasMany(category => category.Photos)
                .WithMany(photo => photo.Categories);

            modelBuilder.Entity<Category>()
                .HasMany(category => category.Users)
                .WithMany(user => user.AccessibleCategories);
        }

        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Photo> Photos => Set<Photo>();
        public DbSet<Category> Categories => Set<Category>();
    }
}
