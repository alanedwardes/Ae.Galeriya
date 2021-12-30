using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    internal interface IUploadRepository
    {
        Task<FileInfo> AcceptChunk(string checksum, int chunk, int totalChunks, IFormFile file, CancellationToken token);
    }
}
