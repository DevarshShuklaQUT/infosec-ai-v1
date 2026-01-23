# InfoSec Application - Multi-Tenant .NET Core MVC with Identity

A secure multi-tenant web application built with .NET Core MVC and Web API, featuring ASP.NET Core Identity authentication and API-key based authorization.

## Architecture

This solution consists of two main projects:

1. **InfoSecApp.Web** - MVC Frontend with ASP.NET Core Identity
2. **InfoSecApp.Api** - Web API with API-Key Authentication

## Features

### Authentication & Authorization
- ASP.NET Core Identity for user authentication
- Role-based authorization (SuperAdmin, Admin, User)
- API-key authentication for API endpoints
- Multi-tenant support with tenant isolation

### Multi-Tenancy
- Support for multiple tenants with separate data isolation
- Users can belong to specific tenants or use the application privately
- Tenant management via SuperAdmin API

### SuperAdmin Functionality
- Tenant management (Create, Read, Update, Delete)
- User management
- Role assignment

## Getting Started

### Prerequisites
- .NET 10.0 SDK or later
- Docker and Docker Compose (for SQL Server)

### Installation

1. Clone the repository
2. Navigate to the solution directory

### Database Setup

#### Start SQL Server (Docker)
```bash
docker compose up -d
```

This starts SQL Server 2022 in Docker. Wait about 30 seconds for it to be ready.

#### Apply Migrations
```bash
cd InfoSecApp.Web
dotnet ef database update
```

**Note:** See [DOCKER_SETUP.md](DOCKER_SETUP.md) for detailed Docker and database management instructions.

### Running the Applications

#### Run Web Application (MVC Frontend)
```bash
cd InfoSecApp.Web
dotnet run --urls "http://localhost:5000"
```
The web application will be available at: http://localhost:5000

#### Run API Application
```bash
cd InfoSecApp.Api
dotnet run --urls "http://localhost:7000"
```
The API will be available at: http://localhost:7000

**Note:** For production deployments, configure HTTPS with proper SSL certificates.

### Default Credentials

**SuperAdmin Account:**
- Email: superadmin@infosecapp.com
- Password: SuperAdmin@123

## API Authentication

The API uses header-based API-key authentication. Include the API key in the request header:

```
X-API-Key: test-api-key-12345
```

### Valid API Keys (configured in appsettings.json):
- `test-api-key-12345` (Development)
- `prod-api-key-67890` (Production)

## API Endpoints

### Web Application SuperAdmin API
Base URL: `http://localhost:5000/api/superadmin`

**Tenant Management:**
- `GET /api/superadmin/tenants` - List all tenants
- `GET /api/superadmin/tenants/{id}` - Get tenant by ID
- `POST /api/superadmin/tenants` - Create new tenant
- `PUT /api/superadmin/tenants/{id}` - Update tenant
- `DELETE /api/superadmin/tenants/{id}` - Delete tenant

**User Management:**
- `GET /api/superadmin/users` - List all users
- `POST /api/superadmin/users/{userId}/assign-role` - Assign role to user

### API Application
Base URL: `http://localhost:7000/api`

**Data Management:**
- `GET /api/data` - Get all data items
- `GET /api/data/{id}` - Get data item by ID
- `POST /api/data` - Create new data item

**Health Check:**
- `GET /health` - API health status

## Configuration

### Web Application (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=InfoSecAppDb;User Id=sa;Password=InfoSec@Pass123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Database:** SQL Server 2022 running in Docker (see [DOCKER_SETUP.md](DOCKER_SETUP.md))

### API Application (appsettings.json)
```json
{
  "HashedApiKeys": [
    "test-api-key-12345",
    "prod-api-key-67890"
  ]
}
```

## Security

- Passwords must be at least 6 characters
- Passwords must contain uppercase and lowercase letters
- Passwords must contain at least one digit
- API keys should be stored securely and rotated regularly
- Use HTTPS in production environments

## Multi-Tenant Usage

### Creating a Tenant
```bash
curl -X POST http://localhost:5000/api/superadmin/tenants \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Acme Corporation",
    "identifier": "acme",
    "connectionString": null
  }'
```

### Assigning User to Tenant
Users can be assigned to tenants during registration or via database updates. The TenantId field in ApplicationUser determines tenant membership.

## Development

### Building the Solution
```bash
dotnet build InfoSecApp.sln
```

### Running Tests
```bash
dotnet test
```

## Project Structure

```
InfoSecApp.sln
├── InfoSecApp.Web/
│   ├── Controllers/
│   │   └── SuperAdminController.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── DbInitializer.cs
│   ├── Models/
│   │   ├── ApplicationUser.cs
│   │   └── Tenant.cs
│   └── Program.cs
└── InfoSecApp.Api/
    ├── Controllers/
    │   └── DataController.cs
    ├── Middleware/
    │   └── ApiKeyAuthMiddleware.cs
    ├── Models/
    │   └── ApiKey.cs
    └── Program.cs
```

## License

This project is licensed under the MIT License.
