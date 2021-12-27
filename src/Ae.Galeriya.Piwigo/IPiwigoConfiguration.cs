using Ae.Galeriya.Core;
using System;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoConfiguration
    {
        Func<IServiceProvider, IBlobRepository> ChunkBlobRepository { get; }
        Func<IServiceProvider, IFileBlobRepository> TemporaryBlobRepository { get; }
        Func<IServiceProvider, IFileBlobRepository> ThumbnailBlobRepository { get; }
        Func<IServiceProvider, IBlobRepository> PersistentBlobRepository { get; }
    }
}