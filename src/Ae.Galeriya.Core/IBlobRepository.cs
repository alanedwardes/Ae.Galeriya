using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface IBlobRepository
    {
        Task PutBlob(Stream blobStream, string blobId, CancellationToken token);
        Task<Stream> GetBlob(string blobId, CancellationToken token);
        Task DeleteBlob(string blobId, CancellationToken token);
    }
}