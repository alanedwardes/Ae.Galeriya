using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    internal sealed class CategoryPermissionsRepository : ICategoryPermissionsRepository
    {
        private readonly GaleriyaDbContext _dbContext;

        public CategoryPermissionsRepository(GaleriyaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<Category> GetCategories()
        {
            return _dbContext.Categories
                .Include(x => x.ParentCategory)
                .Include(x => x.Photos)
                .Include(x => x.Categories)
                .Include(x => x.Users);
        }

        private IQueryable<Photo> GetPhotos()
        {
            return _dbContext.Photos
                .Include(x => x.Categories)
                .ThenInclude(x => x.Users)
                .Include(x => x.Tags);
        }

        public IQueryable<Category> GetAccessibleCategories(uint userId)
        {
            return GetCategories().Where(x => x.Users.Select(y => y.Id).Contains(userId));
        }

        public IQueryable<Photo> GetAccessiblePhotos(uint userId)
        {
            return GetPhotos().Where(x => x.Categories.Any(y => y.Users.Select(z => z.Id).Contains(userId)));
        }

        private static bool CanAccessCategory(uint userId, Category category)
        {
            return category.Users.Select(x => x.Id).Contains(userId);
        }

        private static bool CanAccessPhoto(uint userId, Photo photo)
        {
            return photo.Categories.Any(x => CanAccessCategory(userId, x));
        }

        public async Task<Category> EnsureCanAccessCategory(uint userId, uint categoryId, CancellationToken token)
        {
            var category = await GetCategories().SingleOrDefaultAsync(x => x.CategoryId == categoryId, token);
            if (category == null || !CanAccessCategory(userId, category))
            {
                throw new InvalidOperationException($"User {userId} cannot access category {categoryId}")
                {
                    HResult = 403
                };
            }
            return category;
        }

        public async Task<Photo> EnsureCanAccessPhoto(uint userId, uint photoId, CancellationToken token)
        {
            var photo = await GetPhotos().SingleOrDefaultAsync(x => x.PhotoId == photoId, token);
            if (photo == null || !CanAccessPhoto(userId, photo))
            {
                throw new InvalidOperationException($"User {userId} cannot access photo {photoId}")
                {
                    HResult = 403
                };
            }
            return photo;
        }
    }
}
