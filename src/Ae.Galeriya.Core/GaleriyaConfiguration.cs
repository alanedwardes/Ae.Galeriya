using System.IO;

namespace Ae.Galeriya.Core
{
    public abstract class GaleriyaConfiguration : IGaleriyaConfiguration
    {
        public string BucketName { get; set; }
        public DirectoryInfo BucketCache { get; set; } = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "galeriya"));

        public GaleriyaConfiguration()
        {
            BucketCache?.Create();
        }
    }
}
