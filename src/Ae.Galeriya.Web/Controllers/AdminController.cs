using Ae.Galeriya.Console;
using Ae.Galeriya.Core;
using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo;
using Ae.Galeriya.Web.Models;
using ImageMagick;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ae.Galeriya.Web.Controllers
{
    [Route("/admin/")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly GaleriyaConfiguration _configuration;
        private readonly IPiwigoConfiguration _piwigoConfiguration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPhotoMigrator _photoMigrator;

        public string AuthorizationHeader => "Authorization";
        public string BasicPrefix => "Basic";

        public AdminController(ILogger<AdminController> logger,
            UserManager<User> userManager,
            GaleriyaConfiguration configuration,
            IPiwigoConfiguration piwigoConfiguration,
            IServiceProvider serviceProvider,
            IPhotoMigrator photoMigrator)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _piwigoConfiguration = piwigoConfiguration;
            _serviceProvider = serviceProvider;
            _photoMigrator = photoMigrator;
        }

        private (string Username, string Password)? GetBasicAuth()
        {
            if (!Request.Headers.TryGetValue(AuthorizationHeader, out var authorizationHeader))
            {
                return null;
            }

            var auth = authorizationHeader.ToString()[BasicPrefix.Length..];

            byte[] decoded = Convert.FromBase64String(auth);
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            string[] authPair = iso.GetString(decoded).Split(':');

            if (string.IsNullOrWhiteSpace(authPair[0]) || string.IsNullOrWhiteSpace(authPair[1]))
            {
                return null;
            }

            return (authPair[0], authPair[1]);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var basicAuth = GetBasicAuth();

            if (basicAuth.HasValue &&
                basicAuth.Value.Username == _configuration.AdminUsername &&
                basicAuth.Value.Password == _configuration.AdminPassword)
            {
                base.OnActionExecuting(context);
                return;
            }

            Response.Headers.Add("WWW-Authenticate", "Basic");
            context.Result = Unauthorized();
        }

        public async Task<IActionResult> Index()
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            return View(new IndexModel
            {
                Users = await _userManager.Users.ToArrayAsync(),
                Categories = await context.Categories
                    .Where(x => x.ParentCategory == null)
                    .Include(x => x.Users)
                    .Include(x => x.Photos)
                    .ToArrayAsync(),
                Tags = await context.Tags
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
            var result = await _userManager.CreateAsync(new User { UserName = userModel.Username }, userModel.Password ?? string.Empty);
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
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var category = await context.Categories.Include(x => x.Users).SingleAsync(x => x.CategoryId == categoryId);
            return View(new EditCategoryModel
            {
                Category = category,
                Users = await _userManager.Users.ToArrayAsync()
            });
        }

        [HttpPost("categories/{CategoryId}/edit")]
        public async Task<IActionResult> EditCategory([FromRoute] uint categoryId, [FromForm] string[] userIds)
        {
            using var context = _serviceProvider.GetRequiredService<GaleriyaDbContext>();

            var category = await context.Categories.Include(x => x.Users).SingleAsync(x => x.CategoryId == categoryId);

            var newUsers = new List<User>();

            foreach (var userId in userIds)
            {
                newUsers.Add(await _userManager.FindByIdAsync(userId));
            }

            category.Users = newUsers;

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("migrate")]
        public async Task Migrate()
        {
            var tempRepository = _piwigoConfiguration.TemporaryBlobRepository(_serviceProvider);
            var photoRepository = _piwigoConfiguration.PersistentBlobRepository(_serviceProvider);

            await _photoMigrator.MigratePhotos(photoRepository, tempRepository, Request.HttpContext.RequestAborted);
        }
    }
}
