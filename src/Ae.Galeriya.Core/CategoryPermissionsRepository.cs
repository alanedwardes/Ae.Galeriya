using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        private async Task<IReadOnlyList<Category>> GetAllCategories(CancellationToken token)
        {
            return await _dbContext.Categories
                .Include(x => x.ParentCategory)
                .Include(x => x.Photos)
                .Include(x => x.Categories)
                .Include(x => x.Users)
                .AsSplitQuery()
                .ToArrayAsync(token);
        }

        public async Task<IReadOnlyCollection<Category>> GetAccessibleCategories(User user, CancellationToken token)
        {
            return (await GetAllCategories(token)).Where(x => CanAccessCategory(user, x)).ToArray();
        }

        public bool CanAccessCategory(User user, Category category)
        {
            var relevantCategory = category.IsTopLevel() ? category : category.FindTopLevel();
            return relevantCategory.Users.Contains(user);
        }

        private static void ThrowForNoAccess(User user, Category category)
        {
            throw new InvalidOperationException($"User {user} cannot access category {category}")
            {
                HResult = 403
            };
        }

        public async Task<Category> EnsureCanAccessCategory(User user, uint categoryId, CancellationToken token)
        {
            var category = (await GetAllCategories(token)).SingleOrDefault(x => x.CategoryId == categoryId);
            if (category == null)
            {
                ThrowForNoAccess(user, category);
            }

            if (!CanAccessCategory(user, category))
            {
                ThrowForNoAccess(user, category);
            }
            return category;
        }

        public async Task<Photo> EnsureCanAccessPhoto(User user, uint photoId, CancellationToken token)
        {
            var photo = await (await GetAccessiblePhotos(user, token)).SingleOrDefaultAsync(x => x.PhotoId == photoId, token);
            if (photo == null)
            {
                throw new InvalidOperationException($"User {user} cannot access photo {photoId}")
                {
                    HResult = 403
                };
            }
            return photo;
        }

        public async Task<IQueryable<Photo>> GetAccessiblePhotos(User user, CancellationToken token)
        {
            var acessibleCategories = await GetAccessibleCategories(user, token);
            return _dbContext.Photos.Where(photo => photo.Categories.Any(category => acessibleCategories.Contains(category)))
                .OrderByDescending(x => x.PhotoId);
        }
    }
}
