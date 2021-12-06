using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoUploadAsyncMethod : IPiwigoWebServiceMethod
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUploadRepository _sessionRepository;
        private readonly IPiwigoWebServiceMethodRepository _webServiceRepository;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly UserManager<User> _userManager;
        private readonly IPhotoCreator _photoCreator;
        private readonly IPiwigoConfiguration _piwigoConfiguration;

        public string MethodName => "pwg.images.uploadAsync";
        public bool AllowAnonymous => true;

        public PiwigoUploadAsyncMethod(IHttpContextAccessor contextAccessor,
            IUploadRepository sessionRepository,
            IPiwigoWebServiceMethodRepository webServiceRepository,
            ICategoryPermissionsRepository categoryPermissions,
            UserManager<User> userManager,
            IPhotoCreator photoCreator,
            IPiwigoConfiguration piwigoConfiguration)
        {
            _contextAccessor = contextAccessor;
            _sessionRepository = sessionRepository;
            _webServiceRepository = webServiceRepository;
            _categoryPermissions = categoryPermissions;
            _userManager = userManager;
            _photoCreator = photoCreator;
            _piwigoConfiguration = piwigoConfiguration;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, User user, CancellationToken token)
        {
            var loginResult = await _webServiceRepository
                .GetMethod("pwg.session.login")
                .Execute(parameters, null, token);

            if (!loginResult.Equals(true))
            {
                return loginResult;
            }

            user = await _userManager.FindByNameAsync(parameters.GetRequired<string>("username"));
            var category = await _categoryPermissions.EnsureCanAccessCategory(user, parameters.GetRequired<uint>("category"), token);

            var chunk = parameters.GetRequired<int>("chunk");
            var chunks = parameters.GetRequired<int>("chunks");
            var originalChecksum = parameters.GetRequired<string>("original_sum");
            var fileName = parameters.GetRequired<string>("filename");
            var name = parameters.GetRequired<string>("name");
            var creationDate = DateTimeOffset.ParseExact(parameters.GetRequired<string>("date_creation"), "yyyy-MM-dd HH:mm:ss", null);

            var file = _contextAccessor.HttpContext.Request.Form.Files.Single();

            var uploadedFile = await _sessionRepository.AcceptChunk(originalChecksum, chunk, chunks, file, token);
            if (uploadedFile != null)
            {
                return await CompleteFile(category, fileName, name, user, creationDate, uploadedFile, CancellationToken.None);
            }

            return new PiwigoUploadedChunkResponse { Message = $"chunks uploaded" };
        }

        private async Task<object> CompleteFile(Category category, string fileName, string name, User user, DateTimeOffset creationDate, FileInfo uploadedFile, CancellationToken token)
        {
            var photo = await _photoCreator.CreatePhoto(_piwigoConfiguration.FileBlobRepository, category, fileName, name, user, creationDate, uploadedFile, token);

            return await _webServiceRepository
                .GetMethod("pwg.images.getInfo")
                .Execute(new Dictionary<string, IConvertible>
                {
                        { "image_id", photo.PhotoId }
                }, user, token);
        }
    }
}
