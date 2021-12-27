using Ae.Galeriya.Core;
using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoUploadMethod : IPiwigoWebServiceMethod
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IPhotoCreator _photoCreator;
        private readonly IPiwigoConfiguration _piwigoConfiguration;

        public string MethodName => "pwg.images.upload";
        public bool AllowAnonymous => false;

        public PiwigoUploadMethod(IHttpContextAccessor contextAccessor,
            IServiceProvider serviceProvider,
            ICategoryPermissionsRepository categoryPermissions,
            IPhotoCreator photoCreator,
            IPiwigoConfiguration piwigoConfiguration)
        {
            _contextAccessor = contextAccessor;
            _serviceProvider = serviceProvider;
            _categoryPermissions = categoryPermissions;
            _photoCreator = photoCreator;
            _piwigoConfiguration = piwigoConfiguration;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            var category = await _categoryPermissions.EnsureCanAccessCategory(userId.Value, parameters.GetRequired<uint>("category"), token);

            var name = parameters.GetRequired<string>("name");

            var file = _contextAccessor.HttpContext.Request.Form.Files.Single();

            var uploadedFile = _piwigoConfiguration.TemporaryBlobRepository(_serviceProvider).GetFileInfoForBlob(Guid.NewGuid().ToString());

            using (var write = uploadedFile.OpenWrite())
            {
                await file.CopyToAsync(write, token);
            }

            var photo = await _photoCreator.CreatePhoto(_piwigoConfiguration.TemporaryBlobRepository(_serviceProvider), _piwigoConfiguration.PersistentBlobRepository(_serviceProvider), category, file.FileName, name, userId.Value, DateTimeOffset.UtcNow, uploadedFile, token);

            return new PiwigoUploadedResponse
            {
                ImageId = photo.PhotoId,
                Category = new PiwigoUploadedCategory
                {
                    CategoryId = category.CategoryId,
                    NumberOfPhotos = (uint)category.Photos.Count,
                    Name = category.Name
                }
            };
        }
    }
}
