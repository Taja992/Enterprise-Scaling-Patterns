FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/ArticleService/ArticleService.Api/ArticleService.Api.csproj", "src/ArticleService/ArticleService.Api/"]
COPY ["src/ArticleService/ArticleService.Application/ArticleService.Application.csproj", "src/ArticleService/ArticleService.Application/"]
COPY ["src/ArticleService/ArticleService.Domain/ArticleService.Domain.csproj", "src/ArticleService/ArticleService.Domain/"]
COPY ["src/ArticleService/ArticleService.Infrastructure/ArticleService.Infrastructure.csproj", "src/ArticleService/ArticleService.Infrastructure/"]
COPY ["src/Shared/HappyHeadlines.Observability/HappyHeadlines.Observability.csproj", "src/Shared/HappyHeadlines.Observability/"]
RUN dotnet restore "src/ArticleService/ArticleService.Api/ArticleService.Api.csproj"
COPY . .
WORKDIR "/src/src/ArticleService/ArticleService.Api"
RUN dotnet build "ArticleService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArticleService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArticleService.Api.dll"]