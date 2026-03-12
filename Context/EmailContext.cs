using Project2_EmailNight.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
 


namespace Project2_EmailNight.Context
{
    public class EmailContext:IdentityDbContext<AppUser>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;initial catalog=2ProjectEmailNight;integrated security=true;TrustServerCertificate=True;");
        }
        public DbSet<Message> Messages { get; set; }

        public DbSet<Category> Categories { get; set; }
    }
}
