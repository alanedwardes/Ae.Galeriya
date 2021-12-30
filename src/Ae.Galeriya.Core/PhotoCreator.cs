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
        private readonly IMediaInfoExtractor _infoExtractor;
        private readonly IPhotoMigrator _photoMigrator;

        public PhotoCreator(ILogger<PhotoCreator> logger,
            IMediaInfoExtractor infoExtractor,
            IPhotoMigrator photoMigrator)
        {
            _logger = logger;
            _infoExtractor = infoExtractor;
            _photoMigrator = photoMigrator;
        }

        private async Task<string?> ExtractSnapshot(IFileBlobRepository fileBlobRepository, IBlobRepository persistentBlobRepository, FileInfo uploadedFile, string hash, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            var snapshotFile = fileBlobRepository.GetFileInfoForBlob(Guid.NewGuid() + ".jpg");

            await _infoExtractor.ExtractSnapshot(uploadedFile, snapshotFile, token);

            string? snapshotId = hash;
            if (snapshotFile.Exists)
            {
                snapshotId = hash + "_thumb";
                try
                {
                    await persistentBlobRepository.PutBlob(snapshotFile.OpenRead(), snapshotId, token);
                }
                finally
                {
                    snapshotFile.Delete();
                }
            }

            _logger.LogInformation("Processed snapshot {Snapshot} for {File} in {TotalSeconds}s", snapshotFile, uploadedFile, sw.Elapsed.TotalSeconds, snapshotId);
            return snapshotId;
        }

        private async Task<string> CalculateFileHash(FileInfo uploadedFile, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();
            using (var sha256 = SHA256.Create())
            using (var fs = uploadedFile.OpenRead())
            {
                var hash = string.Concat((await sha256.ComputeHashAsync(fs, token)).Select(x => x.ToString("x2")));
                _logger.LogInformation("Calculated hash {Hash} for file {File} in {TotalSeconds}s", hash, uploadedFile, sw.Elapsed.TotalSeconds); ;
                return hash;
            }
        }

        public async Task<Photo> CreatePhoto(GaleriyaDbContext dbContext, IFileBlobRepository temporaryBlobRepository, IBlobRepository persistentBlobRepository, Category category, string fileName, string name, uint userId, DateTimeOffset creationDate, FileInfo uploadedFile, CancellationToken token)
        {
            var hash = await CalculateFileHash(uploadedFile, token);

            var existingPhoto = await dbContext.Photos.SingleOrDefaultAsync(x => x.BlobId == hash, token);
            if (existingPhoto != null)
            {
                await AddPhotoToCategory(dbContext, existingPhoto, category, token);
                return existingPhoto;
            }

            var blobTask = persistentBlobRepository.PutBlob(uploadedFile.OpenRead(), hash, token);
            var mediaInfo = await _infoExtractor.ExtractInformation(uploadedFile, token);
            var snapshotId = await (mediaInfo.Duration.HasValue ? ExtractSnapshot(temporaryBlobRepository, persistentBlobRepository, uploadedFile, hash, token) : Task.FromResult<string?>(null));

            if (mediaInfo.Size.Width == 0 || mediaInfo.Size.Height == 0)
            {
                throw new InvalidOperationException("Found zero-sized image");
            }

            var fileExtension = Path.GetExtension(fileName)?.ToLower().TrimStart('.');
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                throw new InvalidOperationException("Found no file extension");
            }

            if (!mediaInfo.CreationTime.HasValue)
            {
                _logger.LogWarning("Unable to find creation time for {Hash}, using {CreationDate}", hash, creationDate);
            }

            // Ensure the photo's blob is persisted
            await blobTask;

            var photo = new Photo
            {
                BlobId = hash,
                HasThumbnail = snapshotId != null,
                FileSize = (ulong)uploadedFile.Length,
                Extension = fileExtension,
                FileName = fileName,
                CreatedById = userId,
                Name = name,
                TakenOn = mediaInfo.CreationTime,
                FileCreatedOn = creationDate,
                CreatedOn = DateTimeOffset.UtcNow,
                Orientation = mediaInfo.Orientation ?? MediaOrientation.Unknown,
                Duration = mediaInfo.Duration,
                Width = (uint)mediaInfo.Size.Width,
                Height = (uint)mediaInfo.Size.Height,
                Latitude = mediaInfo.Location?.Latitude,
                Longitude = mediaInfo.Location?.Longitude,
                Categories = new List<Category> { category },
                PhotoMetadataMarshaled = new PhotoMetadata
                {
                    MediaInfo = mediaInfo
                }
            };

            dbContext.Photos.Add(photo);

            try
            {
                await dbContext.SaveChangesAsync(token);
                _logger.LogInformation("Added photo with hash {Hash}", hash);
                // Run migration as a background task
                _photoMigrator.MigratePhotos(persistentBlobRepository, temporaryBlobRepository, CancellationToken.None);
            }
            catch (DbUpdateException e)
            {
                _logger.LogWarning(e, "Found duplicate photo with hash {Hash}, adding to category instead", hash);
                dbContext.Photos.Remove(photo);
                photo = await dbContext.Photos.SingleAsync(x => x.BlobId == hash, token);
                await AddPhotoToCategory(dbContext, photo, category, token);
            }
            finally
            {
                uploadedFile.Delete();
            }

            return photo;
        }

        private async Task AddPhotoToCategory(GaleriyaDbContext dbContext, Photo photo, Category category, CancellationToken token)
        {
            photo.Categories.Add(category);

            try
            {
                await dbContext.SaveChangesAsync(token);
            }
            catch (DbUpdateException)
            {
                // This is OK
            }
        }
    }
}
