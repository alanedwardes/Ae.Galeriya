using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public sealed class AmazonS3BlobRepository : IBlobRepository
    {
        private readonly ITransferUtility _transferUtility;
        private readonly string _bucketName;

        public AmazonS3BlobRepository(ITransferUtility transferUtility, string bucketName)
        {
            _transferUtility = transferUtility;
            _bucketName = bucketName;
        }

        public async Task<Stream> GetBlob(Guid blobId, CancellationToken token)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = blobId.ToString()
            };

            return (await _transferUtility.S3Client.GetObjectAsync(request, token)).ResponseStream;
        }

        public async Task PutBlob(FileInfo photoPath, Guid blobId, CancellationToken token)
        {
            var request = new TransferUtilityUploadRequest
            {
                FilePath = photoPath.FullName,
                BucketName = _bucketName,
                Key = blobId.ToString()
            };

            await _transferUtility.UploadAsync(request, token);
        }
    }
}
