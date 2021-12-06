using Ae.Galeriya.Core.Tables;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface IPhotoCreator
    {
        Task<Photo> CreatePhoto(IFileBlobRepository fileBlobRepository, Category category, string fileName, string name, User user, DateTimeOffset creationDate, FileInfo uploadedFile, CancellationToken token);
    }
}
