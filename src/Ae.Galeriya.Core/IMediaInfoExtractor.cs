using Ae.Galeriya.Core.Entities;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface IMediaInfoExtractor
    {
        Task<MediaInfo> ExtractInformation(FileInfo fileInfo, CancellationToken token);
        Task ExtractSnapshot(FileInfo fileInfo, FileInfo output, CancellationToken token);
    }
}