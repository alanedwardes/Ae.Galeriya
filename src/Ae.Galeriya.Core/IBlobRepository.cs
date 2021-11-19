using Ae.Galeriya.Core.Tables;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface IBlobRepository
    {
        Task<Guid> PutBlob(FileInfo photoPath, CancellationToken token);
        Task<Stream> GetBlob(Photo photo, bool snapshot, CancellationToken token);
    }
}