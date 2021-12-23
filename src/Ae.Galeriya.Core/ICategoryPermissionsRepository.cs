using Ae.Galeriya.Core.Tables;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface ICategoryPermissionsRepository
    {
        Task<Photo> EnsureCanAccessPhoto(uint userId, uint photoId, CancellationToken token);
        Task<Category> EnsureCanAccessCategory(uint userId, uint categoryId, CancellationToken token);
        IQueryable<Category> GetAccessibleCategories(uint userId);
        IQueryable<Photo> GetAccessiblePhotos(uint userId);
    }
}