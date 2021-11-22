﻿using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Ae.Galeriya.Core.Tables;

namespace Ae.Galeriya.Piwigo.Methods
{

    internal sealed class PiwigoGetImages : IPiwigoWebServiceMethod
    {
        private readonly GaleriaDbContext _context;
        private readonly IPiwigoPhotosPageGenerator _pageGenerator;

        public string MethodName => "pwg.categories.getImages";

        public PiwigoGetImages(GaleriaDbContext context,
            IPiwigoPhotosPageGenerator pageGenerator)
        {
            _context = context;
            _pageGenerator = pageGenerator;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var page = parameters["page"].ToInt32(null);
            var perPage = parameters["per_page"].ToInt32(null);

            IQueryable<Photo> photosQuery = _context.Photos.Include(x => x.Categories);

            if (parameters.TryGetValue("cat_id", out var categoryIdRaw))
            {
                var categoryId = categoryIdRaw.ToUInt32(null);
                photosQuery = photosQuery.Where(photo => photo.Categories.Any(category => category.CategoryId == categoryId));
            }

            var photos = await photosQuery.ToArrayAsync(token);

            return _pageGenerator.CreatePage(page, perPage, photos.Length, photos);
        }
    }
}
