using Ae.Galeriya.Core.Tables;
using Ae.MediaMetadata;
using Ae.MediaMetadata.Entities;
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

namespace Ae.Galeriya.Core
{
    internal sealed class PhotoCreator : IPhotoCreator
    {
        private readonly ILogger<PhotoCreator> _logger;
        private readonly IBlobRepository _photoCreator;
        private readonly IMediaInfoExtractor _infoExtractor;
        private readonly GaleriyaDbContext _dbContext;

        public PhotoCreator(ILogger<PhotoCreator> logger,
            IBlobRepository photoCreator,
            IMediaInfoExtractor infoExtractor,
            GaleriyaDbContext dbContext)
        {
            _logger = logger;
            _photoCreator = photoCreator;
            _infoExtractor = infoExtractor;
            _dbContext = dbContext;
        }

        private async Task<Guid?> ExtractSnapshot(IFileBlobRepository fileBlobRepository, FileInfo uploadedFile, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            var snapshotFile = fileBlobRepository.GetFileInfoForBlob(Guid.NewGuid() + ".jpg");

            await _infoExtractor.ExtractSnapshot(uploadedFile, snapshotFile, token);

            Guid? snapshotId = null;
            if (snapshotFile.Exists)
            {
                snapshotId = Guid.NewGuid();
                try
                {
                    await _photoCreator.PutBlob(snapshotFile.OpenRead(), snapshotId.Value, token);
                }
                finally
                {
                    snapshotFile.Delete();
                }
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

        public async Task<Photo> CreatePhoto(IFileBlobRepository fileBlobRepository, Category category, string fileName, string name, User user, DateTimeOffset creationDate, FileInfo uploadedFile, CancellationToken token)
        {
            var blobId = Guid.NewGuid();
            var blobIdTask = _photoCreator.PutBlob(uploadedFile.OpenRead(), blobId, token);
            var mediaInfoTask = _infoExtractor.ExtractInformation(uploadedFile, token);
            var snapshotIdTask = ExtractSnapshot(fileBlobRepository, uploadedFile, token);
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
                CreatedBy = user,
                Hash = hash,
                Name = name,
                CreatedOn = mediaInfo.CreationTime ?? creationDate,
                Make = mediaInfo.CameraMake,
                Model = mediaInfo.CameraModel,
                Software = mediaInfo.CameraSoftware,
                Orientation = mediaInfo.Orientation ?? MediaOrientation.Unknown,
                Duration = mediaInfo.Duration,
                Width = (uint)mediaInfo.Size.Width,
                Height = (uint)mediaInfo.Size.Height,
                Latitude = mediaInfo.Location?.Latitude,
                Longitude = mediaInfo.Location?.Longitude,
                Categories = new List<Category> { category }
            };

            _dbContext.Photos.Add(photo);

            try
            {
                await _dbContext.SaveChangesAsync(token);
            }
            catch (DbUpdateException)
            {
                return await _dbContext.Photos.SingleAsync(x => x.Hash == hash, token);
            }
            finally
            {
                uploadedFile.Delete();
            }

            return photo;
        }
    }
}
