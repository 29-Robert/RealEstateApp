using Microsoft.AspNetCore.Mvc;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
