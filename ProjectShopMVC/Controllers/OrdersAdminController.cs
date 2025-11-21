using Microsoft.AspNetCore.Mvc;

namespace ProjectShopMVC.Controllers
{
    public class OrdersAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
