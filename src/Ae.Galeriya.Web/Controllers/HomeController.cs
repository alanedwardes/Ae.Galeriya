using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(Request.HttpContext.User.Identity.Name);

            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var categories = await _categoryPermissions.GetAccessibleCategories(context, user.Id).ToArrayAsync();

            return View(new HomeModel
            {
                Categories = categories,
            });
        }
    }
}
