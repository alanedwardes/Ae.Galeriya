using Ae.Galeriya.Core;
using System;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoConfiguration : IPiwigoConfiguration
    {
        public Func<IServiceProvider, IBlobRepository> ChunkBlobRepository { get; set; }
        public Func<IServiceProvider, IFileBlobRepository> FileBlobRepository { get; set; }
    }
}
