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
            if (_memoryCache.TryGetValue(blobId.ToString(), out object value))
            {
                return Task.FromResult<Stream>(new MemoryStream((byte[])value));
            }

            throw new BlobNotFoundException(blobId);
        }

        public async Task PutBlob(Stream blobStream, Guid blobId, CancellationToken token)
        {
            using var ms = new MemoryStream();

            using (blobStream)
            {
                await blobStream.CopyToAsync(ms, token);
            }

            using var memoryCacheItem = _memoryCache.CreateEntry(blobId.ToString());
            memoryCacheItem.SetValue(ms.ToArray());
            memoryCacheItem.SetSize(ms.Length);
        }
    }
}