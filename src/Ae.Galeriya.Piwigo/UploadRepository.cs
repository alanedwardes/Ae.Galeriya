using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    internal sealed class UploadRepository : IUploadRepository
    {
        private readonly DirectoryInfo _tempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "piwigo"));

        public FileInfo CreateTempFile(string name) => new FileInfo(Path.Combine(_tempDirectory.FullName, name));

        public async Task<FileInfo> AcceptChunk(string checksum, int chunk, int totalChunks, IFormFile file, CancellationToken token)
        {
            _tempDirectory.Create();

            FileInfo ChunkFileInfo(int chunk)
            {
                return CreateTempFile(checksum + "-" + chunk);
            }

            var chunkFile = ChunkFileInfo(chunk);

            using (var chunkStream = chunkFile.Open(FileMode.Create))
            {
                await file.CopyToAsync(chunkStream, token);
            }

            var parts = Enumerable.Range(0, totalChunks)
                .Select(x => ChunkFileInfo(x));

            if (parts.All(x => x.Exists))
            {
                var uploadFile = new FileInfo(Path.Combine(_tempDirectory.FullName, Guid.NewGuid().ToString()));

                using (var fileWriteStream = uploadFile.Open(FileMode.Create))
                {
                    foreach (var part in parts)
                    {
                        using var stream = part.OpenRead();
                        await stream.CopyToAsync(fileWriteStream, token);
                    }
                }

                return uploadFile;
            }

            return null;
        }
    }
}
