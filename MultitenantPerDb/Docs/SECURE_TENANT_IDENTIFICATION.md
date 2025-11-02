# Secure Tenant Identification with Subdomain Branding

## 🔐 Security-First Approach

### Critical Security Rule

**TenantId MUST ONLY come from JWT claims. Never from headers, query strings, or subdomains.**

This prevents unauthorized access to other tenants' data through URL/header manipulation.

## 🎨 Subdomain Purpose: UI Branding ONLY

Subdomains are used **exclusively** for UI customization, NOT for tenant identification or data access.

### Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│  User navigates to: https://tenant1.myapp.com               │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  Frontend calls: GET /api/tenant-branding/current           │
│  Returns: Logo, Colors, Background Image, Custom CSS        │
│  ✅ Anonymous access OK - no sensitive data                  │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  UI applies branding (shows Tenant1's logo & colors)        │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  User clicks Login                                          │
│  POST /api/auth/login { username, password }               │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  LoginHandler:                                              │
│  1. Query Master DB (TenantDbContext) by username          │
│  2. Find User → Get User.TenantId                          │
│  3. Generate JWT with TenantId claim                       │
│  4. Return JWT token                                        │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  Frontend stores JWT token                                  │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  User requests data: GET /api/products                      │
│  Authorization: Bearer {JWT with TenantId claim}           │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  TenantResolver:                                            │
│  1. Extract TenantId from JWT claim ✅ SECURE               │
│  2. Ignore subdomain for data access ✅ SAFE                │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  TenantDbContextFactory:                                    │
│  1. Query Master DB for Tenant by TenantId (from JWT)      │
│  2. Get Tenant.ConnectionString                             │
│  3. Create ApplicationDbContext with tenant DB              │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  Return products from Tenant's database                     │
│  ✅ Secure: Data access based on JWT claim only             │
└─────────────────────────────────────────────────────────────┘
```

## 🏗️ Architecture Components

### 1. TenantResolver (Secure)

```csharp
public class TenantResolver : ITenantResolver
{
    public string? TenantId
    {
        get
        {
            // Priority 1: Explicit set (background jobs only)
            if (_isExplicitlySet) return _tenantId;
            
            // Priority 2: JWT Claims (ONLY secure source)
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.FindFirst("TenantId")?.Value;
            }
            
            // NO fallback to headers/query strings/subdomains!
            return null;
        }
    }
    
    // Separate method for UI branding only
    public string? GetSubdomainForBranding()
    {
        // Extract subdomain for UI customization
        // NOT used for data access
        return ExtractTenantFromSubdomain(httpContext);
    }
}
```

### 2. Tenant Entity (with Branding)

```csharp
public class Tenant : BaseEntity
{
    public string Name { get; private set; }
    public string ConnectionString { get; private set; }
    public bool IsActive { get; private set; }
    
    // Subdomain (e.g., "tenant1" in tenant1.myapp.com)
    public string? Subdomain { get; private set; }
    
    // Branding & Customization
    public string? DisplayName { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BackgroundImageUrl { get; private set; }
    public string? PrimaryColor { get; private set; }
    public string? SecondaryColor { get; private set; }
    public string? CustomCss { get; private set; }
}
```

### 3. TenantBrandingController (Anonymous Access)

```csharp
[ApiController]
[Route("api/tenant-branding")]
public class TenantBrandingController : ControllerBase
{
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentBranding()
    {
        var subdomain = _tenantResolver.GetSubdomainForBranding();
        
        var tenant = await _tenantDbContext.Tenants
            .Where(t => t.Subdomain == subdomain && t.IsActive)
            .Select(t => new { /* branding fields only */ })
            .FirstOrDefaultAsync();
        
        return Ok(new { subdomain, branding = tenant });
    }
}
```

### 4. TenantMiddleware (Simplified)

```csharp
public class TenantMiddleware
{
    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        // TenantResolver automatically resolves TenantId from JWT only
        // No manual setting from headers/query strings
        // Subdomain available via GetSubdomainForBranding() for UI only
        
        await _next(context);
    }
}
```

## 🎨 Branding Features

### Customizable Properties

| Property | Purpose | Example |
|----------|---------|---------|
| `Subdomain` | URL identifier | `tenant1` |
| `DisplayName` | Company name shown in UI | `Acme Corporation` |
| `LogoUrl` | Company logo | `/assets/logos/acme.png` |
| `BackgroundImageUrl` | Login page background | `/assets/bg/acme.jpg` |
| `PrimaryColor` | Main theme color | `#1976D2` |
| `SecondaryColor` | Accent color | `#424242` |
| `CustomCss` | Additional styling | `.header { ... }` |

### Frontend Integration Example

```javascript
// 1. Get branding on page load (anonymous)
const response = await fetch('https://tenant1.myapp.com/api/tenant-branding/current');
const { subdomain, branding } = await response.json();

// 2. Apply branding to UI
document.documentElement.style.setProperty('--primary-color', branding.primaryColor);
document.documentElement.style.setProperty('--secondary-color', branding.secondaryColor);
document.querySelector('.logo').src = branding.logoUrl;
document.querySelector('.login-bg').style.backgroundImage = `url(${branding.backgroundImageUrl})`;

if (branding.customCss) {
    const style = document.createElement('style');
    style.textContent = branding.customCss;
    document.head.appendChild(style);
}

// 3. Login (subdomain shown in UI, but NOT used for authentication)
const loginResponse = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username: 'admin', password: '123456' })
});

const { token, tenantId } = await loginResponse.json();

// 4. Store JWT (contains TenantId claim)
localStorage.setItem('jwt', token);

// 5. Make authenticated requests
const productsResponse = await fetch('/api/products', {
    headers: {
        'Authorization': `Bearer ${token}`
        // TenantId automatically extracted from JWT
        // Subdomain ignored for data access
    }
});
```

## 🔒 Security Benefits

### ✅ Secure Approach (Current)

```
User navigates to: https://tenant2.myapp.com
User logs in as: admin (from Tenant1)
JWT contains: TenantId = 1

Request: GET /api/products
TenantResolver reads: TenantId = 1 (from JWT)
Subdomain "tenant2" is IGNORED for data access
Result: Returns products from Tenant1 (correct!)
```

### ❌ Insecure Approach (Prevented)

```
User navigates to: https://tenant2.myapp.com
TenantId extracted from subdomain: tenant2 ❌ SPOOFING
User could access Tenant2's data without authentication ❌ BREACH
```

## 🧪 Testing

### Test 1: Branding Endpoint (Anonymous)

```bash
# Get branding for tenant1 subdomain
curl http://tenant1.localhost:5231/api/tenant-branding/current

# Response:
{
  "subdomain": "tenant1",
  "branding": {
    "displayName": "Tenant 1 Company",
    "logoUrl": null,
    "backgroundImageUrl": null,
    "primaryColor": "#1976D2",
    "secondaryColor": "#424242",
    "customCss": null
  }
}
```

### Test 2: Secure Login

```bash
# Login (subdomain visible but not used for auth)
curl -X POST http://tenant1.localhost:5231/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user1","password":"123456"}'

# Response:
{
  "token": "eyJ...",
  "username": "user1",
  "tenantId": 1,
  "expiresAt": "2025-11-03T10:00:00Z"
}
```

### Test 3: Secure Data Access

```bash
# Get products (TenantId from JWT only)
curl http://tenant1.localhost:5231/api/products \
  -H "Authorization: Bearer eyJ..."

# TenantResolver extracts TenantId from JWT
# Subdomain "tenant1" is ignored
# Returns products from JWT's TenantId database
```

### Test 4: Cross-Subdomain Security

```bash
# User from Tenant1 tries to access via Tenant2 subdomain
curl http://tenant2.localhost:5231/api/products \
  -H "Authorization: Bearer {Tenant1-JWT}"

# Subdomain: tenant2 (UI shows Tenant2 branding)
# JWT TenantId: 1 (from Tenant1)
# Result: Returns Tenant1 products (correct!)
# UI shows Tenant2 branding but data is from Tenant1
```

## 📊 Comparison

| Aspect | Headers/Query | Subdomain (Old) | JWT Claims (Secure) |
|--------|--------------|-----------------|-------------------|
| **Data Access** | ❌ Spoofable | ❌ Spoofable | ✅ Secure |
| **UI Branding** | ❌ Not intuitive | ✅ User-friendly | ⚠️ No URL context |
| **Security** | ❌ Very Low | ❌ Low | ✅ High |
| **Use Case** | ❌ None | ✅ Branding only | ✅ Data access |

## ✅ Best Practices

1. **Always validate JWT** before accessing data
2. **Use subdomain for UI/UX only**, never for authorization
3. **TenantId claim is the source of truth** for data access
4. **Branding endpoint is anonymous** - returns only UI settings, no sensitive data
5. **Log all tenant resolution** for audit purposes
6. **Rate limit branding endpoint** to prevent enumeration attacks

## 🎉 Summary

**Subdomain**: User-friendly branding and UI customization  
**JWT TenantId**: Secure data access and tenant isolation  
**Separation of Concerns**: UI ≠ Security  

This architecture provides the best of both worlds:
- Beautiful, branded user experience
- Rock-solid security and data isolation

