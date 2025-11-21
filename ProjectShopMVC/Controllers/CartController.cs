using Microsoft.AspNetCore.Mvc;
using ProjectShopMVC.Models;
using System.Text.Json;

namespace ProjectShopMVC.Controllers
{
    public class CartController : Controller
    {
        private const string SessionCartKey = "CartSession";

        private List<CartItem>? GetCart()
        {
            var data = HttpContext.Session.GetString(SessionCartKey);
            if (string.IsNullOrEmpty(data)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(data) ?? new List<CartItem>();
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }
    }
}
