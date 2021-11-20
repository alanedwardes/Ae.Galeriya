using System.IO;

namespace Ae.Galeriya.Core
{
    public interface IGaleriyaConfiguration
    {
        string BucketName { get; }
        DirectoryInfo BucketCache { get; }
    }
}