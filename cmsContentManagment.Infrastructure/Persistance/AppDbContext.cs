using cmsContentManagment.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace cmsContentManagment.Infrastructure.Persistance;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public virtual DbSet<Content> Contents { get; set; }

}
