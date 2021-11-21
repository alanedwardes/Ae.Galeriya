using Ae.Galeriya.Core.Exceptions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public sealed class FileBlobRepository : IBlobRepository
    {
        private readonly DirectoryInfo _cacheDirectoy;

        public FileBlobRepository(DirectoryInfo cacheDirectoy)
        {
            cacheDirectoy.Create();
            _cacheDirectoy = cacheDirectoy;
        }

        private FileInfo GetFileInfoForBlob(Guid blobId)
        {
            return new FileInfo(Path.Combine(_cacheDirectoy.FullName, blobId.ToString()));
        }

        public Task<Stream> GetBlob(Guid blobId, CancellationToken token)
        {
            try
            {
                return Task.FromResult<Stream>(GetFileInfoForBlob(blobId).OpenRead());
            }
            catch (IOException)
            {
                throw new BlobNotFoundException(blobId);
            }
        }

        public async Task PutBlob(Stream blobStream, Guid blobId, CancellationToken token)
        {
            using (var toStream = GetFileInfoForBlob(blobId).OpenWrite())
            using (blobStream)
            {
                await blobStream.CopyToAsync(toStream);
            }
        }
    }
}