using Microsoft.AspNetCore.Mvc;
using ProjectShopMVC.Data;
using ProjectShopMVC.Models;
using System.Text.Json;

namespace ProjectShopMVC.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string SessionCartKey = "CartSession";

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ShowCount()
        {
            var cart = GetCart();
            return Json(new { Count = cart?.Sum(i => i.Quantity) ?? 0 });
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var product = _context.Products.Find(id);
            if (product == null) return Json(new { Success = false, msg = "Sản phẩm không tồn tại" });

            var cart = GetCart() ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item == null)
            {
                cart.Add(new CartItem { ProductId = id, Name = product.Name, Price = product.Price, Quantity = quantity, ImageUrl = product.ImageUrl });
            }
            else
            {
                item.Quantity += quantity;
            }
            SaveCart(cart);
            return Json(new { Success = true, Count = cart.Sum(i => i.Quantity), msg = "Đã thêm vào giỏ hàng" });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var cart = GetCart();
            if (cart == null) return Json(new { Success = false });
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return Json(new { Success = true, Count = cart.Sum(i => i.Quantity) });
        }

        [HttpPost]
        public IActionResult Update(int id, int quantity)
        {
            var cart = GetCart();
            if (cart == null) return Json(new { Success = false });
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null) item.Quantity = quantity;
            SaveCart(cart);
            return Json(new { Success = true, Count = cart.Sum(i => i.Quantity) });
        }

        [HttpPost]
        public IActionResult DeleteAll()
        {
            SaveCart(new List<CartItem>());
            return Json(new { Success = true });
        }

        [HttpGet]
        public IActionResult Partial_Item_Cart()
        {
            var cart = GetCart() ?? new List<CartItem>();
            return PartialView("~/views/Shared/_CartPartial.cshtml", cart);
        }

        private List<CartItem>? GetCart()
        {
            var sessionData = HttpContext.Session.GetString(SessionCartKey);
            if (string.IsNullOrEmpty(sessionData)) return null;
            return JsonSerializer.Deserialize<List<CartItem>>(sessionData);
        }

        private void SaveCart(List<CartItem> cart)
        {
            var data = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(SessionCartKey, data);
        }
    }
}