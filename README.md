# MyDevTemplate

`MyDevTemplate` is a comprehensive .NET 9 solution template following Clean Architecture principles. It features a Blazor Server frontend for the UI and an ASP.NET Core Web API for backend services.

## Overview

This project is designed as a starting point for building scalable and maintainable applications. It leverages the latest .NET 9 features and incorporates Domain-Driven Design (DDD) patterns like Aggregate Roots and Value Objects.

### Technology Stack

- **Framework**: .NET 9.0
- **Frontend**: Blazor Server with [MudBlazor](https://mudblazor.com/)
- **API**: ASP.NET Core Web API
- **Persistence**: Entity Framework Core
- **Logging**: Serilog
- **Testing**: xUnit

## Project Structure

The solution follows Clean Architecture, organized into the following layers:

- **View Layer**:
    - `MyDevTemplate.Blazor.Server`: The frontend application using Blazor Server and MudBlazor.
    - `MyDevTemplate.Api`: The backend RESTful API.
- **Application Layer**:
    - `MyDevTemplate.Application`: Contains application services and business logic.
- **Infrastructure Layer**:
    - `MyDevTemplate.Persistence`: Handles data access and EF Core configurations.
- **Domain Layer**:
    - `MyDevTemplate.Domain`: Contains domain entities, aggregate roots, value objects, and contracts.
- **Test Projects**:
    - `MyDevTemplate.Domain.Tests`: Unit tests for the domain layer.
    - `MyDevTemplate.Api.IntegrationTests`: Integration tests for the API layer using `WebApplicationFactory`.

## Requirements

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Getting Started

### 1. Clone the repository
```bash
git clone <repository-url>
cd MyDevTemplate
```

### 2. Build the solution
```bash
dotnet build
```

### 3. Configure the application
- Connection strings are currently not configured in `appsettings.json`.
- **TODO**: Update `appsettings.json` in `MyDevTemplate.Api` and `MyDevTemplate.Blazor.Server` with your database connection string.

### 4. Run the applications

#### Run the Web API:
```bash
dotnet run --project MyDevTemplate.Api
```

#### Run the Blazor Server UI:
```bash
dotnet run --project MyDevTemplate.Blazor.Server
```

## Scripts

Currently, there are no custom scripts. All operations are performed using standard `dotnet` CLI commands.

## Environment Variables

The following configuration settings are expected in `appsettings.json` or as environment variables:

| Name | Description | Default |
|------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | **TODO**: Database connection string | `null` |
| `Serilog` | Logging configuration | (Defined in `appsettings.json`) |

## Testing

Use the `dotnet test` command to run all tests in the solution.

```bash
dotnet test
```

### Unit Tests
- Located in `MyDevTemplate.Domain.Tests`.
- Focus on domain logic, entities, and value objects.

### Integration Tests
- Located in `MyDevTemplate.Api.IntegrationTests`.
- Use `WebApplicationFactory` to test the API in-memory.
- Configuration is managed via `appsettings.json` in the test project.
- **Note**: `Program.cs` in the API project must have `public partial class Program { }` to be accessible by the test project.

### Adding New Tests
- Use the existing xUnit project `MyDevTemplate.Domain.Tests` for Domain tests.
- Use `MyDevTemplate.Api.IntegrationTests` for API integration tests.
- Follow the naming convention `<Entity/Service>Tests.cs` or `<Controller>Tests.cs`.
- Example Unit Test:
    ```csharp
    public class RoleRootEntityTests
    {
        [Fact]
        public void RoleRootEntity_Should_Initialize_Correctly()
        {
            // Arrange & Act
            var role = new RoleRootEntity("Admin", "Description");
            // Assert
            Assert.Equal("Admin", role.Title);
        }
    }
    ```

## Development Guidelines

- **Base Entity**: All entities should inherit from `EntityBase` (provides `Id`, `CreatedAtUtc`, `TenantId`).
- **DDD**: Use Aggregate Roots and Value Objects.
- **Code Style**:
    - Use file-scoped namespaces.
    - Use `async`/`await` for asynchronous operations.
    - Entities should have a private constructor for EF Core.
    - Register dependencies in `ServiceCollectionExtensions.cs` within Service and Persistence projects.

## License

**TODO**: Specify the license for this project (e.g., MIT, Apache 2.0).
