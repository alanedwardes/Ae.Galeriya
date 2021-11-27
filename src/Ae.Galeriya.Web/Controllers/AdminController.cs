using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Ae.Galeriya.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly GaleriaDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(GaleriaDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(new IndexModel
            {
                Users = await _userManager.Users.ToArrayAsync(),
                Categories = await _context.Categories
                    .Where(x => x.ParentCategory == null)
                    .Include(x => x.Users)
                    .Include(x => x.Photos)
                    .ToArrayAsync()
            });
        }

        [HttpGet("users")]
        public IActionResult AddUser()
        {
            return View("EditUser");
        }

        [HttpPost("users")]
        public async Task<IActionResult> AddUser([FromForm] EditUserModel userModel)
        {
            var result = await _userManager.CreateAsync(new IdentityUser { UserName = userModel.Username }, userModel.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            return View("EditUser", new EditUserModelErrorFail
            {
                Username = userModel.Username,
                Password = userModel.Password,
                Errors = result.Errors.ToArray()
            });
        }

        [HttpGet("users/{UserId}/edit")]
        public async Task<IActionResult> EditUser([FromRoute] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return View(new EditUserModel
            {
                Username = user?.UserName
            });
        }

        [HttpPost("users/{UserId}/edit")]
        public async Task<IActionResult> EditUser([FromRoute] string userId, [FromForm] EditUserModel userModel)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var userNameChangeResult = await _userManager.SetUserNameAsync(user, userModel.Username);
            
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordChangeResult = await _userManager.ResetPasswordAsync(user, token, userModel.Password ?? string.Empty);

            if (!userNameChangeResult.Succeeded || !passwordChangeResult.Succeeded)
            {
                return View(new EditUserModelErrorFail
                {
                    Username = userModel.Username,
                    Password = userModel.Password,
                    Errors = userNameChangeResult.Errors.Concat(passwordChangeResult.Errors).ToArray()
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("users/{UserId}/delete")]
        public async Task<IActionResult> DeleteUser([FromRoute] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("categories/{CategoryId}/edit")]
        public async Task<IActionResult> EditCategory([FromRoute] uint categoryId)
        {
            var category = await _context.Categories.Include(x => x.Users).SingleAsync(x => x.CategoryId == categoryId);
            return View(new EditCategoryModel
            {
                Category = category,
                Users = await _userManager.Users.ToArrayAsync()
            });
        }

        [HttpPost("categories/{CategoryId}/edit")]
        public async Task<IActionResult> EditCategory([FromRoute] uint categoryId, [FromForm] string[] userIds)
        {
            var category = await _context.Categories.SingleAsync(x => x.CategoryId == categoryId);

            var categoryUsers = await _context.CategoryUsers.Where(x => x.Category == category).ToArrayAsync();

            foreach (var categoryUser in categoryUsers)
            {
                _context.CategoryUsers.Remove(categoryUser);
            }

            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);

                _context.CategoryUsers.Add(new CategoryUser
                {
                    Category = category,
                    User = user
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
