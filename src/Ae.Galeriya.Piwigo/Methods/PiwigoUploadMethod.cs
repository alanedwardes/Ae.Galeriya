using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
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
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IPhotoCreator _photoCreator;
        private readonly IPiwigoConfiguration _piwigoConfiguration;

        public string MethodName => "pwg.images.upload";
        public bool AllowAnonymous => false;

        public PiwigoUploadMethod(IHttpContextAccessor contextAccessor,
            ICategoryPermissionsRepository categoryPermissions,
            IPhotoCreator photoCreator,
            IPiwigoConfiguration piwigoConfiguration)
        {
            _contextAccessor = contextAccessor;
            _categoryPermissions = categoryPermissions;
            _photoCreator = photoCreator;
            _piwigoConfiguration = piwigoConfiguration;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var category = await _categoryPermissions.EnsureCanAccessCategory(user, parameters.GetRequired<uint>("category"), token);

            var name = parameters.GetRequired<string>("name");

            var file = _contextAccessor.HttpContext.Request.Form.Files.Single();

            var uploadedFile = _piwigoConfiguration.FileBlobRepository.GetFileInfoForBlob(Guid.NewGuid().ToString());

            using (var write = uploadedFile.OpenWrite())
            {
                await file.CopyToAsync(write, token);
            }

            var photo = await _photoCreator.CreatePhoto(_piwigoConfiguration.FileBlobRepository, category, file.FileName, name, user, DateTimeOffset.UtcNow, uploadedFile, token);

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
