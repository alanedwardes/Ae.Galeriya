using Ae.Galeriya.Core.Tables;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface IPhotoBlobRepository
    {
        Task<Guid> CreatePhotoBlob(FileInfo photoPath, CancellationToken token);
        Task<Stream> GetPhotoBlob(Photo photo, CancellationToken token);
    }
}