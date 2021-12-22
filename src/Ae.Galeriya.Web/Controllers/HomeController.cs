using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ICategoryPermissionsRepository _categoryPermissions;
        private readonly UserManager<User> _userManager;

        public HomeController(ICategoryPermissionsRepository categoryPermissions, UserManager<User> userManager)
        {
            _categoryPermissions = categoryPermissions;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(Request.HttpContext.User.Identity.Name);

            var categories = await _categoryPermissions.GetAccessibleCategories(user.Id, CancellationToken.None);

            return View(new HomeModel
            {
                Categories = categories,
            });
        }
    }
}
