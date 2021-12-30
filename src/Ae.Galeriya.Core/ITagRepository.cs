using Ae.Galeriya.Core.Tables;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    internal interface ITagRepository
    {
        Task<Tag> CreateTag(GaleriyaDbContext dbContext, uint userId, string tagName, CancellationToken token);
    }
}