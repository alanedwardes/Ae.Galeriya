using Ae.Galeriya.Core.Exceptions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{

    public sealed class CachingBlobRepository : IBlobRepository
    {
        private readonly IBlobRepository _cacheBlobRepository;
        private readonly IBlobRepository _liveBlobRepository;

        public CachingBlobRepository(IBlobRepository cacheBlobRepository, IBlobRepository liveBlobRepository)
        {
            _cacheBlobRepository = cacheBlobRepository;
            _liveBlobRepository = liveBlobRepository;
        }

        public async Task<Stream> GetBlob(Guid blobId, CancellationToken token)
        {
            try
            {
                return await _cacheBlobRepository.GetBlob(blobId, token);
            }
            catch (BlobNotFoundException)
            {
                using (var fromBlob = await _liveBlobRepository.GetBlob(blobId, token))
                {
                    await _cacheBlobRepository.PutBlob(fromBlob, blobId, token);
                }

                return await _cacheBlobRepository.GetBlob(blobId, token);
            }
        }

        public async Task PutBlob(Stream blobStream, Guid blobId, CancellationToken token)
        {
            await _cacheBlobRepository.PutBlob(blobStream, blobId, token);
            var cachedBlob = await _cacheBlobRepository.GetBlob(blobId, token);
            await _liveBlobRepository.PutBlob(cachedBlob, blobId, token);
        }
    }
}