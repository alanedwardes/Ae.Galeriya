using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                    using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

                    var photo = await context.Photos.Where(x => x.HasThumbnail == false)
                                                    .OrderBy(X => X.PhotoId)
                                                    .FirstOrDefaultAsync(token);

                    if (photo == null)
                    {
                        break;
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
                finally
                {
                    _semaphore.Release();
                }
            }
        }
    }
}
