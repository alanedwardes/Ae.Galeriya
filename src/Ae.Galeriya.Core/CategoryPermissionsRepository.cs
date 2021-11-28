﻿using Ae.Galeriya.Core.Tables;
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
        private readonly GaleriaDbContext _dbContext;

        public CategoryPermissionsRepository(GaleriaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static bool IsTopLevel(Category category)
        {
            return category.ParentCategoryId == null;
        }

        private static Category FindTopLevel(Category start)
        {
            var current = start;

            while (!IsTopLevel(current!))
            {
                current = current!.ParentCategory;
            }

            return current!;
        }

        private async Task<IReadOnlyList<Category>> GetAllCategories(CancellationToken token)
        {
            return await _dbContext.Categories.Include(x => x.ParentCategory)
                .Include(x => x.Photos)
                .Include(x => x.Categories)
                .Include(x => x.Users)
                .AsSplitQuery()
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Category>> GetAccessibleCategories(User user, CancellationToken token)
        {
            return (await GetAllCategories(token)).Where(x => CanAccessCategory(user, x)).ToArray();
        }

        public bool CanAccessCategory(User user, Category category)
        {
            var relevantCategory = IsTopLevel(category) ? category : FindTopLevel(category);
            return relevantCategory.Users.Contains(user);
        }

        public bool CanAccessPhoto(User user, Photo photo)
        {
            foreach (var category in photo.Categories)
            {
                if (CanAccessCategory(user, category))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ThrowForNoAccess(User user, Category category)
        {
            throw new InvalidOperationException($"User {user} cannot access category {category}")
            {
                HResult = 403
            };
        }

        private static void ThrowForNoAccess(User user, Photo photo)
        {
            throw new InvalidOperationException($"User {user} cannot access photo {photo}")
            {
                HResult = 403
            };
        }

        public void EnsureCanAccessCategory(User user, Category? category)
        {
            if (category == null)
            {
                ThrowForNoAccess(user, category);
            }

            if (!CanAccessCategory(user, category))
            {
                ThrowForNoAccess(user, category);
            }
        }

        public void EnsureCanAccessPhoto(User user, Photo? photo)
        {
            if (photo == null)
            {
                ThrowForNoAccess(user, photo);
            }

            if (!CanAccessPhoto(user, photo))
            {
                ThrowForNoAccess(user, photo);
            }
        }

        public async Task<Category> EnsureCanAccessCategory(User user, uint categoryId, CancellationToken token)
        {
            var category = (await GetAllCategories(token)).SingleOrDefault(x => x.CategoryId == categoryId);
            EnsureCanAccessCategory(user, category);
            return category;
        }

        public async Task<Photo> EnsureCanAccessPhoto(User user, uint photoId, CancellationToken token)
        {
            await GetAllCategories(token);
            var photo = await _dbContext.Photos.SingleOrDefaultAsync(x => x.PhotoId == photoId, token);
            EnsureCanAccessPhoto(user, photo);
            return photo;
        }
    }
}
