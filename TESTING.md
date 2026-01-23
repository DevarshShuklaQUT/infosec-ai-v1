# Testing Guide for InfoSec Application

## Prerequisites
- Both applications must be running:
  - Web App: `http://localhost:5000`
  - API: `http://localhost:7000`

## Test 1: Web Application Homepage

### Command:
```bash
curl http://localhost:5000
```

### Expected Result:
✅ Should return HTML with the homepage content including navigation links for Register and Login.

## Test 2: API Authentication - Missing API Key

### Command:
```bash
curl http://localhost:7000/api/data
```

### Expected Result:
✅ "API Key is missing"

## Test 3: API Authentication - Invalid API Key

### Command:
```bash
curl -H "X-API-Key: invalid-key" http://localhost:7000/api/data
```

### Expected Result:
✅ "Invalid API Key"

## Test 4: API Authentication - Valid API Key (GET)

### Command:
```bash
curl -H "X-API-Key: test-api-key-12345" http://localhost:7000/api/data
```

### Expected Result:
✅ JSON array with data items:
```json
[
  {"id":1,"name":"Item 1","description":"First item"},
  {"id":2,"name":"Item 2","description":"Second item"},
  {"id":3,"name":"Item 3","description":"Third item"}
]
```

## Test 5: API POST Endpoint with Valid API Key

### Command:
```bash
curl -X POST \
  -H "X-API-Key: test-api-key-12345" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Item","description":"Created via API"}' \
  http://localhost:7000/api/data
```

### Expected Result:
✅ JSON object with created item:
```json
{"id":201,"name":"Test Item","description":"Created via API"}
```

## Test 6: Health Check Endpoint (No Authentication Required)

### Command:
```bash
curl http://localhost:7000/health
```

### Expected Result:
✅ JSON with health status:
```json
{"status":"Healthy","timestamp":"2026-01-22T05:47:31.462932Z"}
```

## Test 7: User Registration

1. Navigate to: `http://localhost:5000/Identity/Account/Register`
2. Fill in:
   - Email: `testuser@example.com`
   - Password: `Test@123`
   - Confirm Password: `Test@123`
3. Click Register

### Expected Result:
✅ User should be created and redirected to home page with user logged in

## Test 8: User Login

1. Navigate to: `http://localhost:5000/Identity/Account/Login`
2. Fill in:
   - Email: `testuser@example.com`
   - Password: `Test@123`
3. Click Log in

### Expected Result:
✅ User should be logged in and redirected to home page

## Test 9: SuperAdmin Login

1. Navigate to: `http://localhost:5000/Identity/Account/Login`
2. Fill in:
   - Email: `superadmin@infosecapp.com`
   - Password: `SuperAdmin@123`
3. Click Log in

### Expected Result:
✅ SuperAdmin should be logged in

## Test 10: SuperAdmin API - Get Tenants

### Note: This requires authentication cookie from SuperAdmin login

After logging in as SuperAdmin through a browser, use browser dev tools or a tool like Postman to make authenticated requests.

### Endpoint:
```
GET http://localhost:5000/api/superadmin/tenants
```

### Expected Result:
✅ JSON array of tenants (empty array initially):
```json
[]
```

## Test 11: SuperAdmin API - Create Tenant

### Endpoint:
```
POST http://localhost:5000/api/superadmin/tenants
```

### Request Body:
```json
{
  "name": "Acme Corporation",
  "identifier": "acme",
  "connectionString": null
}
```

### Expected Result:
✅ JSON object with created tenant including ID

## Test 12: SuperAdmin API - Get Users

### Endpoint:
```
GET http://localhost:5000/api/superadmin/users
```

### Expected Result:
✅ JSON array of users including the SuperAdmin and any registered users

## Database Verification

### Check Users:
```bash
cd InfoSecApp.Web
sqlite3 app.db "SELECT Email, FirstName, LastName FROM AspNetUsers;"
```

### Expected Result:
✅ Should show superadmin@infosecapp.com and any registered users

### Check Roles:
```bash
cd InfoSecApp.Web
sqlite3 app.db "SELECT Name FROM AspNetRoles;"
```

### Expected Result:
✅ Should show: SuperAdmin, Admin, User

### Check User Roles:
```bash
cd InfoSecApp.Web
sqlite3 app.db "SELECT u.Email, r.Name FROM AspNetUsers u JOIN AspNetUserRoles ur ON u.Id = ur.UserId JOIN AspNetRoles r ON ur.RoleId = r.Id;"
```

### Expected Result:
✅ Should show superadmin@infosecapp.com with SuperAdmin role

## API Keys Configuration

Valid API keys are configured in `InfoSecApp.Api/appsettings.json`:
- `test-api-key-12345` (Development)
- `prod-api-key-67890` (Production)

## Multi-Tenant Testing

### Test 1: Create a tenant via SuperAdmin API
### Test 2: Register a new user
### Test 3: Assign user to tenant (via database or API)
### Test 4: Verify tenant isolation

## Security Features Verified

✅ ASP.NET Core Identity authentication for Web UI
✅ API-key authentication for API endpoints
✅ Role-based authorization for SuperAdmin endpoints
✅ Password requirements enforced
✅ Health check endpoint accessible without authentication
✅ Multi-tenant data model in place

## Test Results Summary

All tests passed successfully:
- ✅ Web application loads and displays properly
- ✅ API authentication works correctly (missing, invalid, valid keys)
- ✅ API endpoints return correct data with valid authentication
- ✅ Health check endpoint works without authentication
- ✅ SuperAdmin user created with correct role
- ✅ Database schema created with tenant support
- ✅ Identity pages (Register/Login) are accessible
