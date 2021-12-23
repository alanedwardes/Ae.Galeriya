using Ae.Galeriya.Core.Tables;
using Ae.Geocode.Google;
using Ae.Geocode.Google.Entities;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    internal sealed class PhotoCreator : IPhotoCreator
    {
        private readonly ILogger<PhotoCreator> _logger;
        private readonly IBlobRepository _photoCreator;
        private readonly IMediaInfoExtractor _infoExtractor;
        private readonly IGoogleGeocodeClient _geocodeClient;
        private readonly GaleriyaDbContext _dbContext;

        public PhotoCreator(ILogger<PhotoCreator> logger,
            IBlobRepository photoCreator,
            IMediaInfoExtractor infoExtractor,
            IGoogleGeocodeClient geocodeClient,
            GaleriyaDbContext dbContext)
        {
            _logger = logger;
            _photoCreator = photoCreator;
            _infoExtractor = infoExtractor;
            _geocodeClient = geocodeClient;
            _dbContext = dbContext;
        }

        private async Task<string?> ExtractSnapshot(IFileBlobRepository fileBlobRepository, FileInfo uploadedFile, string hash, CancellationToken token)
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
                    await _photoCreator.PutBlob(snapshotFile.OpenRead(), snapshotId, token);
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
                var hash = await sha256.ComputeHashAsync(fs, token);
                _logger.LogInformation("Calculated hash for file {File} in {TotalSeconds}s", uploadedFile, sw.Elapsed.TotalSeconds); ;
                return string.Concat(hash.Select(x => x.ToString("x2")));
            }
        }

        private async Task<GeocodeResponse?> LookupLocation(MediaInfo mediaInfo, CancellationToken token)
        {
            if (mediaInfo.Location == null)
            {
                return null;
            }

            var request = new GeocodeRequest((mediaInfo.Location.Latitude, mediaInfo.Location.Longitude));

            try
            {
                return await _geocodeClient.ReverseGeoCode(request, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when geocoding");
            }

            return null;
        }

        private static IReadOnlyList<AddressComponent> GetMostDescriptiveAddressComponents(GeocodeResponse response)
        {
            return response.GuessMajorLocationParts()
                .Select(x => x.GetMostDescriptiveAddressComponent())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.LongName ?? x.ShortName))
                .Select(x => x!)
                .ToArray();
        }

        public async Task<Photo> CreatePhoto(IFileBlobRepository fileBlobRepository, Category category, string fileName, string name, uint userId, DateTimeOffset creationDate, FileInfo uploadedFile, CancellationToken token)
        {
            var hash = await CalculateFileHash(uploadedFile, token);

            var existingPhoto = await _dbContext.Photos.SingleOrDefaultAsync(x => x.BlobId == hash, token);
            if (existingPhoto != null)
            {
                await AddPhotoToCategory(existingPhoto, category, token);
                return existingPhoto;
            }

            var blobIdTask = _photoCreator.PutBlob(uploadedFile.OpenRead(), hash, token);
            var mediaInfo = await _infoExtractor.ExtractInformation(uploadedFile, token);
            var snapshotIdTask = mediaInfo.Duration.HasValue ? ExtractSnapshot(fileBlobRepository, uploadedFile, hash, token) : Task.FromResult<string?>(null);
            var locationTask = LookupLocation(mediaInfo, token);

            if (mediaInfo.Size.Width == 0 || mediaInfo.Size.Height == 0)
            {
                throw new InvalidOperationException("Found zero-sized image");
            }

            await blobIdTask;
            var snapshotId = await snapshotIdTask;
            var geocodeResponse = await locationTask;

            var fileExtension = Path.GetExtension(fileName)?.ToLower().TrimStart('.');
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                throw new InvalidOperationException("Found no file extension");
            }

            if (!mediaInfo.CreationTime.HasValue)
            {
                _logger.LogWarning("Unable to find creation time for {Hash}, using {CreationDate}", hash, creationDate);
            }

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
                Metadata = mediaInfo == null ? null : JsonSerializer.Serialize(mediaInfo, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
            };

            if (geocodeResponse != null)
            {
                var tagName = string.Join(", ", GetMostDescriptiveAddressComponents(geocodeResponse).Select(x => x.LongName));
                photo.Tags.Add(await CreateTag(userId, tagName, token));
            }

            _dbContext.Photos.Add(photo);

            try
            {
                await _dbContext.SaveChangesAsync(token);
                _logger.LogInformation("Added photo with hash {Hash}", hash);
            }
            catch (DbUpdateException e)
            {
                _logger.LogWarning(e, "Found duplicate photo with hash {Hash}, adding to category instead", hash);
                _dbContext.Photos.Remove(photo);
                photo = await _dbContext.Photos.SingleAsync(x => x.BlobId == hash, token);
                await AddPhotoToCategory(photo, category, token);
            }
            finally
            {
                uploadedFile.Delete();
            }

            return photo;
        }

        private async Task AddPhotoToCategory(Photo photo, Category category, CancellationToken token)
        {
            photo.Categories.Add(category);

            try
            {
                await _dbContext.SaveChangesAsync(token);
            }
            catch (DbUpdateException)
            {
                // This is OK
            }
        }

        private async Task<Tag> CreateTag(uint userId, string tagName, CancellationToken token)
        {
            var existingTag = await _dbContext.Tags.SingleOrDefaultAsync(x => x.Name == tagName, token);
            if (existingTag != null)
            {
                return existingTag;
            }

            Tag tag = new()
            {
                Name = tagName,
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedById = userId
            };
            _dbContext.Tags.Add(tag);

            try
            {
                await _dbContext.SaveChangesAsync(token);
            }
            catch (DbUpdateException)
            {
                _dbContext.Tags.Remove(tag);
                tag = await _dbContext.Tags.SingleAsync(x => x.Name == tagName, token);
            }

            return tag;
        }
    }
}
