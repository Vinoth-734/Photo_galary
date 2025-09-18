using Microsoft.EntityFrameworkCore;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<Photo> Photos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Username)
                .IsUnique(false);

            modelBuilder.Entity<Photo>()
                .Property(p => p.UploadedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
