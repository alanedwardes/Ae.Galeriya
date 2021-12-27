using Ae.Galeriya.Core.Exceptions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public sealed class FileBlobRepository : IFileBlobRepository
    {
        private readonly DirectoryInfo _directory;

        public FileBlobRepository(DirectoryInfo directory)
        {
            directory.Create();
            _directory = directory;
        }

        public FileInfo GetFileInfoForBlob(string fileName)
        {
            return new FileInfo(Path.Combine(_directory.FullName, fileName));
        }

        public Task<Stream> GetBlob(string blobId, CancellationToken token)
        {
            try
            {
                return Task.FromResult<Stream>(GetFileInfoForBlob(blobId.ToString()).OpenRead());
            }
            catch (IOException)
            {
                throw new BlobNotFoundException(blobId);
            }
        }

        public async Task PutBlob(Stream blobStream, string blobId, CancellationToken token)
        {
            var incompleteFileInfo = GetFileInfoForBlob(blobId.ToString() + "_incomplete");
            var completeFileInfo = GetFileInfoForBlob(blobId.ToString());

            using (var toStream = incompleteFileInfo.OpenWrite())
            using (blobStream)
            {
                await blobStream.CopyToAsync(toStream, token);
            }

            incompleteFileInfo.MoveTo(completeFileInfo.FullName, true);
        }

        public Task DeleteBlob(string blobId, CancellationToken token)
        {
            GetFileInfoForBlob(blobId.ToString()).Delete();
            return Task.CompletedTask;
        }
    }
}