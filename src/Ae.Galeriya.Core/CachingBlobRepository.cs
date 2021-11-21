using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public sealed class CachingBlobRepository : IBlobRepository
    {
        private readonly DirectoryInfo _cacheDirectoy;
        private readonly IBlobRepository _blobRepository;

        public CachingBlobRepository(DirectoryInfo cacheDirectoy, IBlobRepository blobRepository)
        {
            _cacheDirectoy = cacheDirectoy;
            _blobRepository = blobRepository;
        }

        private FileInfo GetCacheFileBlob(Guid blobId)
        {
            return new FileInfo(Path.Combine(_cacheDirectoy.FullName, blobId.ToString()));
        }

        public async Task<Stream> GetBlob(Guid blobId, CancellationToken token)
        {
            var cacheFile = GetCacheFileBlob(blobId);
            if (!cacheFile.Exists)
            {
                using (var blobStream = await _blobRepository.GetBlob(blobId, token))
                using (var cacheStream = cacheFile.Open(FileMode.CreateNew, FileAccess.Write))
                {
                    await blobStream.CopyToAsync(cacheStream, token);
                }
            }

            return cacheFile.OpenRead();
        }

        public async Task PutBlob(FileInfo blobPath, Guid blobId, CancellationToken token)
        {
            var cacheFile = GetCacheFileBlob(blobId);
            await _blobRepository.PutBlob(blobPath, blobId, token);
            blobPath.CopyTo(cacheFile.FullName);
        }
    }
}