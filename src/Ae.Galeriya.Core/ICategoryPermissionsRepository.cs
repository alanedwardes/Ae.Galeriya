using Ae.Galeriya.Core.Tables;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    public interface ICategoryPermissionsRepository
    {
        bool CanAccessPhoto(User user, Photo photo);
        void EnsureCanAccessPhoto(User user, Photo photo);
        Task<Photo> EnsureCanAccessPhoto(User user, uint photoId, CancellationToken token);
        bool CanAccessCategory(User user, Category category);
        void EnsureCanAccessCategory(User user, Category category);
        Task<Category> EnsureCanAccessCategory(User user, uint categoryId, CancellationToken token);
        Task<IReadOnlyCollection<Category>> GetAccessibleCategories(User user, CancellationToken token);
    }
}