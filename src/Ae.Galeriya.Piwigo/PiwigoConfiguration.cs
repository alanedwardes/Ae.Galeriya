using Ae.Galeriya.Core;
using System;
using System.IO;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoConfiguration : GaleriyaConfiguration, IPiwigoConfiguration
    {
        public Uri BaseAddress { get; set; }
        public DirectoryInfo TempFolder { get; set; } = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "piwigo"));

        public PiwigoConfiguration()
        {
            TempFolder?.Create();
        }
    }
}
