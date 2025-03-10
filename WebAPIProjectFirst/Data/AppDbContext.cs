using Microsoft.EntityFrameworkCore;
using WebAPIProjectFirst.Models;

namespace WebAPIProjectFirst.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserToken>()
                .HasKey(ut => ut.UserId);  // Define UserId as the primary key
        }
    }
}
