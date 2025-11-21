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

            // Ensure DB is created (use EnsureCreated for initial dev; switch to migrations for production)
            context.Database.EnsureCreated();

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
                var c = context.Categories.First();
                context.Products.AddRange(
                    new Product { Name = "Sneakers", Slug = "sneakers", Price = 99.99m, CategoryId = c.Id, ImageUrl = "/images/product1.jpg", Stock = 50, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Product { Name = "T-Shirt", Slug = "t-shirt", Price = 29.99m, CategoryId = c.Id, ImageUrl = "/images/product2.jpg", Stock = 120, IsActive = true, CreatedAt = DateTime.UtcNow }
                );
                context.SaveChanges();
            }
        }
    }
}
