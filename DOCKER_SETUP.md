# Docker Setup Guide for InfoSec Application

## Overview
This guide explains how to set up and use SQL Server in Docker for development.

## Prerequisites
- Docker and Docker Compose installed
- .NET 10.0 SDK

## SQL Server Configuration

### Docker Compose File
The `docker-compose.yml` file at the root of the repository configures SQL Server 2022 Developer Edition.

**Configured Settings:**
- **Image:** mcr.microsoft.com/mssql/server:2022-latest
- **Container Name:** infosec-sqlserver
- **Port:** 1433 (exposed to host)
- **SA Password:** InfoSec@Pass123
- **Database Name:** InfoSecAppDb (created automatically by EF Core migrations)

### Starting SQL Server

From the repository root:

```bash
docker compose up -d
```

This will:
1. Pull the SQL Server 2022 image (if not already downloaded)
2. Create a network (`infosec-network`)
3. Create a volume for persistent data (`sqlserver_data`)
4. Start the SQL Server container
5. Run health checks to ensure SQL Server is ready

### Checking SQL Server Status

```bash
# Check if container is running
docker ps | grep sqlserver

# Check container logs
docker logs infosec-sqlserver

# Check health status
docker inspect infosec-sqlserver | grep -A 5 Health
```

### Stopping SQL Server

```bash
docker compose down
```

This stops and removes the container but **preserves the data volume**.

### Removing All Data

To completely remove SQL Server including all databases:

```bash
docker compose down -v
```

**Warning:** This deletes all data permanently!

## Connection String

The application uses this connection string (in `appsettings.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=InfoSecAppDb;User Id=sa;Password=InfoSec@Pass123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Connection String Components:
- **Server:** localhost,1433 (SQL Server hostname and port)
- **Database:** InfoSecAppDb (database name)
- **User Id:** sa (System Administrator account)
- **Password:** InfoSec@Pass123 (SA password)
- **TrustServerCertificate:** True (accepts self-signed certificate)
- **MultipleActiveResultSets:** True (allows multiple active result sets)

## Database Migrations

### Creating Migrations

When you modify models:

```bash
cd InfoSecApp.Web
dotnet ef migrations add MigrationName
```

### Applying Migrations

```bash
cd InfoSecApp.Web
dotnet ef database update
```

The first time you run this, it will:
1. Create the `InfoSecAppDb` database
2. Apply all migrations
3. Seed initial data (SuperAdmin user, roles)

### Viewing Migration Status

```bash
cd InfoSecApp.Web
dotnet ef migrations list
```

### Removing Last Migration (if not applied)

```bash
cd InfoSecApp.Web
dotnet ef migrations remove
```

### Reverting Database to Previous Migration

```bash
cd InfoSecApp.Web
dotnet ef database update PreviousMigrationName
```

## Accessing SQL Server

### Using SQL Server Management Tools

You can connect to the containerized SQL Server using:

**SQL Server Management Studio (SSMS):**
- Server: localhost,1433
- Authentication: SQL Server Authentication
- Login: sa
- Password: InfoSec@Pass123

**Azure Data Studio:**
- Server: localhost,1433
- Authentication Type: SQL Login
- User name: sa
- Password: InfoSec@Pass123

**Command Line (sqlcmd):**

From inside the container:
```bash
docker exec -it infosec-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P InfoSec@Pass123
```

Example queries:
```sql
-- List all databases
SELECT name FROM sys.databases;
GO

-- Use the InfoSecAppDb database
USE InfoSecAppDb;
GO

-- List all tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
GO

-- Query users
SELECT Email, FirstName, LastName FROM AspNetUsers;
GO

-- Query roles
SELECT Name FROM AspNetRoles;
GO

-- Exit
EXIT
```

## Troubleshooting

### Container Won't Start

```bash
# Check logs for errors
docker logs infosec-sqlserver

# Common issues:
# 1. Port 1433 already in use
# 2. Password doesn't meet complexity requirements
# 3. Insufficient memory allocated to Docker
```

### Can't Connect to Database

```bash
# Verify container is running
docker ps | grep sqlserver

# Check if port is accessible
telnet localhost 1433
# or
nc -zv localhost 1433

# Test connection with sqlcmd
docker exec infosec-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P InfoSec@Pass123 -Q "SELECT @@VERSION"
```

### Database Migrations Fail

```bash
# Ensure SQL Server is running
docker ps | grep sqlserver

# Check connection string in appsettings.json
# Verify SA password matches docker-compose.yml

# Try applying migrations with verbose output
cd InfoSecApp.Web
dotnet ef database update --verbose
```

### Reset Everything

If you need to start fresh:

```bash
# Stop and remove everything
docker compose down -v

# Remove all migrations
rm -rf InfoSecApp.Web/Data/Migrations

# Start SQL Server
docker compose up -d

# Wait for SQL Server to be ready (about 30 seconds)
sleep 30

# Create new migrations
cd InfoSecApp.Web
dotnet ef migrations add InitialMigration

# Apply migrations
dotnet ef database update
```

## Production Considerations

### Security
⚠️ **DO NOT use these settings in production:**

1. **Change the SA password** to a strong, unique password
2. **Use Azure Key Vault** or similar for storing connection strings
3. **Create dedicated database users** with minimal permissions instead of using SA
4. **Enable SSL/TLS** connections
5. **Use managed database services** (Azure SQL Database, AWS RDS) instead of containers

### Connection String for Production

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=production-server.database.windows.net;Database=InfoSecAppDb;User Id=appuser;Password=<secure-password>;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true"
  }
}
```

Store this in:
- Azure Key Vault
- AWS Secrets Manager  
- Environment variables (encrypted)
- User secrets (development only)

### Backup Strategy

For production SQL Server:

```bash
# Create backup
docker exec infosec-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P InfoSec@Pass123 -Q "BACKUP DATABASE InfoSecAppDb TO DISK='/var/opt/mssql/backup/InfoSecAppDb.bak'"

# Copy backup from container
docker cp infosec-sqlserver:/var/opt/mssql/backup/InfoSecAppDb.bak ./backups/

# Restore backup
docker exec infosec-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P InfoSec@Pass123 -Q "RESTORE DATABASE InfoSecAppDb FROM DISK='/var/opt/mssql/backup/InfoSecAppDb.bak' WITH REPLACE"
```

## Docker Volume Management

### View Volumes

```bash
docker volume ls | grep sqlserver
```

### Inspect Volume

```bash
docker volume inspect infosec-ai-v1_sqlserver_data
```

### Backup Volume Data

```bash
docker run --rm -v infosec-ai-v1_sqlserver_data:/data -v $(pwd):/backup ubuntu tar czf /backup/sqlserver-backup.tar.gz /data
```

### Restore Volume Data

```bash
docker run --rm -v infosec-ai-v1_sqlserver_data:/data -v $(pwd):/backup ubuntu tar xzf /backup/sqlserver-backup.tar.gz -C /
```

## Environment Variables

You can override settings using environment variables:

```bash
# Custom SA password
export MSSQL_SA_PASSWORD=MyStrongPassword123!

# Start with custom password
docker compose up -d
```

Or edit `docker-compose.yml`:

```yaml
environment:
  - MSSQL_SA_PASSWORD=${MSSQL_SA_PASSWORD:-InfoSec@Pass123}
```

## Health Checks

The compose file includes health checks:

```yaml
healthcheck:
  test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P InfoSec@Pass123 -Q "SELECT 1" || exit 1
  interval: 10s
  timeout: 3s
  retries: 10
  start_period: 10s
```

This ensures SQL Server is fully operational before marking the container as healthy.

## Quick Reference

```bash
# Start SQL Server
docker compose up -d

# View logs
docker logs -f infosec-sqlserver

# Stop SQL Server
docker compose stop

# Restart SQL Server
docker compose restart

# Remove everything (including data)
docker compose down -v

# Execute SQL query
docker exec infosec-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P InfoSec@Pass123 -Q "SELECT @@VERSION"

# Apply migrations
cd InfoSecApp.Web && dotnet ef database update
```

## Summary

✅ SQL Server 2022 running in Docker
✅ Persistent data storage with Docker volumes
✅ Health checks for reliability
✅ Easy start/stop/restart commands
✅ Compatible with all SQL Server tools
✅ Ready for development use
⚠️ Requires production hardening for deployment
