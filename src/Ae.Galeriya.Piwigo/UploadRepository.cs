using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    internal sealed class UploadRepository : IUploadRepository
    {
        public UploadRepository(IPiwigoConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IDictionary<(string, int), Guid> _uploadedChunks = new ConcurrentDictionary<(string, int), Guid>();

        private readonly IPiwigoConfiguration _configuration;

        private (string, int) ChunkKey(string checksum, int chunk) => (checksum, chunk);

        public async Task<FileInfo> AcceptChunk(string checksum, int chunk, int totalChunks, IFormFile file, CancellationToken token)
        {
            var chunkId = Guid.NewGuid();
            await _configuration.ChunkBlobRepository.PutBlob(file.OpenReadStream(), chunkId, token);

            _uploadedChunks[ChunkKey(checksum, chunk)] = chunkId;

            var parts = Enumerable.Range(0, totalChunks)
                .Select(x => ChunkKey(checksum, x));

            if (parts.All(_uploadedChunks.ContainsKey))
            {
                var uploadFile = _configuration.FileBlobRepository.GetFileInfoForBlob(Guid.NewGuid().ToString());

                using (var fileWriteStream = uploadFile.Open(FileMode.Create))
                {
                    foreach (var part in parts)
                    {
                        var partStream = _uploadedChunks[part];
                        using var stream = await _configuration.ChunkBlobRepository.GetBlob(partStream, token);
                        await stream.CopyToAsync(fileWriteStream, token);
                    }
                }

                return uploadFile;
            }

            return null;
        }
    }
}
