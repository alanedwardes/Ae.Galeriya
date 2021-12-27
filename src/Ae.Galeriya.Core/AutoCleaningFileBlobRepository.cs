using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public sealed class AutoCleaningFileBlobRepository : IFileBlobRepository
    {
        private readonly ILogger<AutoCleaningFileBlobRepository> _logger;
        private readonly DirectoryInfo _directory;
        private readonly long _maxSizeInBytes;
        private readonly IFileBlobRepository _fileBlobRepository;

        public AutoCleaningFileBlobRepository(ILogger<AutoCleaningFileBlobRepository> logger, DirectoryInfo directory, long maxSizeInBytes)
        {
            _logger = logger;
            _directory = directory;
            _maxSizeInBytes = maxSizeInBytes;
            _fileBlobRepository = new FileBlobRepository(directory);
        }

        public Task DeleteBlob(string blobId, CancellationToken token)
        {
            return _fileBlobRepository.DeleteBlob(blobId, token);
        }

        public Task<Stream> GetBlob(string blobId, CancellationToken token)
        {
            return _fileBlobRepository.GetBlob(blobId, token);
        }

        public FileInfo GetFileInfoForBlob(string fileName)
        {
            return _fileBlobRepository.GetFileInfoForBlob(fileName);
        }

        public Task PutBlob(Stream blobStream, string blobId, CancellationToken token)
        {
            Cleanup();
            return _fileBlobRepository.PutBlob(blobStream, blobId, token);
        }

        public void Cleanup()
        {
            var writeGracePeriod = TimeSpan.FromHours(24);

            var files = _directory.GetFiles();
            var totalSize = files.Sum(x => x.Length);
            var exceededBytes = totalSize - _maxSizeInBytes;
            if (exceededBytes > 0)
            {
                var reclaimedBytes = 0L;
                var reclaimedFiles = 0;
                foreach (var file in files.Where(x => x.LastWriteTimeUtc < (DateTime.UtcNow - writeGracePeriod)).OrderBy(x => x.LastAccessTimeUtc))
                {
                    if (reclaimedBytes > exceededBytes)
                    {
                        break;
                    }

                    try
                    {
                        var fileSize = file.Length;
                        file.Delete();
                        reclaimedBytes += fileSize;
                        reclaimedFiles++;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Unable to delete {File}", file);
                    }
                }

                _logger.LogInformation("Cleaned up {ReclaimedBytes} bytes ({ReclaimedFiles} files) in {Directory}", reclaimedBytes, reclaimedFiles, _directory);
            }
        }
    }
}