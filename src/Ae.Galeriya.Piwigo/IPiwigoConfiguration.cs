using Ae.Galeriya.Core;
using System;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoConfiguration
    {
        IBlobRepository ChunkBlobRepository { get; }
        IFileBlobRepository FileBlobRepository { get; }
    }
}