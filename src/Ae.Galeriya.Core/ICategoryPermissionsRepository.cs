using Ae.Galeriya.Core.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface ICategoryPermissionsRepository
    {
        Task<Photo> EnsureCanAccessPhoto(uint userId, uint photoId, CancellationToken token);
        Task<Category> EnsureCanAccessCategory(uint userId, uint categoryId, CancellationToken token);
        Task<IReadOnlyCollection<Category>> GetAccessibleCategories(uint userId, CancellationToken token);
        Task<IQueryable<Photo>> GetAccessiblePhotos(uint userId, CancellationToken token);
    }
}