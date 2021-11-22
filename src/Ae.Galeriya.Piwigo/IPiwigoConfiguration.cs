using Ae.Galeriya.Core;
using System;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoConfiguration
    {
        Uri BaseAddress { get; }
        IBlobRepository ChunkBlobRepository { get; }
        IFileBlobRepository FileBlobRepository { get; }
    }
}