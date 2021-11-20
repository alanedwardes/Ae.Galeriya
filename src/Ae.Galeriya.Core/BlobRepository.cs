using Ae.Galeriya.Core.Tables;
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

        public BlobRepository(ILogger<BlobRepository> logger, ITransferUtility transferUtility, IGaleriyaConfiguration configuration)
        {
            _logger = logger;
            _transferUtility = transferUtility;
            _configuration = configuration;
        }

        public async Task<Stream> GetBlob(Photo photo, bool snapshot, CancellationToken token)
        {
            var request = new GetObjectRequest
            {
                BucketName = _configuration.BucketName,
                Key = (snapshot ? (photo.SnapshotBlob ?? photo.Blob) : photo.Blob).ToString()
            };

            var response = await _transferUtility.S3Client.GetObjectAsync(request, token);

            return response.ResponseStream;
        }

        public async Task<Guid> PutBlob(FileInfo photoPath, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            var blobId = Guid.NewGuid();

            await _transferUtility.UploadAsync(new TransferUtilityUploadRequest
            {
                FilePath = photoPath.FullName,
                BucketName = _configuration.BucketName,
                Key = blobId.ToString()
            }, token);

            _logger.LogInformation("Uploaded blob {BlobId} in {TotalSeconds}s", blobId, sw.Elapsed.TotalSeconds);
            return blobId;
        }
    }
}
