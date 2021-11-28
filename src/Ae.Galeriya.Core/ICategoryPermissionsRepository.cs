using Ae.Galeriya.Core.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface ICategoryPermissionsRepository
    {
        Task<Photo> EnsureCanAccessPhoto(User user, uint photoId, CancellationToken token);
        Task<Category> EnsureCanAccessCategory(User user, uint categoryId, CancellationToken token);
        Task<IReadOnlyCollection<Category>> GetAccessibleCategories(User user, CancellationToken token);
        Task<IQueryable<Photo>> GetAccessiblePhotos(User user, CancellationToken token);
    }
}