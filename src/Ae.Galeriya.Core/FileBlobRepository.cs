using Ae.Galeriya.Core.Exceptions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public sealed class FileBlobRepository : IFileBlobRepository
    {
        private readonly DirectoryInfo _cacheDirectoy;

        public FileBlobRepository(DirectoryInfo cacheDirectoy)
        {
            cacheDirectoy.Create();
            _cacheDirectoy = cacheDirectoy;
        }

        public FileInfo GetFileInfoForBlob(string fileName)
        {
            return new FileInfo(Path.Combine(_cacheDirectoy.FullName, fileName));
        }

        public Task<Stream> GetBlob(Guid blobId, CancellationToken token)
        {
            try
            {
                return Task.FromResult<Stream>(GetFileInfoForBlob(blobId.ToString()).OpenRead());
            }
            catch (IOException)
            {
                throw new BlobNotFoundException(blobId);
            }
        }

        public async Task PutBlob(Stream blobStream, Guid blobId, CancellationToken token)
        {
            using (var toStream = GetFileInfoForBlob(blobId.ToString()).OpenWrite())
            using (blobStream)
            {
                await blobStream.CopyToAsync(toStream);
            }
        }
    }
}