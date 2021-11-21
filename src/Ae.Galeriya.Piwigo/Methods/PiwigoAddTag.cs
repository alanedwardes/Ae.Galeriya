using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoAddTag : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _context;

        public string MethodName => "pwg.tags.add";

        public PiwigoAddTag(GalleriaDbContext context)
        {
            _context = context;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var name = parameters["name"].ToString(null);

            var tag = new Tag
            {
                Name = name,
                CreatedOn = DateTimeOffset.UtcNow
            };

            _context.Tags.Add(tag);

            try
            {
                await _context.SaveChangesAsync(token);
            }
            catch (DbUpdateException)
            {
                tag = await _context.Tags.SingleAsync(x => x.Name == name);
            }

            return new PiwigoAddedTagResponse
            {
                Info = "Album added",
                Id = tag.TagId,
                Name = tag.Name,
                Slug = tag.GenerateSlug()
            };
        }
    }
}
