using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface IPhotoMigrator
    {
        Task MigratePhotos(IBlobRepository photoRepository, IFileBlobRepository tempRepository, CancellationToken token);
    }
}