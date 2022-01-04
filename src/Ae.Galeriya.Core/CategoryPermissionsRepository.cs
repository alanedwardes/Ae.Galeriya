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
        private IQueryable<Category> GetCategories(GaleriyaDbContext dbContext)
        {
            return dbContext.Categories
                .Include(x => x.ParentCategory)
                .Include(x => x.Categories)
                .Include(x => x.Users);
        }

        private IQueryable<Photo> GetPhotos(GaleriyaDbContext dbContext)
        {
            return dbContext.Photos
                .Include(x => x.Categories)
                .ThenInclude(x => x.Users)
                .Include(x => x.Tags);
        }

        private IQueryable<PhotoSummary> GetPhotoSummaries(IQueryable<Photo> photos)
        {
            return photos.Select(x => new PhotoSummary
            {
                BlobId = x.BlobId,
                PhotoId = x.PhotoId,
                FileName = x.FileName,
                Categories = x.Categories,
                CreatedOn = x.CreatedOn,
                Tags = x.Tags,
                Comment = x.Comment,
                Name = x.Name,
                ColourB = x.ColourB,
                ColourG = x.ColourG,
                ColourR = x.ColourR,
                Duration = x.Duration,
                Extension = x.Extension,
                FileCreatedOn = x.FileCreatedOn,
                HasThumbnail = x.HasThumbnail,
                Height = x.Height,
                Orientation = x.Orientation,
                TakenOn = x.TakenOn,
                Width = x.Width
            });
        }

        public IQueryable<Category> GetAccessibleCategories(GaleriyaDbContext dbContext, uint userId)
        {
            return GetCategories(dbContext).Where(x => x.Users.Select(y => y.Id).Contains(userId));
        }

        public IQueryable<PhotoSummary> GetAccessiblePhotos(GaleriyaDbContext dbContext, uint userId)
        {
            return GetPhotoSummaries(GetPhotos(dbContext).Where(x => x.Categories.Any(y => y.Users.Select(z => z.Id).Contains(userId))));
        }

        private static bool CanAccessCategory(uint userId, Category category)
        {
            return category.Users.Select(x => x.Id).Contains(userId);
        }

        private static bool CanAccessPhoto(uint userId, Photo photo)
        {
            return photo.Categories.Any(x => CanAccessCategory(userId, x));
        }

        public async Task<Category> EnsureCanAccessCategory(GaleriyaDbContext dbContext, uint userId, uint categoryId, CancellationToken token)
        {
            var category = await GetCategories(dbContext).SingleOrDefaultAsync(x => x.CategoryId == categoryId, token);
            if (category == null || !CanAccessCategory(userId, category))
            {
                throw new InvalidOperationException($"User {userId} cannot access category {categoryId}")
                {
                    HResult = 403
                };
            }
            return category;
        }

        public async Task<Photo> EnsureCanAccessPhoto(GaleriyaDbContext dbContext, uint userId, uint photoId, CancellationToken token)
        {
            var photo = await GetPhotos(dbContext).SingleOrDefaultAsync(x => x.PhotoId == photoId, token);
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
