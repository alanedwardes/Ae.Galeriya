using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Web.Controllers
{
    [Authorize]
    [Route("/api/v1")]
    public sealed class ApiController : Controller
    {
        private readonly IPiwigoConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IPhotoCreator _photoCreator;

        public ApiController(IPiwigoConfiguration configuration, IServiceProvider serviceProvider, ICategoryPermissionsRepository categoryPermissions, IPhotoCreator photoCreator)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _categoryPermissions = categoryPermissions;
            _photoCreator = photoCreator;
        }

        [HttpPost("hashes:query")]
        public async Task<string[]> QueryHashes([FromBody] string[] hashes, CancellationToken token)
        {
            using (var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>())
            {
                return await context.Photos.Where(x => hashes.Contains(x.BlobId))
                                           .Select(x => x.BlobId)
                                           .ToArrayAsync(token);
            }
        }

        [HttpPut("photos:upload")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task UploadPhoto([FromForm(Name = "file")] IFormFile file, [FromForm(Name = "categoryId")] uint categoryId, [FromForm(Name = "name")] string name, [FromForm(Name = "createdOn")] DateTimeOffset createdOn, CancellationToken token)
        {
            var userId = HttpContext.User.Identity.GetUserId();

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var category = await _categoryPermissions.EnsureCanAccessCategory(context, userId, categoryId, token);

            var fileBlobRepository = _configuration.TemporaryBlobRepository(_serviceProvider);

            var fileInfo = fileBlobRepository.GetFileInfoForBlob(Guid.NewGuid().ToString());

            using (var writeStream = fileInfo.OpenWrite())
            using (var readStream = file.OpenReadStream())
            {
                await readStream.CopyToAsync(writeStream);
            }

            await _photoCreator.CreatePhoto(context, fileBlobRepository, _configuration.PersistentBlobRepository(_serviceProvider), category, name, name, userId, createdOn, fileInfo, token);
        }

        private Category CreateCategoryHeirarchyFromPath(GaleriyaDbContext context, ISet<Category> categories, string path, User user)
        {
            Category previousCategory = null;
            foreach (var item in path.Split('/'))
            {
                var category = categories.SingleOrDefault(x => x.Name == item && x.ParentCategory == previousCategory);
                if (category == null)
                {
                    category = new Category
                    {
                        Name = item,
                        ParentCategory = previousCategory,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = user,
                        Users = previousCategory == null ? new [] { user } : previousCategory.Users
                    };
                    categories.Add(category);
                    context.Categories.Add(category);
                }
                previousCategory = category;
            }

            return previousCategory;
        }

        [HttpPut("photos:paths")]
        public async Task CategorisePhotos([FromBody] Dictionary<string, string> hashPaths)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var user = await context.Users.SingleAsync(x => x.Id == HttpContext.User.Identity.GetUserId(), HttpContext.RequestAborted);

            var groups = hashPaths.GroupBy(x => Path.GetDirectoryName(x.Value))
                .ToDictionary(x => x.Key, x => x.Select(x => x.Key).ToArray());

            var allCategories = groups.Keys.SelectMany(x => x.Split('/')).ToHashSet();
            var allHashes = groups.Values.SelectMany(x => x).ToHashSet();

            var categories = new HashSet<Category>(await _categoryPermissions.GetAccessibleCategories(context, user.Id).ToArrayAsync(HttpContext.RequestAborted));

            var photos = await _categoryPermissions.GetAccessiblePhotos(context, user.Id)
                .Where(x => allHashes.Contains(x.BlobId))
                .ToArrayAsync(HttpContext.RequestAborted);

            foreach (var group in groups)
            {
                var category = CreateCategoryHeirarchyFromPath(context, categories, group.Key, user);

                foreach (var photo in photos.Where(x => group.Value.Contains(x.BlobId)))
                {
                    if (!category.Photos.Select(x => x.PhotoId).Contains(photo.PhotoId))
                    {
                        //category.Photos.Add(photo);
                    }
                }
            }

            await context.SaveChangesAsync(HttpContext.RequestAborted);
        }
    }
}
