# Subdomain-Based Tenant Resolution

## 🎯 Overview

The system supports multiple tenant identification strategies with the following priority order:

1. **Explicit Set** (Background jobs)
2. **JWT Claims** (Authenticated requests)
3. **Subdomain** (e.g., tenant1.myapp.com)
4. **HTTP Header** (X-Tenant-ID)
5. **Query String** (tenantId)

## 🌐 Subdomain Strategy

### How It Works

The `TenantResolver` automatically extracts the tenant identifier from the subdomain:

```
https://tenant1.myapp.com/api/products
         ↓
    Subdomain: "tenant1"
         ↓
    Lookup Tenant by Name: "Tenant1"
         ↓
    Load ApplicationDbContext with Tenant1's connection string
```

### Subdomain Examples

| URL | Extracted Tenant | Result |
|-----|-----------------|--------|
| `tenant1.localhost:5231/api/products` | `tenant1` | ✅ Valid |
| `tenant2.myapp.com/api/orders` | `tenant2` | ✅ Valid |
| `www.myapp.com/api/products` | `null` | ❌ Reserved subdomain |
| `api.myapp.com/api/products` | `null` | ❌ Reserved subdomain |
| `localhost:5231/api/products` | `null` | ⚠️ No subdomain, fallback to other strategies |
| `myapp.com/api/products` | `null` | ⚠️ No subdomain, fallback to other strategies |

### Reserved Subdomains

These subdomains are **NOT** treated as tenant identifiers:

- `www` - Main website
- `api` - API gateway
- `admin` - Admin panel
- `app` - Main application
- `localhost` - Development

## 🔄 Complete Resolution Flow

### Example 1: Subdomain Resolution (Anonymous Request)

```http
GET http://tenant1.localhost:5231/api/products
```

**Flow:**
1. TenantMiddleware executes
2. TenantResolver.TenantId is called
3. User is NOT authenticated → Skip JWT claims
4. Extract subdomain: `tenant1`
5. Query TenantDbContext: `Tenants.FirstOrDefault(t => t.Name == "tenant1")`
6. Create ApplicationDbContext with Tenant1's connection string
7. Return products from Tenant1's database

### Example 2: JWT Claims (Authenticated Request)

```http
GET http://localhost:5231/api/products
Authorization: Bearer {JWT with TenantId: 1}
```

**Flow:**
1. User authenticated → JWT contains `TenantId: "1"`
2. TenantResolver reads JWT claim
3. Subdomain check is **skipped** (JWT has higher priority)
4. Query TenantDbContext: `Tenants.FirstOrDefault(t => t.Id == 1)`
5. Create ApplicationDbContext with Tenant1's connection string
6. Return products from Tenant1's database

### Example 3: Header Fallback

```http
GET http://localhost:5231/api/products
X-Tenant-ID: 1
```

**Flow:**
1. User NOT authenticated
2. No subdomain extracted (localhost)
3. TenantMiddleware reads `X-Tenant-ID` header
4. Manually sets tenant: `tenantResolver.SetTenant("1")`
5. Rest of the flow continues...

### Example 4: Query String Fallback

```http
GET http://localhost:5231/api/products?tenantId=1
```

**Flow:**
1. User NOT authenticated
2. No subdomain extracted
3. No header provided
4. TenantMiddleware reads `tenantId` query parameter
5. Manually sets tenant: `tenantResolver.SetTenant("1")`
6. Rest of the flow continues...

## 🏗️ Architecture Components

### 1. TenantResolver

```csharp
public string? TenantId
{
    get
    {
        // Priority 1: Explicit set (background jobs)
        if (_isExplicitlySet) return _tenantId;
        
        // Priority 2: JWT Claims (authenticated)
        if (User.IsAuthenticated)
        {
            return User.FindFirst("TenantId")?.Value;
        }
        
        // Priority 3: Subdomain extraction
        var subdomain = ExtractTenantFromSubdomain(HttpContext);
        if (!string.IsNullOrEmpty(subdomain))
        {
            return subdomain;
        }
        
        // Priority 4: Manual set (from middleware)
        return _tenantId;
    }
}

private string? ExtractTenantFromSubdomain(HttpContext context)
{
    var host = context.Request.Host.Host;
    var parts = host.Split('.');
    
    // Need at least 3 parts: [subdomain].[domain].[tld]
    if (parts.Length <= 2) return null;
    
    var subdomain = parts[0].ToLowerInvariant();
    
    // Check if reserved
    if (IsReserved(subdomain)) return null;
    
    return subdomain;
}
```

### 2. TenantMiddleware

```csharp
public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
{
    // TenantResolver automatically handles:
    // - JWT Claims
    // - Subdomain extraction
    
    // Middleware only handles fallback strategies
    if (!context.User.IsAuthenticated)
    {
        var currentTenant = tenantResolver.TenantId; // Triggers subdomain check
        
        if (string.IsNullOrEmpty(currentTenant))
        {
            // Fallback to Header or Query String
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
            {
                tenantResolver.SetTenant(tenantId);
            }
            else if (context.Request.Query.TryGetValue("tenantId", out var queryTenantId))
            {
                tenantResolver.SetTenant(queryTenantId);
            }
        }
    }
    
    await _next(context);
}
```

### 3. TenantDbContextFactory

```csharp
public async Task<ApplicationDbContext> CreateDbContextAsync()
{
    var tenantIdentifier = _tenantResolver.TenantId;
    
    // Lookup by ID (from JWT) OR Name (from subdomain)
    var tenant = await _tenantDbContext.Tenants
        .FirstOrDefaultAsync(t => 
            (t.Id.ToString() == tenantIdentifier || 
             t.Name.ToLower() == tenantIdentifier.ToLower()) 
            && t.IsActive);
    
    if (tenant == null)
    {
        throw new InvalidOperationException($"Tenant not found: {tenantIdentifier}");
    }
    
    // Create tenant-specific DbContext
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseSqlServer(tenant.ConnectionString);
    
    return new ApplicationDbContext(optionsBuilder.Options);
}
```

## 🧪 Testing Scenarios

### Local Development (Windows hosts file)

Add to `C:\Windows\System32\drivers\etc\hosts`:

```
127.0.0.1   tenant1.localhost
127.0.0.1   tenant2.localhost
```

Then access:
```
http://tenant1.localhost:5231/api/products
http://tenant2.localhost:5231/api/products
```

### Docker Setup

Configure DNS or use Traefik with labels:

```yaml
services:
  app:
    labels:
      - "traefik.http.routers.app.rule=HostRegexp(`{subdomain:[a-z0-9]+}.myapp.com`)"
```

### Production DNS

Configure wildcard DNS:

```
*.myapp.com → Your Server IP
```

Then subdomains automatically resolve:
- `tenant1.myapp.com` → Tenant1 DB
- `tenant2.myapp.com` → Tenant2 DB

## 🔐 Security Considerations

1. **Subdomain Validation**: Reserved subdomains prevent conflicts
2. **Tenant Isolation**: Each tenant has separate database
3. **JWT Priority**: Authenticated requests use JWT, preventing subdomain spoofing
4. **Active Check**: Only active tenants are resolved
5. **Error Handling**: Clear error messages for invalid tenants

## 📊 Strategy Comparison

| Strategy | Priority | Use Case | Security |
|----------|----------|----------|----------|
| **JWT Claims** | 1 (Highest) | Authenticated API calls | ✅ Best - Token-based |
| **Subdomain** | 2 | Multi-tenant SaaS UI | ✅ Good - DNS-based |
| **HTTP Header** | 3 | API testing, mobile apps | ⚠️ Medium - Can be spoofed |
| **Query String** | 4 (Lowest) | Quick testing, debugging | ❌ Low - URL visible |

## 🚀 Usage Examples

### React SPA with Subdomain

```javascript
// User navigates to: https://tenant1.myapp.com
// Login:
const response = await fetch('https://tenant1.myapp.com/api/auth/login', {
  method: 'POST',
  body: JSON.stringify({ username: 'admin', password: '123456' })
});

const { token } = await response.json();

// Fetch products (JWT contains TenantId, subdomain is for UX only)
const products = await fetch('https://tenant1.myapp.com/api/products', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

### Mobile App with Header

```csharp
// User selects tenant from dropdown
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Tenant-ID", selectedTenantId);

// Login
var loginResponse = await client.PostAsJsonAsync("/api/auth/login", credentials);
var token = await loginResponse.Content.ReadAsAsync<LoginResponse>();

// Fetch products (JWT now contains TenantId)
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
var products = await client.GetFromJsonAsync<Product[]>("/api/products");
```

## ✅ Benefits

1. **User-Friendly URLs**: `tenant1.myapp.com` is cleaner than headers
2. **SEO**: Each tenant can have unique subdomain
3. **Branding**: Tenants feel like separate apps
4. **Flexibility**: Multiple strategies for different scenarios
5. **Security**: JWT priority prevents subdomain spoofing

## 🎉 Summary

The subdomain-based tenant resolution provides a seamless multi-tenant experience while maintaining security through JWT priority and multiple fallback strategies. Users can access their tenant via clean URLs while the system automatically routes to the correct database.
