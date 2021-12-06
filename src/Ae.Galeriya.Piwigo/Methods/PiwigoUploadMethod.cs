using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
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
        private readonly IPiwigoWebServiceMethodRepository _webServiceRepository;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IPhotoCreator _photoCreator;
        private readonly IPiwigoConfiguration _piwigoConfiguration;

        public string MethodName => "pwg.images.upload";
        public bool AllowAnonymous => true;

        public PiwigoUploadMethod(IHttpContextAccessor contextAccessor,
            IPiwigoWebServiceMethodRepository webServiceRepository,
            ICategoryPermissionsRepository categoryPermissions,
            IPhotoCreator photoCreator,
            IPiwigoConfiguration piwigoConfiguration)
        {
            _contextAccessor = contextAccessor;
            _webServiceRepository = webServiceRepository;
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

            return await _webServiceRepository
                .GetMethod("pwg.images.getInfo")
                .Execute(new Dictionary<string, IConvertible>
                {
                        { "image_id", photo.PhotoId }
                }, user, token);
        }
    }
}
