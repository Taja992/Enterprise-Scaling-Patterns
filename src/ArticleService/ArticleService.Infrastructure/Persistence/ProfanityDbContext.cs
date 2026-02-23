using ArticleService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure.Persistence;

public class ProfanityDbContext : DbContext
{
    public ProfanityDbContext(DbContextOptions<ProfanityDbContext> options)
        : base(options) { }

    public DbSet<ProfaneWord> ProfaneWords => Set<ProfaneWord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProfaneWord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Word).IsRequired().HasMaxLength(100);
        });
    }
}
