### Project Overview
`MyDevTemplate` is a .NET 9 solution following Clean Architecture principles, featuring a Blazor Server frontend and an ASP.NET Core Web API.

### Build/Configuration Instructions
- **SDK Requirements**: .NET 9.0 SDK is required.
- **Build**: Use `dotnet build` from the solution root.
- **Run API**: `dotnet run --project MyDevTemplate.Api`
- **Run Blazor Server**: `dotnet run --project MyDevTemplate.Blazor.Server`
- **Persistence**: `AppDbContext` is located in `MyDevTemplate.Persistence`.
  - Connection strings are configured in `appsettings.Development.json` for local development.
  - Entity Framework Core is used for data access.
  - Configurations are applied from the assembly using `modelBuilder.ApplyConfigurationsFromAssembly`.
  - Default database provider is SQL Server.

### Testing Information
- **Testing Framework**: xUnit is used for unit and integration testing.
- **Unit Testing**:
    - Located in `01_Core` solution folder (e.g., `MyDevTemplate.Domain.Tests`).
    - Run using `dotnet test`.
- **Integration Testing (C# - Recommended)**:
    - Located in `MyDevTemplate.Api.IntegrationTests`.
    - Uses `Microsoft.AspNetCore.Mvc.Testing` and `WebApplicationFactory<Program>`.
    - Base class `IntegrationTestBase.cs` provides common logic:
        - Configuration (API Key, TenantId) loading from `appsettings.json`.
        - `HttpClient` initialization with necessary headers.
        - `[Collection("IntegrationTests")]` for sequential execution.
    - Test Coverage:
        - `UserControllerTests`: User CRUD operations.
        - `RoleControllerTests`: Role CRUD operations.
        - `ApiKeyControllerTests`: API Key lifecycle.
        - `TenantControllerTests`: Tenant management (requires Master API Key).
        - `AuthenticationTests`: Missing/invalid keys, tenant ID validation, and policy checks.
    - Run using `dotnet test`.
    - To allow testing, `Program.cs` in `MyDevTemplate.Api` must have `public partial class Program { }`.
- **Integration Testing (HTTP Files - Manual/Lightweight Debugging)**:
    - API endpoints can also be manually executed using `.http` files located in `MyDevTemplate.Api/IntegrationTests`.
    - These files are primarily for manual debugging and do not contain automated tests.
    - These files can be executed directly in IDEs like JetBrains Rider or VS Code with the REST Client extension.
- **Adding Tests**:
    - Use the existing xUnit projects (e.g., `MyDevTemplate.Domain.Tests` or `MyDevTemplate.Api.IntegrationTests`).
    - Add a reference to the project being tested: `dotnet add <TestProject> reference <ProjectUnderTest>`.
    - Create test classes following the naming convention `<Entity/Service>Tests.cs` or `<Controller>Tests.cs`.
- **Unit Test Example**:
    ```csharp
    using MyDevTemplate.Domain.Entities.RoleAggregate;
    using Xunit;

    namespace MyDevTemplate.Domain.Tests;

    public class RoleRootEntityTests
    {
        [Fact]
        public void RoleRootEntity_Should_Initialize_Correctly()
        {
            // Arrange
            var title = "Admin";
            var description = "Administrator role";

            // Act
            var role = new RoleRootEntity(title, description);

            // Assert
            Assert.Equal(title, role.Title);
            Assert.Equal(description, role.Description);
        }
    }
    ```
- **Integration Test Example**:
    ```csharp
    public class UserControllerTests : IntegrationTestBase
    {
        public UserControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }
        // [Fact] ...
    }
    ```

### Architecture Information
- **Clean Architecture**:
  - The solution is organized into numbered folders:
    - `01_Core`: Domain entities, value objects, contracts, and their tests.
    - `02_Infrastructure`: Persistence (EF Core), external service implementations.
    - `03_ApplicationLayer`: Application services and business logic.
    - `04_ViewLayer`: Blazor Server UI and ASP.NET Core Web API.
  - **Dependency Direction**: View -> Application -> Infrastructure -> Domain.

### Additional Development Information
- **Domain-Driven Design (DDD)**: The project uses Aggregate Roots (e.g., `UserRootEntity`, `RoleRootEntity`) and Value Objects (e.g., `EmailAddress`).
- **Base Entity**: All entities should inherit from `EntityBase` which provides `Id` (Guid), `CreatedAtUtc`, and `TenantId`.
- **UI Framework**: `MudBlazor` is used for the Blazor Server UI components.
- **Authentication**: API uses API Key authentication.
  - Header: `X-Api-Key`
  - Configuration: `Authentication:ApiKey` in `appsettings.json`.
- **API Versioning**: Uses `Asp.Versioning.Http`. Versioning is applied to controllers and actions (e.g., `/api/v1/Resource`).
- **Logging**: Serilog is used for logging, configured in `appsettings.json`. It supports Console and SQL Server sinks.
- **Code Style**:
    - Use file-scoped namespaces.
    - Follow standard .NET naming conventions (PascalCase for classes/methods, camelCase for private fields).
    - Entities must have a private constructor for EF Core and public constructors for domain usage.
    - Use `ServiceCollectionExtensions.cs` in each layer for registering dependencies.
    - Domain Entities should have EF Core Configuration files in `MyDevTemplate.Persistence/ModelConfigurations`.
    - Use `ILogger<T>` for logging, injected via constructor.
    

