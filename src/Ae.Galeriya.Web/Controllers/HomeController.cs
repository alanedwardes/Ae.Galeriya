using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly UserManager<User> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(ICategoryPermissionsRepository categoryPermissions, UserManager<User> userManager, IServiceProvider serviceProvider)
        {
            _categoryPermissions = categoryPermissions;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Index(CancellationToken token)
        {
            var user = await _userManager.FindByNameAsync(Request.HttpContext.User.Identity.Name);

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var categories = await _categoryPermissions.GetAccessibleCategories(context, user.Id)
                .Where(x => x.ParentCategory == null)
                .ToArrayAsync(token);

            return View(new HomeModel
            {
                Categories = categories,
            });
        }

        [HttpGet("/categories/{categoryId}/")]
        public async Task<IActionResult> Categories([FromRoute] uint categoryId, CancellationToken token)
        {
            var user = await _userManager.FindByNameAsync(Request.HttpContext.User.Identity.Name);

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var categories = await _categoryPermissions.GetAccessibleCategories(context, user.Id)
                .Where(x => x.ParentCategoryId == categoryId)
                .ToArrayAsync(token);

            var photos = await _categoryPermissions.GetAccessiblePhotos(context, user.Id)
                .Where(x => x.Categories.Select(x => x.CategoryId).Contains(categoryId))
                .ToArrayAsync(token);

            return View("Index", new HomeModel
            {
                Categories = categories,
                Photos = photos
            });
        }
    }
}
