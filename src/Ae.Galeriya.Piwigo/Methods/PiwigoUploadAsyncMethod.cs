using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoUploadAsyncMethod : IPiwigoWebServiceMethod
    {
        private readonly ILogger<PiwigoUploadAsyncMethod> _logger;
        private readonly IPiwigoConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUploadRepository _sessionRepository;
        private readonly IPiwigoWebServiceMethodRepository _webServiceRepository;
        private readonly IBlobRepository _photoCreator;
        private readonly IMediaInfoExtractor _infoExtractor;
        private readonly GalleriaDbContext _dbContext;

        public string MethodName => "pwg.images.uploadAsync";

        public PiwigoUploadAsyncMethod(ILogger<PiwigoUploadAsyncMethod> logger,
            IPiwigoConfiguration configuration,
            IHttpContextAccessor contextAccessor,
            IUploadRepository sessionRepository,
            IPiwigoWebServiceMethodRepository webServiceRepository,
            IBlobRepository photoCreator,
            IMediaInfoExtractor infoExtractor,
            GalleriaDbContext dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            _sessionRepository = sessionRepository;
            _webServiceRepository = webServiceRepository;
            _photoCreator = photoCreator;
            _infoExtractor = infoExtractor;
            _dbContext = dbContext;
        }

        private async Task<Guid?> ExtractSnapshot(FileInfo uploadedFile, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            var snapshotFile = _configuration.FileBlobRepository.GetFileInfoForBlob(Guid.NewGuid() + ".jpg");
            
            await _infoExtractor.ExtractSnapshot(uploadedFile, snapshotFile, token);

            Guid? snapshotId = null;
            if (snapshotFile.Exists)
            {
                snapshotId = Guid.NewGuid();
                await _photoCreator.PutBlob(snapshotFile.OpenRead(), snapshotId.Value, token);
            }

            _logger.LogInformation("Processed snapshot {Snapshot} for {File} in {TotalSeconds}s", snapshotFile, uploadedFile, sw.Elapsed.TotalSeconds, snapshotId.HasValue);
            return snapshotId;
        }

        private async Task<string> CalculateFileHash(FileInfo uploadedFile, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();
            using (var sha256 = SHA256.Create())
            using (var fs = uploadedFile.OpenRead())
            {
                var hash = await sha256.ComputeHashAsync(fs, token);
                _logger.LogInformation("Calculated hash for file {File} in {TotalSeconds}s", uploadedFile, sw.Elapsed.TotalSeconds); ;
                return string.Concat(hash.Select(x => x.ToString("X2")));
            }
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
                token = CancellationToken.None;

                var blobId = Guid.NewGuid();
                var blobIdTask = _photoCreator.PutBlob(uploadedFile.OpenRead(), blobId, token);
                var mediaInfoTask = _infoExtractor.ExtractInformation(uploadedFile, token);
                var snapshotIdTask = ExtractSnapshot(uploadedFile, token);
                var hashTask = CalculateFileHash(uploadedFile, token);

                await blobIdTask;
                var mediaInfo = await mediaInfoTask;
                var snapshotId = await snapshotIdTask;
                var hash = await hashTask;

                var fileExtension = Path.GetExtension(fileName)?.ToLower().TrimStart('.');
                if (string.IsNullOrWhiteSpace(fileExtension))
                {
                    throw new InvalidOperationException("No file extension found");
                }

                var photo = new Photo
                {
                    Blob = blobId,
                    SnapshotBlob = snapshotId,
                    FileSize = (ulong)uploadedFile.Length,
                    Extension = fileExtension,
                    FileName = fileName,
                    Hash = hash,
                    Name = name,
                    CreatedOn = mediaInfo.CreationTime ?? creationDate,
                    Make = mediaInfo.Camera.Make,
                    Model = mediaInfo.Camera.Model,
                    Software = mediaInfo.Camera.Software,
                    Orientation = mediaInfo.Orientation,
                    Duration = mediaInfo.Duration,
                    Width = (uint)mediaInfo.Size.Width,
                    Height = (uint)mediaInfo.Size.Height,
                    Latitude = mediaInfo.Location?.Latitude,
                    Longitude = mediaInfo.Location?.Longitude,
                    Categories = await _dbContext.Categories.Where(x => x.CategoryId == categoryId).ToListAsync(token)
                };

                _dbContext.Photos.Add(photo);

                try
                {
                    await _dbContext.SaveChangesAsync(token);
                }
                catch (DbUpdateException)
                {
                    photo = await _dbContext.Photos.SingleAsync(x => x.Hash == hash, token);
                }
                finally
                {
                    uploadedFile.Delete();

                }

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
