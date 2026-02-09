# HappyHeadlines

A scalable, microservice-based article management system built with .NET 10.0 and Clean Architecture principles. This project demonstrates enterprise-level patterns including geographical sharding, containerization, and comprehensive testing strategies.

## 🚀 Features

- **Geographical Sharding**: Articles are distributed across database shards based on continents for optimal performance and scalability
- **Clean Architecture**: Separation of concerns with distinct Domain, Application, Infrastructure, and API layers
- **Microservice Design**: Modular architecture ready for horizontal scaling
- **Containerization**: Docker support with multi-instance deployment via Docker Compose
- **Load Balancing**: Nginx reverse proxy for distributing requests across multiple API instances
- **API Documentation**: Integrated Scalar/OpenAPI for interactive API exploration
- **Comprehensive Testing**: Unit and integration tests ensuring code quality and reliability
- **Minimal APIs**: Modern .NET 10.0 minimal API endpoints for efficient request handling

## 🏗️ Architecture

The project follows Clean Architecture principles with four distinct layers:

### Domain Layer (`ArticleService.Domain`)

- Core business entities (Article, Continent enum)
- Domain logic and validation rules
- Factory methods for entity creation

### Application Layer (`ArticleService.Application`)

- Business logic services
- DTOs for data transfer
- Application interfaces

### Infrastructure Layer (`ArticleService.Infrastructure`)

- Data persistence with Entity Framework Core
- Repository implementations
- Sharding logic and database configuration
- Dependency injection setup

### API Layer (`ArticleService.Api`)

- RESTful endpoints using minimal APIs
- Swagger/OpenAPI documentation
- Request routing and middleware configuration

## 🛠️ Technologies

- **Backend**: .NET 10.0, ASP.NET Core Minimal APIs
- **Database**: PostgreSQL with Entity Framework Core
- **Containerization**: Docker, Docker Compose
- **Load Balancing**: Nginx
- **Testing**: xUnit
- **Documentation**: Scalar/OpenAPI

## 📋 Prerequisites

- .NET 10.0 SDK
- Docker and Docker Compose
- Git

## 🚀 Installation & Setup

1. **Clone the repository**

   ```bash
   git clone https://github.com/yourusername/HappyHeadlines.git
   cd HappyHeadlines
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Build the solution**

   ```bash
   dotnet build
   ```

## 🏃 Running the Application

### Local Development

```bash
cd src/ArticleService/ArticleService.Api
dotnet run
```

The API will be available at `https://localhost:7229` with Swagger UI at `https://localhost:7229/swagger`.

### Docker Deployment

```bash
docker-compose up --build
```

This starts:

- 3 API instances (ports 8081, 8082, 8083)
- Nginx load balancer (port 5000)

Access the application via `http://localhost:5000`.

## 🧪 Testing

Run unit tests:

```bash
dotnet test tests/ArticleService.UnitTests/
```

Run integration tests:

```bash
dotnet test tests/ArticleService.IntegrationTests/
```

## 📚 API Documentation

When running locally, visit `https://localhost:7229/scalar` for interactive API documentation.

### Key Endpoints

- `GET /api/articles?continent={continent}` - Retrieve articles from a specific continent *(Note: This endpoint is currently a placeholder and returns a status message. Full implementation planned.)*
- `POST /api/articles` - Create new article
- `GET /api/articles/{id}?continent={continent}` - Get article by ID
- `PUT /api/articles/{id}` - Update article
- `DELETE /api/articles/{id}` - Delete article

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 🎯 Project Highlights

- **Scalability**: Geographical sharding allows horizontal scaling across continents
- **Performance**: Multi-instance deployment with load balancing
- **Maintainability**: Clean Architecture ensures separation of concerns
- **Testability**: Comprehensive test coverage for reliability
- **Modern .NET**: Leverages latest .NET 10.0 features and minimal APIs
- **DevOps Ready**: Containerized deployment with Docker Compose

This project showcases enterprise development practices suitable for high-traffic content platforms requiring global distribution and high availability.
