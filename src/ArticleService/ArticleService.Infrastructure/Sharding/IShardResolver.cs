using ArticleService.Infrastructure.Persistence;

namespace ArticleService.Infrastructure.Sharding;

public interface IShardResolver
{
    ArticleDbContext GetDbContext(string continent);
    string GetConnectionString(string continent);
}
