using Ae.Galeriya.Core;
using Ae.Galeriya.Piwigo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
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
    }
}
