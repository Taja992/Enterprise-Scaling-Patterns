var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add Infrastructure layer (DbContext, Repositories, ShardResolver)
builder.Services.AddInfrastructure();

// Add Application layer (Business logic services)
builder.Services.AddScoped<IArticleAppService, ArticleAppService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Article Service API");
    });
}

app.UseHttpsRedirection();

// Map API endpoints
app.MapArticleEndpoints();

app.Run();
