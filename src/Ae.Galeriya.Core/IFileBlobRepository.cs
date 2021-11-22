using System.IO;

namespace Ae.Galeriya.Core
{
    public interface IFileBlobRepository : IBlobRepository
    {
        FileInfo GetFileInfoForBlob(string fileName);
    }
}