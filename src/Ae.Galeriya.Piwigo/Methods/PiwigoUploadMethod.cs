using Ae.Galeriya.Core;
using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoUploadMethod : IPiwigoWebServiceMethod
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly IPhotoCreator _photoCreator;
        private readonly IPiwigoConfiguration _piwigoConfiguration;

        public string MethodName => "pwg.images.upload";
        public bool AllowAnonymous => false;

        public PiwigoUploadMethod(IServiceProvider serviceProvider,
            ICategoryPermissionsRepository categoryPermissions,
            IPhotoCreator photoCreator,
            IPiwigoConfiguration piwigoConfiguration)
        {
            _serviceProvider = serviceProvider;
            _categoryPermissions = categoryPermissions;
            _photoCreator = photoCreator;
            _piwigoConfiguration = piwigoConfiguration;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, FileMultipartSection> fileParameters, uint? userId, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var category = await _categoryPermissions.EnsureCanAccessCategory(context, userId.Value, parameters.GetRequired<uint>("category"), token);

            var name = parameters.GetRequired<string>("name");

            var file = fileParameters.Values.Single();

            var uploadedFile = _piwigoConfiguration.TemporaryBlobRepository(_serviceProvider).GetFileInfoForBlob(Guid.NewGuid().ToString());

            using (var write = uploadedFile.OpenWrite())
            {
                await file.FileStream.CopyToAsync(write, token);
            }

            var photo = await _photoCreator.CreatePhoto(context, _piwigoConfiguration.TemporaryBlobRepository(_serviceProvider), _piwigoConfiguration.PersistentBlobRepository(_serviceProvider), category, file.FileName, name, userId.Value, DateTimeOffset.UtcNow, uploadedFile, token);

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
