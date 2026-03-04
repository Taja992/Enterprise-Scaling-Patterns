using DraftService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DraftService.Infrastructure.Persistence;

public class DraftDbContext : DbContext
{
    public DraftDbContext(DbContextOptions<DraftDbContext> options)
        : base(options) { }

    public DbSet<Draft> Drafts => Set<Draft>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Draft>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AuthorId).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Efficient retrieval by author
            entity.HasIndex(e => e.AuthorId);
        });
    }
}