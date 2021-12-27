using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetTags : IPiwigoWebServiceMethod
    {
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.tags.getList";
        public bool AllowAnonymous => false;

        public PiwigoGetTags(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var tags = await context.Tags.Include(x => x.Photos).ToArrayAsync();

            return new PiwigoTags
            {
                Tags = tags.Select(x => new PiwigoTag
                {
                    Name = x.Name,
                    Counter = (uint)x.Photos.Count,
                    LastModified = x.CreatedOn,
                    TagId = x.TagId,
                    Slug = x.Name.ToLower().Replace(' ', '-')
                }).ToArray()
            };
        }
    }
}
