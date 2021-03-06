using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IUploadRepository _sessionRepository;
        private readonly IPiwigoWebServiceMethodRepository _webServiceRepository;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IPhotoCreator _photoCreator;
        private readonly IPiwigoConfiguration _piwigoConfiguration;
        private readonly SignInManager<User> _signInManager;

        public string MethodName => "pwg.images.uploadAsync";
        public bool AllowAnonymous => true;

        public PiwigoUploadAsyncMethod(IServiceProvider serviceProvider,
            IUploadRepository sessionRepository,
            IPiwigoWebServiceMethodRepository webServiceRepository,
            ICategoryPermissionsRepository categoryPermissions,
            IPhotoCreator photoCreator,
            IPiwigoConfiguration piwigoConfiguration,
            SignInManager<User> signInManager)
        {
            _serviceProvider = serviceProvider;
            _sessionRepository = sessionRepository;
            _webServiceRepository = webServiceRepository;
            _categoryPermissions = categoryPermissions;
            _photoCreator = photoCreator;
            _piwigoConfiguration = piwigoConfiguration;
            _signInManager = signInManager;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, IFormFile> fileParameters, uint? userId, CancellationToken token)
        {
            var result = await _signInManager.PasswordSignInAsync(parameters.GetRequired<string>("username"), parameters.GetRequired<string>("password"), false, false);
            if (!result.Succeeded)
            {
                return false;
            }

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            userId = (await _signInManager.UserManager.FindByNameAsync(parameters.GetRequired<string>("username"))).Id;
            var category = await _categoryPermissions.EnsureCanAccessCategory(context, userId.Value, parameters.GetRequired<uint>("category"), token);

            var chunk = parameters.GetRequired<int>("chunk");
            var chunks = parameters.GetRequired<int>("chunks");
            var originalChecksum = parameters.GetRequired<string>("original_sum");
            var fileName = parameters.GetRequired<string>("filename");
            var name = parameters.GetOptional("name") ?? fileName;

            var creationDate = DateTimeOffset.ParseExact(parameters.GetRequired<string>("date_creation"), "yyyy-MM-dd HH:mm:ss", null);

            var file = fileParameters.Values.Single();

            var uploadedFile = await _sessionRepository.AcceptChunk(originalChecksum, chunk, chunks, file, token);
            if (uploadedFile != null)
            {
                return await CompleteFile(context, category, fileName, name, userId.Value, creationDate, uploadedFile, CancellationToken.None);
            }

            return new PiwigoUploadedChunkResponse { Message = $"chunks uploaded" };
        }

        private async Task<object> CompleteFile(GaleriyaDbContext dbContext, Category category, string fileName, string name, uint userId, DateTimeOffset creationDate, FileInfo uploadedFile, CancellationToken token)
        {
            var photo = await _photoCreator.CreatePhoto(dbContext, _piwigoConfiguration.TemporaryBlobRepository(_serviceProvider), _piwigoConfiguration.PersistentBlobRepository(_serviceProvider), category, fileName, name, userId, creationDate, uploadedFile, token);

            return await _webServiceRepository
                .GetMethod("pwg.images.getInfo")
                .Execute(new Dictionary<string, IConvertible>
                {
                    { "image_id", photo.PhotoId }
                }, new Dictionary<string, IFormFile>(), userId, token);
        }
    }
}
