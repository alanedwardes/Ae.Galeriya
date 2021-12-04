using Ae.Galeriya.Core;
using System;
using System.IO;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoConfiguration : IPiwigoConfiguration
    {
        public IBlobRepository ChunkBlobRepository { get; set; } = new FileBlobRepository(GetTempFolder());
        public IFileBlobRepository FileBlobRepository { get; set; } = new FileBlobRepository(GetTempFolder());

        private static DirectoryInfo GetTempFolder()
        {
            return new DirectoryInfo(Path.Combine(Path.GetTempPath(), "piwigo"));
        }
    }
}
