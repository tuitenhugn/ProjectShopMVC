using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectShopMVC.Data;
using ProjectShopMVC.Models;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần thiết cho SelectList (Dropdown)
using System.Linq; // Cần thiết cho IQueryable

namespace ProjectShopMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------------------------------------
        // --- READ: Index (Dành cho người dùng/front-end) ---
        // --------------------------------------------------------------------------------
        public async Task<IActionResult> Index(int? categoryId, string? q)
        {
            // Khai báo rõ ràng là IQueryable<Product> để tránh lỗi chuyển đổi
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);

            // Filter theo trạng thái hoạt động (Dành cho người dùng)
            productsQuery = productsQuery.Where(p => p.IsActive);

            // Filter theo CategoryId
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter theo từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(q))
            {
                string searchLower = q.ToLower();
                productsQuery = productsQuery.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    p.Description.ToLower().Contains(searchLower));

                ViewData["SearchQuery"] = q;
            }

            var products = await productsQuery.OrderByDescending(p => p.CreatedAt).ToListAsync();
            ViewData["Categories"] = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(products);
        }

        // --------------------------------------------------------------------------------
        // --- READ: AdminIndex (Dành cho quản lý/CRUD View) ---
        // --------------------------------------------------------------------------------
        // Phương thức này sẽ hiển thị tất cả sản phẩm và Category cho trang quản lý
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

        // --- READ: Details ---
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null) return NotFound();
            return View(product);
        }

        // --------------------------------------------------------------------------------
        // --- CREATE (Thêm Sản Phẩm Mới) ---
        // --------------------------------------------------------------------------------

        // POST: Products/Create (API endpoint cho AJAX)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return Json(new { Success = true, Product = product }); // Trả về JSON để View cập nhật
            }
            // Trả về BadRequest kèm theo lỗi ModelState
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Success = false, Errors = errors });
        }

        // GET: Products/GetProduct/5 (API endpoint để lấy dữ liệu cho modal Edit)
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Json(product);
        }


        // --------------------------------------------------------------------------------
        // --- EDIT (Chỉnh Sửa Sản Phẩm) ---
        // --------------------------------------------------------------------------------

        // POST: Products/Edit (API endpoint cho AJAX)
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] Product product)
        {
            if (product.Id == 0) return BadRequest(new { Success = false, Error = "Product ID is missing." });

            // Lấy thông tin CreatedAt ban đầu
            var originalProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
            if (originalProduct == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Giữ lại CreatedAt
                    product.CreatedAt = originalProduct.CreatedAt;
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Json(new { Success = true, Product = product });
            }
            // Trả về BadRequest kèm theo lỗi ModelState
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Success = false, Errors = errors });
        }

        // --------------------------------------------------------------------------------
        // --- DELETE (Xóa Sản Phẩm) ---
        // --------------------------------------------------------------------------------

        // POST: Products/Delete/5 (API endpoint cho AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { Success = false, Error = "Product not found." });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Json(new { Success = true, ProductId = id });
        }


        // Hàm hỗ trợ kiểm tra tồn tại sản phẩm
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}