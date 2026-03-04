using Microsoft.EntityFrameworkCore;
using ProfanityService.Domain.Entities;

namespace ProfanityService.Infrastructure.Persistence;

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
            entity.HasIndex(e => e.Word).IsUnique();
        });
    }
}
