using Ae.MediaMetadata.Entities;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface IThumbnailGenerator
    {
        Task GenerateThumbnail(Stream stream, FileInfo fileInfo, MediaOrientation orientation, int width, int height, CancellationToken token);
    }
}