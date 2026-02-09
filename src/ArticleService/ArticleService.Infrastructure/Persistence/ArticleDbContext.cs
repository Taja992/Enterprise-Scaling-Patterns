using ArticleService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure.Persistence;

public class ArticleDbContext : DbContext
{
    public ArticleDbContext(DbContextOptions<ArticleDbContext> options)
        : base(options) { }

    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);

            entity.Property(e => e.Content).IsRequired();

            entity.Property(e => e.Continent).IsRequired().HasMaxLength(50);

            entity.Property(e => e.PublishedAt).IsRequired();

            entity.Property(e => e.PublisherId).IsRequired();
        });
    }
}
