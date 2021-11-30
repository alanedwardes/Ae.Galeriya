using System;

namespace Ae.Galeriya.Console
{
    public sealed class GaleriyaConfiguration
    {
        public string BindAddress { get; set; }
        public Uri BaseAddress { get; set; }
        public string BucketName { get; set; }
        public long MemoryCacheSize { get; set; } = 512_000_000;
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string UploadCacheDirectory { get; set; }
        public string SqliteDatabaseFile { get; set; }
        public string DataProtectionDirectory { get; set; }
        public string FfmpegDirectory { get; set; }
    }
}
