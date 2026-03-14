using ArticleService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ArticleService.Infrastructure.Sharding;

public class ShardResolver : IShardResolver
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, ArticleDbContext> _dbContexts;

    public ShardResolver(IConfiguration configuration)
    {
        _configuration = configuration;
        _dbContexts = new Dictionary<string, ArticleDbContext>();
        InitializeDbContexts();
    }

    private void InitializeDbContexts()
    {
        var continents = new[]
        {
            "Africa",
            "Antarctica",
            "Asia",
            "Europe",
            "NorthAmerica",
            "Oceania",
            "SouthAmerica",
            "Global",
        };

        foreach (var continent in continents)
        {
            var connectionString = GetConnectionString(continent);
            var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            var context = new ArticleDbContext(optionsBuilder.Options);
            context.Database.EnsureCreated();
            _dbContexts[continent] = context;
        }
    }

    public ArticleDbContext GetDbContext(string continent)
    {
        if (!_dbContexts.ContainsKey(continent))
        {
            throw new ArgumentException($"Invalid continent: {continent}");
        }

        return _dbContexts[continent];
    }

    public string GetConnectionString(string continent)
    {
        var connectionString = _configuration[$"DatabaseShards:{continent}"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException($"No connection string found for continent: {continent}");
        }

        return connectionString;
    }
}
