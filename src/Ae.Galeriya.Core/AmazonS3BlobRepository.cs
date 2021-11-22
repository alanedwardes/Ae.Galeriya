using Ae.Galeriya.Core.Exceptions;
using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Net;
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

        public async Task DeleteBlob(Guid blobId, CancellationToken token)
        {
            await _transferUtility.S3Client.DeleteAsync(_bucketName, blobId.ToString(), null, token);
        }

        public async Task<Stream> GetBlob(Guid blobId, CancellationToken token)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = blobId.ToString()
            };

            try
            {
                return (await _transferUtility.S3Client.GetObjectAsync(request, token)).ResponseStream;
            }
            catch (AmazonServiceException e) when (e.StatusCode == HttpStatusCode.NotFound || e.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new BlobNotFoundException(blobId);
            }
        }

        public async Task PutBlob(Stream blobStream, Guid blobId, CancellationToken token)
        {
            var request = new TransferUtilityUploadRequest
            {
                InputStream = blobStream,
                AutoCloseStream = true,
                BucketName = _bucketName,
                Key = blobId.ToString()
            };

            await _transferUtility.UploadAsync(request, token);
        }
    }
}
