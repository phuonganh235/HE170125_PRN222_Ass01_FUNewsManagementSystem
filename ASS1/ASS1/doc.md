# ASS1 - News Management System Documentation

## Project Overview

ASS1 is a .NET 8.0 ASP.NET Core MVC web application designed for news management. The project follows a clean architecture pattern with separate layers for Data Access (DAL), Business Logic (BLL), and Presentation (MVC).

## Solution Structure

The solution consists of 4 projects:

1. **ASS1** - Main MVC Web Application
2. **ASS1.DAL** - Data Access Layer
3. **ASS1.BLL** - Business Logic Layer  
4. **MyWebApp.BLL** - Additional Business Logic Layer (currently empty)

## Architecture

### 1. Data Access Layer (ASS1.DAL)

#### Models
The system manages the following entities:

- **Category** - News categories with hierarchical structure
- **NewsArticle** - News articles with content and metadata
- **SystemAccount** - User accounts for system access
- **Tag** - Tags for categorizing news articles

#### Database Context
- **AppDbContext** - Entity Framework Core context
- Database: SQL Server (FUNewsManagement)
- Connection: Local SQL Server with trusted connection

#### Repository Pattern
- **IGenericRepository<T>** - Generic repository interface
- **GenericRepository<T>** - Generic repository implementation
- Supports CRUD operations, pagination, filtering, and search

### 2. Business Logic Layer (ASS1.BLL)

#### Service Pattern
- **IGenericService<T>** - Generic service interface
- **GenericService<T>** - Generic service implementation
- Provides business logic abstraction over repository layer

### 3. Presentation Layer (ASS1)

#### Controllers
- **HomeController** - Basic MVC controller with Index, Privacy, and Error actions

#### Views
- Razor views with Bootstrap styling
- Layout: `_Layout.cshtml` with responsive navigation
- Pages: Home/Index, Home/Privacy, Error handling

#### Configuration
- **Program.cs** - Application startup and service configuration
- **appsettings.json** - Configuration settings including connection strings
- **launchSettings.json** - Development launch profiles

## Database Schema

### Category Table
```sql
- CategoryID (short, Primary Key)
- CategoryName (string, 100 chars)
- CategoryDesciption (string, 250 chars)
- ParentCategoryID (short, Foreign Key - Self-referencing)
- IsActive (bool) - Status: active(1)/inactive(0)
```

### NewsArticle Table
```sql
- NewsArticleID (string, 20 chars, Primary Key)
- NewsTitle (string, 400 chars)
- Headline (string, 150 chars)
- CreatedDate (datetime)
- NewsContent (string, 4000 chars)
- NewsSource (string, 400 chars)
- CategoryID (short, Foreign Key) - One-to-Many with Category
- NewsStatus (bool) - Status: active(1)/inactive(0)
- CreatedByID (short, Foreign Key) - References SystemAccount
- UpdatedByID (short, Foreign Key) - References SystemAccount
- ModifiedDate (datetime)
```

### SystemAccount Table
```sql
- AccountID (short, Primary Key)
- AccountName (string, 100 chars)
- AccountEmail (string, 70 chars)
- AccountRole (int) - Staff(1), Lecturer(2), Admin(from appsettings.json)
- AccountPassword (string, 70 chars)
```

### Tag Table
```sql
- TagID (int, Primary Key)
- TagName (string, 50 chars)
- Note (string, 400 chars)
```

### NewsTag Junction Table
```sql
- NewsArticleID (string, Foreign Key)
- TagID (int, Foreign Key)
- Composite Primary Key
```

## Business Rules

### Entity Relationships
1. **News Article ↔ Category**: One-to-Many relationship
   - A news article belongs to only one category
   - A category can have many news articles

2. **System Account ↔ News Article**: One-to-Many relationship
   - An account with staff role (role = 1) can create many news articles
   - Lecturer role = 2
   - Admin role is configured in appsettings.json

3. **News Article ↔ Tag**: Many-to-Many relationship
   - A news article can have many tags
   - A tag can belong to zero or many news articles

### Status Values
- **News Status**: 
  - `1` = Active
  - `0` = Inactive

- **Category Status**:
  - `1` = Active  
  - `0` = Inactive

### Role Definitions
- **Staff Role**: `1` - Can create news articles
- **Lecturer Role**: `2` - Limited permissions
- **Admin Role**: Configured in appsettings.json - Full system access
  - Default Admin Account: `admin@FUNewsManagementSystem.org` / `@@abc123@@`

## Key Features

### Generic Repository Pattern
- Full CRUD operations
- Asynchronous operations with cancellation token support
- Pagination with filtering and search capabilities
- Predicate-based querying
- Bulk operations support

### Generic Service Pattern
- Business logic abstraction
- Error handling and validation
- Service layer over repository pattern

### Entity Framework Core Integration
- Code-first approach
- SQL Server provider
- Relationship mapping with foreign keys
- Cascade delete configurations

## Configuration

### Connection String
```json
"ConnectionStrings": {
  "MyCnn": "Server=localhost;database=FUNewsManagement;user=sa;password=123;TrustServerCertificate=true"
}
```

### Default Admin Account
```json
"DefaultAdmin": {
  "Email": "admin@FUNewsManagementSystem.org",
  "Password": "@@abc123@@"
}
```

### Dependency Injection
- DbContext registration
- Generic repository and service registration
- Scoped lifetime for data access components

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- SQL Server (Local or Express)
- Visual Studio 2022 or VS Code

### Running the Application
1. Restore NuGet packages
2. Update connection string if needed
3. Run database migrations (if applicable)
4. Start the application

### Launch Profiles
- **HTTP**: http://localhost:5007
- **HTTPS**: https://localhost:7230
- **IIS Express**: http://localhost:43855

## Project Dependencies

### ASS1.DAL
- Microsoft.EntityFrameworkCore.Design (8.0.20)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.20)
- Microsoft.EntityFrameworkCore.Tools (8.0.20)

### ASS1.BLL
- No external dependencies (references ASS1.DAL)

### ASS1 (Main Project)
- References to ASS1.DAL and ASS1.BLL
- ASP.NET Core MVC framework

## Code Quality Features

### Error Handling
- Global exception handling
- Custom error view
- Request ID tracking for debugging

### Security
- HTTPS redirection
- HSTS configuration for production
- Input validation through data annotations

### Performance
- AsNoTracking() for read-only operations
- Pagination support
- Efficient query building with LINQ expressions

## Future Enhancements

### Potential Improvements
1. Authentication and Authorization system
2. API controllers for mobile/web clients
3. Caching layer implementation
4. Logging framework integration
5. Unit and integration tests
6. API documentation with Swagger
7. File upload for news images
8. Advanced search functionality
9. News approval workflow
10. User role management

### Missing Components
- Authentication/Authorization
- Specific business services for news management
- DTOs for data transfer
- Validation attributes
- API endpoints
- Test projects

## File Structure

```
ASS1/
├── Controllers/
│   └── HomeController.cs
├── Models/
│   └── ErrorViewModel.cs
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       └── Error.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── Program.cs
├── appsettings.json
└── Properties/
    └── launchSettings.json

ASS1.DAL/
├── Models/
│   ├── AppDbContext.cs
│   ├── Category.cs
│   ├── NewsArticle.cs
│   ├── SystemAccount.cs
│   └── Tag.cs
└── Repository/
    ├── IGenericRepository.cs
    └── GenericRepository.cs

ASS1.BLL/
└── Service/
    ├── IGenericService.cs
    └── GenericService.cs
```

## Conclusion

This is a well-structured .NET 8.0 application following clean architecture principles. The generic repository and service patterns provide a solid foundation for CRUD operations, while the Entity Framework Core integration ensures efficient data access. The project is ready for further development with additional business logic, authentication, and advanced features.
