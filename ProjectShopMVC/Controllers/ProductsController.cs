using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectShopMVC.Data;
using ProjectShopMVC.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace ProjectShopMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================================
        // FRONT-END: Hiển thị sản phẩm cho người dùng
        // ============================================================================
        public async Task<IActionResult> Index(int? categoryId, string? q)
        {
            IQueryable<Product> productsQuery =
                _context.Products.Include(p => p.Category).Where(p => p.IsActive);

            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                string keyword = q.ToLower();
                productsQuery = productsQuery.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword));
            }

            ViewData["Categories"] = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(await productsQuery.OrderByDescending(p => p.CreatedAt).ToListAsync());
        }

        // ============================================================================
        // ADMIN: Hiển thị trang quản lý sản phẩm
        // ============================================================================
        public async Task<IActionResult> AdminIndex()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewData["Categories"] = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(products);
        }

        // ============================================================================
        // DETAILS
        // ============================================================================
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // ============================================================================
        // CREATE (GET) → để hiển thị trang /Products/Create
        // ============================================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Categories"] = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View();
        }

        // ============================================================================
        // CREATE (POST) → AJAX JSON API
        // ============================================================================
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                return View(product);
            }

            // Xử lý upload ảnh
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

                if (!Directory.Exists(wwwRootPath))
                    Directory.CreateDirectory(wwwRootPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(wwwRootPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/products/" + fileName; // Lưu đường dẫn ảnh
            }

            product.CreatedAt = DateTime.Now;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("AdminIndex");
        }


        // ============================================================================
        // GET PRODUCT (API để mở modal Edit)
        // ============================================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["Categories"] = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(product); // Trả về view Edit.cshtml
        }

        // ============================================================================
        // EDIT (POST) 
        // ============================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? ImageFile)
        {
            if (id != product.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                return View(product);
            }

            var original = await _context.Products.FindAsync(id);
            if (original == null) return NotFound();

            // Xử lý upload ảnh
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                if (!Directory.Exists(wwwRootPath)) Directory.CreateDirectory(wwwRootPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(wwwRootPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                product.ImageUrl = "/images/products/" + fileName;
            }
            else
            {
                product.ImageUrl = original.ImageUrl; // giữ lại ảnh cũ nếu không upload mới
            }

            // Giữ CreatedAt
            product.CreatedAt = original.CreatedAt;

            _context.Entry(original).CurrentValues.SetValues(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("AdminIndex");
        }

        // ============================================================================
        // DELETE (POST) → AJAX JSON API
        // ============================================================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { Success = false, Error = "Product not found." });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Json(new { Success = true, ProductId = id });
        }

        // ============================================================================
        // SUPPORT
        // ============================================================================
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
