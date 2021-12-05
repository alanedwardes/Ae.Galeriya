using Ae.Galeriya.Core.Entities;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface IThumbnailGenerator
    {
        Task<Stream> GenerateThumbnail(Stream stream, MediaOrientation orientation, int width, int height, ResizeMode mode, CancellationToken token);
    }
}