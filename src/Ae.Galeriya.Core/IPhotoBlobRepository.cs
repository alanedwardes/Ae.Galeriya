using Ae.Galeriya.Core.Entities;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface IPhotoBlobRepository
    {
        Task<(Guid BlobId, uint Width, uint Height)> CreatePhotoBlob(FileInfo photoPath, CancellationToken token);
        Task<Stream> GetPhotoBlob(Photo photo, CancellationToken token);
    }
}