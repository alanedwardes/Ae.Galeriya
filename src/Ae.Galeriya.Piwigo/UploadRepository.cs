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
        public UploadRepository(IPiwigoConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        private readonly IDictionary<(string, int), string> _uploadedChunks = new ConcurrentDictionary<(string, int), string>();

        private readonly IPiwigoConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private (string, int) ChunkKey(string checksum, int chunk) => (checksum, chunk);

        public async Task<FileInfo> AcceptChunk(string checksum, int chunk, int totalChunks, IFormFile file, CancellationToken token)
        {
            var chunkId = Guid.NewGuid().ToString();
            await _configuration.ChunkBlobRepository(_serviceProvider).PutBlob(file.OpenReadStream(), chunkId, token);

            _uploadedChunks[ChunkKey(checksum, chunk)] = chunkId;

            var parts = Enumerable.Range(0, totalChunks)
                .Select(x => ChunkKey(checksum, x));

            if (parts.All(_uploadedChunks.ContainsKey))
            {
                var uploadFile = _configuration.FileBlobRepository(_serviceProvider).GetFileInfoForBlob(Guid.NewGuid().ToString());

                using (var fileWriteStream = uploadFile.Open(FileMode.Create))
                {
                    foreach (var part in parts)
                    {
                        using (var stream = await _configuration.ChunkBlobRepository(_serviceProvider).GetBlob(_uploadedChunks[part], token))
                        {
                            await stream.CopyToAsync(fileWriteStream, token);
                        }
                    }
                }

                return uploadFile;
            }

            return null;
        }
    }
}
