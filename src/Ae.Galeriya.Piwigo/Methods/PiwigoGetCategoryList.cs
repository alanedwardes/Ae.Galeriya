using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetCategoryList : IPiwigoWebServiceMethod
    {
        private readonly GalleriaDbContext _context;
        private readonly IPiwigoConfiguration _configuration;
        private readonly IPiwigoImageDerivativesGenerator _derivativesGenerator;

        public string MethodName => "pwg.categories.getList";

        public PiwigoGetCategoryList(GalleriaDbContext context, IPiwigoConfiguration configuration, IPiwigoImageDerivativesGenerator derivativesGenerator)
        {
            _context = context;
            _configuration = configuration;
            _derivativesGenerator = derivativesGenerator;
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var categoryId = parameters["cat_id"].ToUInt32(null);
            var recursive = parameters["recursive"].ToBoolean(null);
            var thumbnailSize = parameters["thumbnail_size"].ToString(null);

            var nullableCategoryId = categoryId == 0 ? (uint?)null : categoryId;

            var response = new PiwigoCategories { Categories = new List<PiwigoCategory>() };

            var allCategories = await _context.Categories.Include(x => x.Photos).Include(x => x.Categories).AsSplitQuery().ToArrayAsync(token);

            foreach (var category in allCategories.Where(x => recursive || x.ParentCategoryId == nullableCategoryId))
            {
                uint firstPhoto = category.CoverPhotoId ?? category.Photos.Select(x => x.PhotoId).FirstOrDefault();

                uint? thumbnailId = null;
                Uri thumbnailUri = null;
                if (firstPhoto > 0)
                {
                    var thumb = _derivativesGenerator.GenerateDerivatives(firstPhoto);
                    thumbnailUri = thumb[thumbnailSize].Url;
                    thumbnailId = firstPhoto;
                }

                IList<Category> upperCategories = new List<Category>();

                Category parent = category;
                while (parent != null)
                {
                    upperCategories.Add(parent);
                    parent = parent.ParentCategory;
                }

                response.Categories.Add(new PiwigoCategory
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Comment = category.Comment,
                    Permalink = null,
                    UpperCategories = string.Join(',', upperCategories.Reverse().Select(x => x.CategoryId)),
                    GlobalRank = "1",
                    UpperCategoryId = category.ParentCategoryId,
                    ImageCount = category.Photos.Count,
                    TotalImageCount = category.Photos.Count,
                    RepresentativePictureId = firstPhoto,
                    LastImageDate = category.Photos.LastOrDefault()?.CreatedOn,
                    PageLastImageDate = category.Photos.LastOrDefault()?.CreatedOn,
                    CategoryCount = category.Categories.Count,
                    Url = new Uri("https://www.example.com/"),
                    ThumbnailUrl = thumbnailUri
                });
            }

            return response;
        }
    }
}
