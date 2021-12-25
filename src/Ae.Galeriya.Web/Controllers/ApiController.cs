using Ae.Galeriya.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Web.Controllers
{
    [Authorize]
    [Route("/api/v1")]
    public sealed class ApiController : Controller
    {
        private readonly GaleriyaDbContext _dbContext;

        public ApiController(GaleriyaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("hashes:query")]
        public async Task<string[]> QueryHashes([FromBody] string[] hashes, CancellationToken token)
        {
            return await _dbContext.Photos.Where(x => hashes.Contains(x.BlobId))
                                          .Select(x => x.BlobId)
                                          .ToArrayAsync(token);
        }
    }
}
