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
        private readonly IServiceProvider _serviceProvider;
        private readonly IUploadRepository _sessionRepository;
        private readonly IPiwigoWebServiceMethodRepository _webServiceRepository;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IPhotoCreator _photoCreator;
        private readonly IPiwigoConfiguration _piwigoConfiguration;
        private readonly SignInManager<User> _signInManager;

        public string MethodName => "pwg.images.uploadAsync";
        public bool AllowAnonymous => true;

        public PiwigoUploadAsyncMethod(IHttpContextAccessor contextAccessor,
            IServiceProvider serviceProvider,
            IUploadRepository sessionRepository,
            IPiwigoWebServiceMethodRepository webServiceRepository,
            ICategoryPermissionsRepository categoryPermissions,
            IPhotoCreator photoCreator,
            IPiwigoConfiguration piwigoConfiguration,
            SignInManager<User> signInManager)
        {
            _contextAccessor = contextAccessor;
            _serviceProvider = serviceProvider;
            _sessionRepository = sessionRepository;
            _webServiceRepository = webServiceRepository;
            _categoryPermissions = categoryPermissions;
            _photoCreator = photoCreator;
            _piwigoConfiguration = piwigoConfiguration;
            _signInManager = signInManager;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            var result = await _signInManager.PasswordSignInAsync(parameters.GetRequired<string>("username"), parameters.GetRequired<string>("password"), false, false);
            if (!result.Succeeded)
            {
                return false;
            }

            userId = (await _signInManager.UserManager.FindByNameAsync(parameters.GetRequired<string>("username"))).Id;
            var category = await _categoryPermissions.EnsureCanAccessCategory(userId.Value, parameters.GetRequired<uint>("category"), token);

            var chunk = parameters.GetRequired<int>("chunk");
            var chunks = parameters.GetRequired<int>("chunks");
            var originalChecksum = parameters.GetRequired<string>("original_sum");
            var fileName = parameters.GetRequired<string>("filename");
            var name = parameters.GetOptional("name") ?? fileName;

            var creationDate = DateTimeOffset.ParseExact(parameters.GetRequired<string>("date_creation"), "yyyy-MM-dd HH:mm:ss", null);

            var file = _contextAccessor.HttpContext.Request.Form.Files.Single();

            var uploadedFile = await _sessionRepository.AcceptChunk(originalChecksum, chunk, chunks, file, token);
            if (uploadedFile != null)
            {
                return await CompleteFile(category, fileName, name, userId.Value, creationDate, uploadedFile, CancellationToken.None);
            }

            return new PiwigoUploadedChunkResponse { Message = $"chunks uploaded" };
        }

        private async Task<object> CompleteFile(Category category, string fileName, string name, uint userId, DateTimeOffset creationDate, FileInfo uploadedFile, CancellationToken token)
        {
            var photo = await _photoCreator.CreatePhoto(_piwigoConfiguration.FileBlobRepository(_serviceProvider), category, fileName, name, userId, creationDate, uploadedFile, token);

            return await _webServiceRepository
                .GetMethod("pwg.images.getInfo")
                .Execute(new Dictionary<string, IConvertible>
                {
                    { "image_id", photo.PhotoId }
                }, userId, token);
        }
    }
}
