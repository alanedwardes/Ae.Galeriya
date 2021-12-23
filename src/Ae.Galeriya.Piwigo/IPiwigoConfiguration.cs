using Ae.Galeriya.Core;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoConfiguration
    {
        IBlobRepository ChunkBlobRepository { get; }
        IFileBlobRepository FileBlobRepository { get; }
    }
}