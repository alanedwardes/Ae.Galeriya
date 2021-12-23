using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    internal sealed class CategoryPermissionsRepository : ICategoryPermissionsRepository
    {
        private readonly ILogger<CategoryPermissionsRepository> _logger;
        private readonly GaleriyaDbContext _dbContext;

        public CategoryPermissionsRepository(ILogger<CategoryPermissionsRepository> logger, GaleriyaDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private async Task<IReadOnlyList<Category>> GetAllCategories(CancellationToken token)
        {
            return await _dbContext.Categories
                .Include(x => x.ParentCategory)
                .Include(x => x.Photos)
                .Include(x => x.Categories)
                .Include(x => x.Users)
                .ToArrayAsync(token);
        }

        public async Task<IReadOnlyCollection<Category>> GetAccessibleCategories(uint userId, CancellationToken token)
        {
            return (await GetAllCategories(token)).Where(x => CanAccessCategory(userId, x)).ToArray();
        }

        public bool CanAccessCategory(uint userId, Category category)
        {
            var relevantCategory = category.IsTopLevel() ? category : category.FindTopLevel();
            return relevantCategory.Users.Select(x => x.Id).Contains(userId);
        }

        public async Task<Category> EnsureCanAccessCategory(uint userId, uint categoryId, CancellationToken token)
        {
            var category = (await GetAllCategories(token)).SingleOrDefault(x => x.CategoryId == categoryId);
            if (category == null)
            {
                throw new InvalidOperationException($"User {userId} cannot access category {categoryId}")
                {
                    HResult = 403
                };
            }

            if (!CanAccessCategory(userId, category))
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
            var sw = Stopwatch.StartNew();
            var photo = await (await GetAccessiblePhotos(userId, token)).SingleOrDefaultAsync(x => x.PhotoId == photoId, token);
            _logger.LogInformation("Got accessible photos in {TotalSeconds}", sw.Elapsed.TotalSeconds);
            if (photo == null)
            {
                throw new InvalidOperationException($"User {userId} cannot access photo {photoId}")
                {
                    HResult = 403
                };
            }
            return photo;
        }

        public async Task<IQueryable<Photo>> GetAccessiblePhotos(uint userId, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();
            var acessibleCategoryIds = (await GetAccessibleCategories(userId, token)).Select(x => x.CategoryId).ToArray();
            _logger.LogInformation("Got accessible categories in {TotalSeconds}", sw.Elapsed.TotalSeconds);
            return _dbContext.Photos.Where(photo => photo.Categories.Select(x => x.CategoryId).Any(acessibleCategoryIds.Contains))
                                    .Include(x => x.Categories)
                                    .Include(x => x.Tags);
        }
    }
}
