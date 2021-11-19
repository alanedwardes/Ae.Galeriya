using Ae.Galeriya.Core.Tables;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public sealed class BlobRepository : IBlobRepository
    {
        private readonly ITransferUtility _transferUtility;

        public BlobRepository(ITransferUtility transferUtility)
        {
            _transferUtility = transferUtility;
        }

        public async Task<Stream> GetBlob(Photo photo, bool snapshot, CancellationToken token)
        {
            var request = new GetObjectRequest
            {
                BucketName = "ae-piwigo-test",
                Key = (snapshot ? (photo.SnapshotBlob ?? photo.Blob) : photo.Blob).ToString()
            };

            var response = await _transferUtility.S3Client.GetObjectAsync(request, token);

            return response.ResponseStream;
        }

        public async Task<Guid> PutBlob(FileInfo photoPath, CancellationToken token)
        {
            var blobId = Guid.NewGuid();

            await _transferUtility.UploadAsync(new TransferUtilityUploadRequest
            {
                FilePath = photoPath.FullName,
                BucketName = "ae-piwigo-test",
                Key = blobId.ToString()
            }, token);

            return blobId;
        }
    }
}
