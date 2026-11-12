using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Website.Models;

namespace Website.Data
{
    public class LoginDBContext : IdentityDbContext<ApplicationUser>
    {
        public LoginDBContext(DbContextOptions<LoginDBContext> options)
            : base(options)
        {
        }

        public DbSet<DeletedRecord> DeletedRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}