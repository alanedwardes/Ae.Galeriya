using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoAddTag : IPiwigoWebServiceMethod
    {
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.tags.add";
        public bool AllowAnonymous => false;

        public PiwigoAddTag(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, IFormFile> fileParameters, uint? userId, CancellationToken token)
        {
            var name = parameters.GetRequired<string>("name");

            var tag = new Tag
            {
                Name = name,
                CreatedById = userId.Value,
                CreatedOn = DateTimeOffset.UtcNow
            };

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            context.Tags.Add(tag);

            try
            {
                await context.SaveChangesAsync(token);
            }
            catch (DbUpdateException)
            {
                tag = await context.Tags.SingleAsync(x => x.Name == name);
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
