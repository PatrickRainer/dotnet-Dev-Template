### Project Overview
`MyDevTemplate` is a .NET 9 solution following Clean Architecture principles, featuring a Blazor Server frontend and an ASP.NET Core Web API.

### Build/Configuration Instructions
- **SDK Requirements**: .NET 9.0 SDK is required.
- **Build**: Use `dotnet build` from the solution root.
- **Run API**: `dotnet run --project MyDevTemplate.Api`
- **Run Blazor Server**: `dotnet run --project MyDevTemplate.Blazor.Server`
- **Persistence**: `AppDbContext` is located in `MyDevTemplate.Persistence`. Currently, connection strings are not configured in `appsettings.json`. Entity Framework Core is used for data access, and configurations are applied from the assembly.

### Testing Information
- **Testing Framework**: xUnit is recommended for unit and integration testing.
- **Integration Testing**: Use the Api for integration testing, create for that for each endpoint a http file in the `MyDevTemplate.IntegrationTests` folder.
- **Running Tests**: Use `dotnet test` to execute all tests in the solution.
- **Adding Tests**:
    - Use the existing xUnit project (e.g., `MyDevTemplate.Domain.Tests`), For Domain Tests.
    - Add a reference to the project being tested: `dotnet add <TestProject> reference <ProjectUnderTest>`.
    - Create test classes following the naming convention `<Entity/Service>Tests.cs`.
- **Test Example**:
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
        }
    }
    ```

### Architecture Information
- **Clean Architecture**:
  - The different layers are visible by solution folders.
    - The 'View' Layer contains Blazor Server UI and the API Project.
    - The `Application`layer contains all Services by the application. 
    - The `Infrastructure` layer contains infrastructure services (e.g. the Persistance Project, or any Helper Libraries).
    - The `Domain` layer contains domain entities and value objects and Contracts
  - The Dependencies must be in the rigt direction. So View -> Service -> Infrastructure -> Domain.

### Additional Development Information
- **Domain-Driven Design (DDD)**: The project uses Aggregate Roots (e.g., `UserRootEntity`, `RoleRootEntity`) and Value Objects (e.g., `EmailAddress`).
- **Base Entity**: All entities should inherit from `EntityBase` which provides `Id` (Guid), `CreatedAtUtc`, and `TenantId`.
- **UI Framework**: `MudBlazor` is used for the Blazor Server UI components.
- **Code Style**:
    - Use file-scoped namespaces.
    - Follow standard .NET naming conventions (PascalCase for classes/methods, camelCase for private fields).
    - Prefer `readonly` for fields that are not intended to be modified after initialization.
    - Entities need to have a private constructor for EF Core and public constructors for domain usage.
    - Service and Persistance Projects should contain an Additional File `ServiceCollectionExtensions.cs` for registering dependencies.
    - Use `record` for value objects when possible.
    - Use `async` and `await` whenever possible.
    - Use `nameof` for string literals.
    - Domain Entities should be configured with EF Core Configuration which will be applied to the `AppDbContext`
      - The Files should be stored in the `Persistence` Project in the folder 'ModelConfigurations'
    - Use `ILogger<T>` for logging, do inject the `ILogger<T>` into the constructor where possible.
    - Use Serilog for Logging
      - Use appsettings.json for configuring Serilog
    

