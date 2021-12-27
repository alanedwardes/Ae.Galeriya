using Ae.Galeriya.Core;
using System;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoConfiguration : IPiwigoConfiguration
    {
        public Func<IServiceProvider, IBlobRepository> ChunkBlobRepository { get; set; }
        public Func<IServiceProvider, IFileBlobRepository> TemporaryBlobRepository { get; set; }
        public Func<IServiceProvider, IFileBlobRepository> ThumbnailBlobRepository { get; set; }
        public Func<IServiceProvider, IBlobRepository> PersistentBlobRepository { get; set; }
    }
}
