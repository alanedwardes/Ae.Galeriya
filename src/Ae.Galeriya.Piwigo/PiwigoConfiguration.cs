using System;
using System.IO;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoConfiguration : IPiwigoConfiguration
    {
        public Uri BaseAddress { get; set; }
        public DirectoryInfo TempFolder { get; set; } = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "piwigo"));

        public PiwigoConfiguration()
        {
            TempFolder?.Create();
        }
    }
}
