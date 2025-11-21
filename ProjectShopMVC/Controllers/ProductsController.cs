using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectShopMVC.Data;
using ProjectShopMVC.Models;

namespace ProjectShopMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? q)
        {
            var productsQuery = _context.Products.Include(p => p.Category).Where(p => p.IsActive);
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }
            if (!string.IsNullOrWhiteSpace(q))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(q) || p.Description.Contains(q));
                ViewData["SearchQuery"] = q;
            }

            var products = await productsQuery.OrderByDescending(p => p.CreatedAt).ToListAsync();
            ViewData["Categories"] = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
