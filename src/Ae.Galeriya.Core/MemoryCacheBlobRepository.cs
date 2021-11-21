using Ae.Galeriya.Core.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public sealed class MemoryCacheBlobRepository : IBlobRepository
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheBlobRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<Stream> GetBlob(Guid blobId, CancellationToken token)
        {
            if (_memoryCache.TryGetValue(blobId.ToString(), out byte[] value))
            {
                return Task.FromResult<Stream>(new MemoryStream(value));
            }

            throw new BlobNotFoundException(blobId);
        }

        public async Task PutBlob(Stream blobStream, Guid blobId, CancellationToken token)
        {
            using var ms = new MemoryStream();
            await blobStream.CopyToAsync(ms, token);
            _memoryCache.Set(blobId, ms.ToArray());
        }
    }
}