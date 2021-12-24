using Ae.Galeriya.Core;
using System;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoConfiguration
    {
        Func<IServiceProvider, IBlobRepository> ChunkBlobRepository { get; }
        Func<IServiceProvider, IFileBlobRepository> FileBlobRepository { get; }
    }
}