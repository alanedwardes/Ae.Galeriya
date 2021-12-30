using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.WebUtilities;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetTagsAdmin : IPiwigoWebServiceMethod
    {
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.tags.getAdminList";
        public bool AllowAnonymous => false;

        public PiwigoGetTagsAdmin(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, FileMultipartSection> fileParameters, uint? userId, CancellationToken token)
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
