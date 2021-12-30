using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Core
{
    internal sealed class TagRepository : ITagRepository
    {
        public async Task<Tag> CreateTag(GaleriyaDbContext dbContext, uint userId, string tagName, CancellationToken token)
        {
            var existingTag = await dbContext.Tags.SingleOrDefaultAsync(x => x.Name == tagName, token);
            if (existingTag != null)
            {
                return existingTag;
            }

            Tag tag = new()
            {
                Name = tagName,
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedById = userId
            };
            dbContext.Tags.Add(tag);

            try
            {
                await dbContext.SaveChangesAsync(token);
            }
            catch (DbUpdateException)
            {
                dbContext.Tags.Remove(tag);
                tag = await dbContext.Tags.SingleAsync(x => x.Name == tagName, token);
            }

            return tag;
        }
    }
}
