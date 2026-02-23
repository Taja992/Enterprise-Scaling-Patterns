using ArticleService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure.Persistence;

public class CommentDbContext : DbContext
{
    public CommentDbContext(DbContextOptions<CommentDbContext> options)
        : base(options) { }

    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ArticleId).IsRequired();
            entity.Property(e => e.AuthorId).IsRequired();
            entity.Property(e => e.Body).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsFlagged).IsRequired();
        });
    }
}
