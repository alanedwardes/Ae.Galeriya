using Ae.Galeriya.Core;
using System;
using System.IO;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoConfiguration
    {
        Uri BaseAddress { get; }
        IBlobRepository ChunkBlobRepository { get; }
        IFileBlobRepository FileBlobRepository { get; }
    }
}