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
                .Include(x => x.Tags);
        }

        public IQueryable<Category> GetAccessibleCategories(uint userId)
        {
            return GetCategories().Where(x => x.Users.Select(y => y.Id).Contains(userId));
        }

        private bool CanAccessCategory(uint userId, Category category)
        {
            return category.Users.Select(x => x.Id).Contains(userId);
        }

        public async Task<Category> EnsureCanAccessCategory(uint userId, uint categoryId, CancellationToken token)
        {
            var category = await GetCategories().SingleOrDefaultAsync(x => x.CategoryId == categoryId);
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
            var photo = await GetPhotos().SingleOrDefaultAsync(x => x.PhotoId == photoId, token);
            _logger.LogInformation("Photo in {TotalSeconds}", sw.Elapsed.TotalSeconds);
            if (photo == null || !photo.Categories.Any(x => CanAccessCategory(userId, x)))
            {
                throw new InvalidOperationException($"User {userId} cannot access photo {photoId}")
                {
                    HResult = 403
                };
            }
            return photo;
        }

        public IQueryable<Photo> GetAccessiblePhotos(uint userId)
        {
            return GetPhotos().Where(x => x.Categories.Any(y => y.Users.Select(z => z.Id).Contains(userId)));
        }
    }
}
