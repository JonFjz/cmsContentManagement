using cmsContentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace cmsContentManagment.Infrastructure.Persistance;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public virtual DbSet<Content> Contents { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Many-to-Many for Content <-> Tag
        modelBuilder.Entity<Content>()
            .HasMany(c => c.Tags)
            .WithMany(t => t.Contents);
            
        // Configure One-to-Many for Category <-> Content
        modelBuilder.Entity<Content>()
            .HasOne(c => c.Category)
            .WithMany(cat => cat.Contents)
            .HasForeignKey(c => c.CategoryId);
    }
}
