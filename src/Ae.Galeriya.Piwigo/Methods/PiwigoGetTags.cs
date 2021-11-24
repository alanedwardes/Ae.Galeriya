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
    internal sealed class PiwigoGetTags : IPiwigoWebServiceMethod
    {
        private readonly GaleriaDbContext _context;

        public string MethodName => "pwg.tags.getList";
        public bool AllowAnonymous => false;

        public PiwigoGetTags(GaleriaDbContext context)
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
