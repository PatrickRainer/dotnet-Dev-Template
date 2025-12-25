### Project Overview
`MyDevTemplate` is a .NET 9 solution following Clean Architecture principles, featuring a Blazor Server frontend and an ASP.NET Core Web API.

### Build/Configuration Instructions
- **SDK Requirements**: .NET 9.0 SDK is required.
- **Build**: Use `dotnet build` from the solution root.
- **Zero Warnings**: Always aim for zero build warnings. When building the project, check for any warnings and fix them immediately to ensure code quality and prevent potential issues.
- **Run API**: `dotnet run --project MyDevTemplate.Api`
- **Run Blazor Server**: `dotnet run --project MyDevTemplate.Blazor.Server`
- **Persistence**: `AppDbContext` is located in `MyDevTemplate.Persistence`.
  - Connection strings are configured in `appsettings.Development.json` for local development.
  - Entity Framework Core is used for data access.
  - Configurations are applied from the assembly using `modelBuilder.ApplyConfigurationsFFromAssembly`.
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
        - `SubscriptionControllerTests`: Subscription plan management.
        - `RegistrationControllerTests`: New tenant onboarding.
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

### Standardized CRUD Pattern
- **Interface**: Use `ICrudService<TEntity, TId>` in the Application layer to define standard operations: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, and `DeleteAsync`.
- **Implementation**: All application services (e.g., `UserService`, `ApiKeyService`, `SubscriptionService`) must implement this interface to ensure consistency.
- **Service Registration**: Register services in the DI container as scoped.

### Multi-tenancy & Isolation
- **Domain**: All entities except `TenantRoot` and `SubscriptionRoot` must inherit from `EntityBase` to include the `TenantId` property.
- **Provider**: `ITenantProvider` is used to resolve the current `TenantId` and whether the caller is a "Master Tenant".
- **Database Enforcement**:
    - **Global Filters**: `AppDbContext` applies a query filter to all entities (except tenants and subscriptions) to restrict results to the current `TenantId`, unless `IsMasterTenant` is true.
    - **Automatic Assignment**: `AppDbContext.SaveChangesAsync` automatically assigns the current `TenantId` to new entities during creation.
- **Service Isolation**: Services naturally respect isolation via the global filters. Explicit checks (like `SingleOrDefaultAsync`) will return `null` if a tenant tries to access a resource they don't own, which should result in a `404 Not Found` at the API level.

### Feature Flagging
- **Logic**: Access to features is determined by `IFeatureService.HasFeatureAsync(featureName)`.
- **Criteria**:
    - **Master Tenant**: Always has access to all features.
    - **Subscription Plan**: Checks if the feature is included in the tenant's assigned `SubscriptionRoot`.
    - **User Roles**: Checks if any of the user's assigned roles explicitly grant the feature.
- **UI Integration**: Use the `FeatureGate` component to conditionally render content:
    ```razor
    <FeatureGate Feature="@SubscriptionFeatures.Automation">
        <ChildContent>...</ChildContent>
        <UnauthorizedContent>...</UnauthorizedContent>
    </FeatureGate>
    ```

### Registration
- **Service**: `IRegistrationService` handles the creation of new tenants and their initial setup.
- **Controller**: `RegistrationController` provides an anonymous endpoint for onboarding.
- **Admin Email**: For new registrations, the administrator's email is captured (either from the authenticated user or an `X-Admin-Email` header).

### Security & Authorization
- **Master Tenant**: Defined by the `Authentication:ApiKey` in `appsettings.json`. Master tenants have full access across all tenants.
- **Tenant Management**: `TenantService` is strictly restricted to the Master Tenant. Use the `EnsureMasterTenant()` check in all service methods.
- **API Keys**: 
    - Regular tenants can only create API keys for their own tenant.
    - Master tenants can specify a `TenantId` when creating keys to manage other tenants.
- **Authorization Policies**: Use the `MasterTenant` policy in controllers to restrict access to sensitive endpoints.

### Validation
- **FluentValidation**: Use FluentValidation for all validation needs (API DTOs, Domain Entities, UI Models).
- **Registration**: Register all validators in the Dependency Injection container using `AddValidatorsFromAssembly`.
- **Value Objects**: Create dedicated validators for Value Objects (e.g., `AddressValidator`) and use `SetValidator` to validate complex properties.
- **Shared Rules**: Centralize common validation logic (e.g., String length, patterns) using extension methods on `IRuleBuilder`.
- **Enforcement**: 
    - API: Inject `IValidator<TDto>` into controllers and validate requests early.
    - Application: Inject `IValidator<TEntity>` into services to ensure business logic operates on valid data.
- **Blazor Integration**: Use the `ValidateValue` pattern in validators to support `MudBlazor` property-level validation.

### Blazor Coding Style
- **Feature-based Organization**: Group all files related to a specific feature/page together in the `Components/Pages` folder.
- **Folder Structure**: Each page should have its own directory containing:
    - The `.razor` file (markup).
    - The `.razor.cs` file (code-behind).
    - The `Model.cs` file (UI-specific data structure).
    - The `Validator.cs` file (FluentValidation logic).
- **URL Management**: Use the `UrlProvider` class in `MyDevTemplate.Blazor.Server.Infrastructure` for all Blazor page routes and navigation Hrefs. Avoid hardcoded strings for URLs.
    - Page route definition: Use `@attribute [Route(UrlProvider.MyPage)]` instead of `@page "/my-page"`.
    - Navigation: Use `NavigationManager.NavigateTo(UrlProvider.MyPage)`.
    - NavMenu: Use `<MudNavLink Href="@UrlProvider.MyPage" ...>`.

### Additional Development Information
- **Development Approach**: 
  - Emphasize maintainability, testability, and scalability.
  - Use TDD approach for new features, so start always with a failing Integration and/or Domain test.
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
    - Don't use redundant modifiers (e.g., `private` on constructors, properties and methods when they are already private by default).
    

