using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ae.Galeriya.Web.Controllers
{
    [Authorize]
    public class PhotosController : Controller
    {
        public IActionResult Upload()
        {
            return View();
        }
    }
}
