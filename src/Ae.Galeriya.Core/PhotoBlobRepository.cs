using Ae.Galeriya.Core.Tables;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Ae.Galeriya.Core
{
    public sealed class PhotoBlobRepository : IPhotoBlobRepository
    {
        private readonly ITransferUtility _transferUtility;

        public PhotoBlobRepository(ITransferUtility transferUtility)
        {
            _transferUtility = transferUtility;
        }

        public async Task<Stream> GetPhotoBlob(Photo photo, CancellationToken token)
        {
            var request = new GetObjectRequest
            {
                BucketName = "ae-piwigo-test",
                Key = photo.Blob.ToString()
            };

            var response = await _transferUtility.S3Client.GetObjectAsync(request, token);

            return response.ResponseStream;
        }

        public async Task<Guid> CreatePhotoBlob(FileInfo photoPath, CancellationToken token)
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
