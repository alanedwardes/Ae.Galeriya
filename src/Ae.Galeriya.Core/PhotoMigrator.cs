using CoenM.ImageHash.HashAlgorithms;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
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
                    await MigrateThumbnail(photoRepository, tempRepository, token);
                    await MigrateContentHash(photoRepository, token);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private async Task MigrateThumbnail(IBlobRepository photoRepository, IFileBlobRepository tempRepository, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var photo = await context.Photos.Where(x => x.HasThumbnail == false)
                                            .OrderBy(X => X.PhotoId)
                                            .FirstOrDefaultAsync(token);
            if (photo == null)
            {
                return;
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

            _logger.LogInformation("Adding thumbnail to image {ImageId}", photo.BlobId);
            photo.HasThumbnail = true;
            await context.SaveChangesAsync(token);
        }

        private async Task MigrateContentHash(IBlobRepository photoRepository, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var photo = await context.Photos.Where(x => (x.ContentAverageHash == null ||
                                                        x.ContentDifferenceHash == null ||
                                                        x.ContentPerceptualHash == null) && x.Duration == null)
                                            .OrderBy(X => X.PhotoId)
                                            .FirstOrDefaultAsync(token);
            if (photo == null)
            {
                return;
            }

            using var blob = await photoRepository.GetBlob(photo.BlobId, token);

            var image = await Image.LoadAsync<Rgba32>(blob);

            photo.ContentAverageHash = new AverageHash().Hash(image).ToString("x2");
            photo.ContentDifferenceHash = new DifferenceHash().Hash(image).ToString("x2");
            photo.ContentPerceptualHash = new CoenM.ImageHash.HashAlgorithms.PerceptualHash().Hash(image).ToString("x2");

            _logger.LogInformation("Adding content hashes to image {ImageId}", photo.BlobId);
            await context.SaveChangesAsync(token);
        }
    }
}
