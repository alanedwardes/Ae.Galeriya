using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public sealed class BlobRepository : IBlobRepository
    {
        private readonly ILogger<BlobRepository> _logger;
        private readonly ITransferUtility _transferUtility;
        private readonly IGaleriyaConfiguration _configuration;

        public BlobRepository(ILogger<BlobRepository> logger,
            ITransferUtility transferUtility,
            IGaleriyaConfiguration configuration)
        {
            _logger = logger;
            _transferUtility = transferUtility;
            _configuration = configuration;
        }

        private FileInfo GetCacheFileBlob(string key)
        {
            if (_configuration.BucketCache == null)
            {
                return null;
            }

            return new FileInfo(Path.Combine(_configuration.BucketCache.FullName, key));
        }

        public async Task<Stream> GetBlob(Guid blobId, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            var request = new GetObjectRequest
            {
                BucketName = _configuration.BucketName,
                Key = blobId.ToString()
            };

            var cacheFile = GetCacheFileBlob(request.Key);
            if (cacheFile != null && cacheFile.Exists)
            {
                return cacheFile.OpenRead();
            }

            var response = await _transferUtility.S3Client.GetObjectAsync(request, token);

            _logger.LogInformation("Got response stream for {Key} in {TotalSeconds}s", request.Key, sw.Elapsed.TotalSeconds);

            if (cacheFile != null && !cacheFile.Exists)
            {
                using (var cacheStream = cacheFile.Open(FileMode.CreateNew, FileAccess.Write))
                {
                    await response.ResponseStream.CopyToAsync(cacheStream, token);
                    await response.ResponseStream.DisposeAsync();
                }
                return cacheFile.OpenRead();
            }

            return response.ResponseStream;
        }

        public async Task PutBlob(FileInfo photoPath, Guid blobId, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            var request = new TransferUtilityUploadRequest
            {
                FilePath = photoPath.FullName,
                BucketName = _configuration.BucketName,
                Key = blobId.ToString()
            };

            await _transferUtility.UploadAsync(request, token);

            var cacheFile = GetCacheFileBlob(request.Key);
            if (cacheFile != null)
            {
                photoPath.CopyTo(cacheFile.FullName);
            }

            _logger.LogInformation("Uploaded blob {Key} in {TotalSeconds}s", request.Key, sw.Elapsed.TotalSeconds);
        }
    }
}
