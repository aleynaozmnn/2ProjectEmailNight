using Microsoft.AspNetCore.Identity;

namespace Project2_EmailNight.Entities
{
    public class AppUser:IdentityUser
    {
        public String Name { get; set; }
        public String Surname { get; set; }
        public String? ImageUrl { get; set; }
        public String? About { get; set; }
        public string? ConfirmCode { get; set; }
        public string? Description { get; set; } // Hakkımda bilgisi burada duracak
        public string? City { get; set; }
    }
}
