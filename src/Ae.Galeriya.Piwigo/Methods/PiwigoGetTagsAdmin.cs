﻿using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetTagsAdmin : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _context;

        public string MethodName => "pwg.tags.getAdminList";

        public PiwigoGetTagsAdmin(GalleriaDbContext context)
        {
            _context = context;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var tags = await _context.Tags.Include(x => x.Photos).ToArrayAsync();

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