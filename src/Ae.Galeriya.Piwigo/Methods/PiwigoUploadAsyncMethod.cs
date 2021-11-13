using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Entities;
using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        private readonly IPhotoBlobRepository _photoCreator;
        private readonly GalleriaDbContext _dbContext;

        public string MethodName => "pwg.images.uploadAsync";

        public PiwigoUploadAsyncMethod(IHttpContextAccessor contextAccessor,
            IUploadRepository sessionRepository,
            IPiwigoWebServiceMethodRepository webServiceRepository,
            IPhotoBlobRepository photoCreator,
            GalleriaDbContext dbContext)
        {
            _contextAccessor = contextAccessor;
            _sessionRepository = sessionRepository;
            _webServiceRepository = webServiceRepository;
            _photoCreator = photoCreator;
            this._dbContext = dbContext;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var chunk = parameters["chunk"].ToInt32(null);
            var chunks = parameters["chunks"].ToInt32(null);
            var categoryId = parameters["category"].ToUInt32(null);
            var originalChecksum = parameters["original_sum"].ToString(null);
            var fileName = parameters["filename"].ToString(null);
            var name = parameters["name"].ToString(null);
            var creationDate = DateTimeOffset.ParseExact(parameters["date_creation"].ToString(null), "yyyy-MM-dd HH:mm:ss", null);

            var file = _contextAccessor.HttpContext.Request.Form.Files.Single();

            var uploadedFile = await _sessionRepository.AcceptChunk(originalChecksum, chunk, chunks, file, token);
            if (uploadedFile != null)
            {
                var result = await _photoCreator.CreatePhotoBlob(uploadedFile, token);

                var photo = new Photo
                {
                    Blob = result.BlobId,
                    FileSize = (ulong)uploadedFile.Length,
                    FileName = fileName,
                    Name = name,
                    CreatedOn = creationDate,
                    Width = result.Width,
                    Height = result.Height,
                    Categories = await _dbContext.Categories.Where(x => x.CategoryId == categoryId).ToListAsync(token)
                };

                _dbContext.Photos.Add(photo);
                await _dbContext.SaveChangesAsync(token);

                return await _webServiceRepository
                    .GetMethod("pwg.images.getInfo")
                    .Execute(new Dictionary<string, IConvertible>
                    {
                        { "image_id", photo.PhotoId }
                    }, token);
            }

            return new PiwigiUploadedChunkResponse { Message = $"chunks uploaded" };
        }
    }
}
