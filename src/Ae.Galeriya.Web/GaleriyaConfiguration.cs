namespace Ae.Galeriya.Console
{
    public sealed class GaleriyaConfiguration
    {
        public string ConnectionString { get; set; }
        public string GoogleApiKey { get; set; }
        public string BucketName { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string UploadCacheDirectory { get; set; }
        public long UploadCacheDirectorySize { get; set; }
        public string DataProtectionDirectory { get; set; }
        public string FfmpegDirectory { get; set; }
    }
}
