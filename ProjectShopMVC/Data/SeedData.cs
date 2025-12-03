using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectShopMVC.Models;

namespace ProjectShopMVC.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationDbContext>();

            // NOTE: Do not call EnsureCreated when using Migrations. Program.cs already calls db.Database.Migrate().
            // context.Database.EnsureCreated();

            var userManager = services.GetService<UserManager<ApplicationUser>>();
            var roleManager = services.GetService<RoleManager<IdentityRole>>();

            // Create admin role
            var adminRoleName = "Admin";
            if (roleManager != null && !roleManager.RoleExistsAsync(adminRoleName).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(adminRoleName)).GetAwaiter().GetResult();
            }

            // Create admin user
            var adminEmail = "admin@local.com";
            if (userManager != null && userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult() == null)
            {
                var adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, FullName = "Administrator" };
                var result = userManager.CreateAsync(adminUser, "Admin@123").GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(adminUser, adminRoleName).GetAwaiter().GetResult();
                }
            }

            // Seed categories and products
            if (!context.Categories.Any())
            {
                var shoes = new Category { Name = "Shoes", Slug = "shoes", IsActive = true, CreatedAt = DateTime.UtcNow };
                var clothing = new Category { Name = "Clothing", Slug = "clothing", IsActive = true, CreatedAt = DateTime.UtcNow };
                context.Categories.AddRange(shoes, clothing);
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                var shoesCat = context.Categories.First(c => c.Slug == "shoes");
                var clothCat = context.Categories.First(c => c.Slug == "clothing");

                //// 6 T-Shirts
                //context.Products.AddRange(
                //    new Product { Name = "Áo Thun Essentials Boyfriend", Slug = "ao-thun-essentials-boyfriend", Price = 800000m, CategoryId = clothCat.Id, ImageUrl = "/images/tshirt1.jpg", Stock = 50, IsActive = true, IsNew = true, Rating = 4.5, Views = 120, Colors = "White,Black,Green", Sizes = "S,M,L,XL", DetailedDescription = "Áo thun cotton m?m m?i, form r?ng tho?i mái. Phù h?p m?c hàng ngày. Ch?t li?u 100% cotton, gi?t máy ???c.", Description = "Áo Thun Essentials Boyfriend" },
                //    new Product { Name = "Áo Thun 3 S?c", Slug = "ao-thun-3-soc", Price = 950000m, CategoryId = clothCat.Id, ImageUrl = "/images/tshirt2.jpg", Stock = 80, IsActive = true, Rating = 4.6, Views = 340, Colors = "Blue,White,Black", Sizes = "S,M,L,XL", DetailedDescription = "Áo thun th? thao v?i thi?t k? 3 s?c bi?u t??ng. V?i co giãn, thoáng mát, phù h?p cho t?p luy?n và d?o ph?.", Description = "ÁO THUN 3 S?C" },
                //    new Product { Name = "Áo ??u Bóng ?á Dài Tay", Slug = "ao-dau-bong-da-dai-tay", Price = 2400000m, CategoryId = clothCat.Id, ImageUrl = "/images/tshirt3.jpg", Stock = 30, IsActive = true, Rating = 4.2, Views = 210, Colors = "Black", Sizes = "M,L,XL", DetailedDescription = "Áo ??u chuyên d?ng v?i ch?t li?u thoáng khí, công ngh? hút ?m t?t, phù h?p thi ??u.", Description = "ÁO ??U BÓNG ?Á DÀI TAY" },
                //    new Product { Name = "Knit TT", Slug = "knit-tt", Price = 2300000m, CategoryId = clothCat.Id, ImageUrl = "/images/tshirt4.jpg", Stock = 20, IsActive = true, Rating = 4.7, Views = 410, Colors = "Grey,White", Sizes = "S,M,L", DetailedDescription = "Áo khoác nh? knit, phong cách retro, phù h?p cho nh?ng ngày se l?nh.", Description = "Knit TT" },
                //    new Product { Name = "Áo Track Top Firebird", Slug = "ao-track-top-firebird", Price = 2000000m, CategoryId = clothCat.Id, ImageUrl = "/images/tshirt5.jpg", Stock = 25, IsActive = true, Rating = 4.8, Views = 520, Colors = "Dark Blue,Red", Sizes = "S,M,L,XL", DetailedDescription = "Track top có các s?c n?i b?t ? tay, thi?t k? ôm v?a ph?i, ch?t li?u cao c?p, thoáng khí.", Description = "ÁO TRACK TOP FIREBIRD" },
                //    new Product { Name = "Áo Thun 3 S?c Tr?ng", Slug = "ao-thun-3-soc-trang", Price = 950000m, CategoryId = clothCat.Id, ImageUrl = "/images/tshirt6.jpg", Stock = 60, IsActive = true, Rating = 4.4, Views = 180, Colors = "White", Sizes = "S,M,L,XL", DetailedDescription = "Phiên b?n màu tr?ng c? ?i?n c?a áo thun 3 s?c, d? ph?i ??.", Description = "Áo Thun 3 S?c Tr?ng" }
                //);

                // 6 Shoes
                //context.Products.AddRange(
                //    new Product { Name = "Giày Samba OG", Slug = "giay-samba-og", Price = 2700000m, CategoryId = shoesCat.Id, ImageUrl = "/images/shoe1.jpg", Stock = 40, IsActive = true, Rating = 4.6, Views = 600, Colors = "White,Black", Sizes = "40,41,42,43", DetailedDescription = "Giày Samba OG c? ?i?n v?i ?? cao su, form ôm chân, phù h?p cho phong cách ???ng ph?.", Description = "Giày Samba OG" },
                //    new Product { Name = "Giày Samba Jane", Slug = "giay-samba-jane", Price = 2400000m, CategoryId = shoesCat.Id, ImageUrl = "/images/shoe2.jpg", Stock = 30, IsActive = true, Rating = 4.3, Views = 300, Colors = "White", Sizes = "36,37,38,39", DetailedDescription = "Phiên b?n Samba dành cho n? v?i quai dán ti?n l?i. ?? êm, nh? nhàng.", Description = "Giày Samba Jane" },
                //    new Product { Name = "Giày Samba Lt", Slug = "giay-samba-lt", Price = 2900000m, CategoryId = shoesCat.Id, ImageUrl = "/images/shoe3.jpg", Stock = 35, IsActive = true, Rating = 4.5, Views = 250, Colors = "Brown,White", Sizes = "40,41,42,43", DetailedDescription = "Samba phiên b?n da cao c?p, phù h?p cho phong cách smart-casual.", Description = "Giày Samba Lt" },
                //    new Product { Name = "Giày Tr? Em Samba Disney", Slug = "giay-tre-em-samba-disney", Price = 1700000m, CategoryId = shoesCat.Id, ImageUrl = "/images/shoe4.jpg", Stock = 20, IsActive = true, Rating = 4.2, Views = 120, Colors = "Multi", Sizes = "28,29,30,31", DetailedDescription = "Phiên b?n h?p tác Disney, màu s?c t??i sáng, ?? êm cho bé.", Description = "Giày Tr? Em adidas Disney Samba 360" },
                //    new Product { Name = "Giày Samba OG Retro", Slug = "giay-samba-og-retro", Price = 2700000m, CategoryId = shoesCat.Id, ImageUrl = "/images/shoe5.jpg", Stock = 50, IsActive = true, Rating = 4.7, Views = 480, Colors = "Green,Brown", Sizes = "40,41,42,43", DetailedDescription = "Phiên b?n retro v?i tông màu ??c ?áo, phù h?p s?u t?m.", Description = "Giày Samba OG Retro" },
                //    new Product { Name = "Giày Samba Pro", Slug = "giay-samba-pro", Price = 3000000m, CategoryId = shoesCat.Id, ImageUrl = "/images/shoe6.jpg", Stock = 30, IsActive = true, Rating = 4.6, Views = 400, Colors = "Black,White", Sizes = "40,41,42,43", DetailedDescription = "Phiên b?n hi?u su?t v?i lòng giày h? tr? t?t khi di chuy?n nhi?u.", Description = "Giày Samba Pro" }
                //);

                context.SaveChanges();
            }
        }
    }
}
