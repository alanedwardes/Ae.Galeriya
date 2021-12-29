using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    internal interface IPiwigoPhotosPageGenerator
    {
        Task<PiwigoImages> CreatePage(int? page, int? perPage, IQueryable<Photo> query, CancellationToken token);
    }
}