using Microsoft.AspNetCore.Identity;

namespace ProjectShopMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
