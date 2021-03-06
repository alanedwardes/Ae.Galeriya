using Ae.Geocode.Google;
using Ae.Geocode.Google.Entities;
using Ae.MediaMetadata;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    internal sealed class PhotoMigrator : IPhotoMigrator
    {
        private readonly ILogger<PhotoMigrator> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public PhotoMigrator(ILogger<PhotoMigrator> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task MigratePhotos(IBlobRepository photoRepository, IFileBlobRepository tempRepository, CancellationToken token)
        {
            while (true)
            {
                await _semaphore.WaitAsync(token);

                try
                {
                    var results = new bool[]
                    {
                        await MigrateThumbnail(photoRepository, tempRepository, token),
                        await MigrateContentHash(photoRepository, token),
                        await MigrateGeocode(token)
                    };

                    if (results.All(x => x == false))
                    {
                        break;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private async Task<bool> MigrateThumbnail(IBlobRepository photoRepository, IFileBlobRepository tempRepository, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var photo = await context.Photos.Where(x => x.HasThumbnail == false)
                                            .OrderBy(X => X.PhotoId)
                                            .FirstOrDefaultAsync(token);
            if (photo == null)
            {
                return false;
            }

            using var blob = await photoRepository.GetBlob(photo.BlobId, token);
            var thumbBlob = photo.BlobId + "_thumb";
            var tempFileInfo = tempRepository.GetFileInfoForBlob(thumbBlob);

            using (var image = new MagickImage(blob))
            {
                image.Format = MagickFormat.Jpeg;
                image.Quality = 50;
                image.Strip();
                image.Resize(2000, 2000);
                image.Write(tempFileInfo);
            }

            using (var readStream = tempFileInfo.OpenRead())
            {
                await photoRepository.PutBlob(readStream, thumbBlob, token);
            }

            _logger.LogInformation("Adding thumbnail to photo {PhotoId}", photo.PhotoId);
            photo.HasThumbnail = true;
            await context.SaveChangesAsync(token);
            return true;
        }

        private async Task<bool> MigrateContentHash(IBlobRepository photoRepository, CancellationToken token)
        {
            var extractor = _serviceProvider.GetRequiredService<IMediaInfoExtractor>();
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var photo = await context.Photos.Where(x => (x.ContentPerceptualHash == null || (x.ColourR == null || x.ColourG == null || x.ColourB == null)) && x.Duration == null)
                                            .OrderBy(x => x.PhotoId)
                                            .FirstOrDefaultAsync(token);
            if (photo == null)
            {
                return false;
            }

            using var blob = await photoRepository.GetBlob(photo.BlobId, token);

            var image = await Image.LoadAsync<Rgba32>(blob);
            var color = extractor.ExtractColor(image);

            if (photo.ContentPerceptualHash == null)
            {
                photo.ContentPerceptualHash = new CoenM.ImageHash.HashAlgorithms.PerceptualHash().Hash(image).ToString("x2");
            }

            if (photo.ColourR == null || photo.ColourG == null || photo.ColourB == null)
            {
                photo.ColourR = color.R;
                photo.ColourG = color.G;
                photo.ColourB = color.B;
            }

            _logger.LogInformation("Adding colours and content hashes to photo {PhotoId}", photo.PhotoId);
            await context.SaveChangesAsync(token);
            return true;
        }

        private static IReadOnlyList<AddressComponent> GetMostDescriptiveAddressComponents(GeocodeResponse response)
        {
            return response.GuessMajorLocationParts()
                .Select(x => x.GetMostDescriptiveAddressComponent())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.LongName ?? x.ShortName))
                .Select(x => x!)
                .ToArray();
        }

        private async Task<bool> MigrateGeocode(CancellationToken token)
        {
            var tagRepository = _serviceProvider.GetRequiredService<ITagRepository>();
            var geocodeClient = _serviceProvider.GetRequiredService<IGoogleGeocodeClient>();
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var query = context.Photos.FromSqlRaw("SELECT * FROM `Photos` WHERE NOT JSON_CONTAINS_PATH(Metadata, 'all', '$.geocode') AND Latitude IS NOT NULL LIMIT 1");

            var photo = await query.SingleOrDefaultAsync(token);
            if (photo == null)
            {
                return false;
            }

            photo = await context.Photos.Include(x => x.Tags).SingleAsync(x => x.PhotoId == photo.PhotoId);

            _logger.LogInformation("Getting goecode response for image {PhotoId}", photo.PhotoId);

            var location = (photo.Latitude.Value, photo.Longitude.Value);

            var geocodeResponse = await geocodeClient.ReverseGeoCode(new GeocodeRequest(location), token);
            if (geocodeResponse != null)
            {
                var tagName = string.Join(", ", GetMostDescriptiveAddressComponents(geocodeResponse).Select(x => x.LongName));
                photo.Tags.Add(await tagRepository.CreateTag(context, photo.CreatedById, tagName, token));
            }

            var metadata = photo.PhotoMetadataMarshaled;
            metadata.Geocode = geocodeResponse;
            photo.PhotoMetadataMarshaled = metadata;

            _logger.LogInformation("Adding goecode data to image {PhotoId}", photo.PhotoId);
            await context.SaveChangesAsync(token);
            return true;
        }
    }
}
