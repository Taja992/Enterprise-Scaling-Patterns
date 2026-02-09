FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/ArticleService/ArticleService.Api/ArticleService.Api.csproj", "src/ArticleService/ArticleService.Api/"]
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