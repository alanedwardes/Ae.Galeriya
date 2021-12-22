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

        public async Task DeleteBlob(string blobId, CancellationToken token)
        {
            await Task.WhenAll(_cacheBlobRepository.DeleteBlob(blobId, token),
                               _liveBlobRepository.DeleteBlob(blobId, token));
        }

        public async Task<Stream> GetBlob(string blobId, CancellationToken token)
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

        public async Task PutBlob(Stream blobStream, string blobId, CancellationToken token)
        {
            using (blobStream)
            {
                await _cacheBlobRepository.PutBlob(blobStream, blobId, token);
            }

            using (var cachedBlob = await _cacheBlobRepository.GetBlob(blobId, token))
            {
                await _liveBlobRepository.PutBlob(cachedBlob, blobId, token);
            }
        }
    }
}