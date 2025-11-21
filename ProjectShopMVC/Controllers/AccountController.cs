using Microsoft.AspNetCore.Mvc;

namespace ProjectShopMVC.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
