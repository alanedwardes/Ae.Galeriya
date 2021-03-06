using Ae.Galeriya.Core.Tables;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface ICategoryPermissionsRepository
    {
        Task<Photo> EnsureCanAccessPhoto(GaleriyaDbContext dbContext, uint userId, uint photoId, CancellationToken token);
        Task<Category> EnsureCanAccessCategory(GaleriyaDbContext dbContext, uint userId, uint categoryId, CancellationToken token);
        IQueryable<Category> GetAccessibleCategories(GaleriyaDbContext dbContext, uint userId);
        IQueryable<PhotoSummary> GetAccessiblePhotos(GaleriyaDbContext dbContext, uint userId);
    }
}