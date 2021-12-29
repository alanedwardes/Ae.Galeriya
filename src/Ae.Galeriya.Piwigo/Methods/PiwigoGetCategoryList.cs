using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ae.Galeriya.Core.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoGetCategoryList : IPiwigoWebServiceMethod
    {
        private readonly IPiwigoImageDerivativesGenerator _derivativesGenerator;
        private readonly ICategoryPermissionsRepository _categoryRepository;
        private readonly IServiceProvider _serviceProvider;

        public string MethodName => "pwg.categories.getList";
        public bool AllowAnonymous => false;

        public PiwigoGetCategoryList(IPiwigoImageDerivativesGenerator derivativesGenerator, ICategoryPermissionsRepository categoryRepository, IServiceProvider serviceProvider)
        {
            _derivativesGenerator = derivativesGenerator;
            _categoryRepository = categoryRepository;
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<Photo> GetPhotosRecursive(Category category)
        {
            foreach (var photo in category.Photos)
            {
                yield return photo;
            }

            foreach (var child in category.Categories)
            {
                foreach (var photo in GetPhotosRecursive(child))
                {
                    yield return photo;
                }
            }
        }

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            var categoryId = parameters.GetRequired<uint>("cat_id");
            var recursive = parameters.GetOptional<bool>("recursive") ?? false;
            var thumbnailSize = parameters.GetRequired<string>("thumbnail_size");

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var nullableParentCategory = categoryId == 0 ? null : await _categoryRepository.EnsureCanAccessCategory(context, userId.Value, categoryId, token);

            var response = new PiwigoCategories { Categories = new List<PiwigoCategory>() };

            var categories = await _categoryRepository.GetAccessibleCategories(context, userId.Value).ToArrayAsync(token);

            foreach (var category in categories.Where(x => recursive || x.ParentCategory == nullableParentCategory))
            {
                uint firstPhoto = category.CoverPhotoId ?? GetPhotosRecursive(category).Select(x => x.PhotoId).FirstOrDefault();

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

                var lastPhoto = GetPhotosRecursive(category).LastOrDefault();

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
                    LastImageDate = lastPhoto?.TakenOn ?? lastPhoto?.CreatedOn,
                    PageLastImageDate = lastPhoto?.TakenOn ?? lastPhoto?.CreatedOn,
                    CategoryCount = categories.Count(x => x.ParentCategory == category),
                    Url = new Uri("https://www.example.com/"),
                    ThumbnailUrl = thumbnailUri
                });
            }

            return response;
        }
    }
}
