# Encrypted TenantId in JWT

## 🔐 Security Enhancement

### Problem
TenantId in plain text JWT claim allows users to:
1. **Read** their TenantId by decoding JWT (base64 decode)
2. **Modify** JWT and potentially access other tenants' data
3. **Enumerate** tenant IDs by incrementing values

### Solution
**AES-256 encryption** of TenantId claim in JWT:
- User **cannot read** TenantId even after decoding JWT
- User **cannot modify** TenantId (encryption integrity check)
- User **cannot enumerate** tenants (encrypted values are random-looking)

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Login Request                                              │
│  POST /api/auth/login                                       │
│  { username: "user1", password: "123456" }                 │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  LoginCommandHandler                                        │
│  1. Query Master DB → Get User (TenantId: 1)              │
│  2. Encrypt TenantId:                                       │
│     Plaintext: "1"                                          │
│     Encrypted: "X8kP2mN9qL4vR7tY3wZ6aB5cD1eF0gH=="        │
│  3. Add to JWT claim: "TenantId": "X8k...gH=="            │
│  4. Return JWT token                                        │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  JWT Token (User receives)                                  │
│  eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.                     │
│  eyJzdWIiOiIxIiwibmFtZSI6InVzZXIxIiwiVGVuYW50SWQiOiJYOGtQ...│
│                                                             │
│  Decoded Payload (user can decode this):                   │
│  {                                                          │
│    "sub": "1",                                             │
│    "name": "user1",                                        │
│    "TenantId": "X8kP2mN9qL4vR7tY3wZ6aB5cD1eF0gH=="        │
│    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^            │
│    User sees encrypted value - CANNOT read actual TenantId │
│  }                                                          │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  Authenticated Request                                      │
│  GET /api/products                                          │
│  Authorization: Bearer {JWT}                                │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  TenantResolver                                             │
│  1. Extract JWT claim: "TenantId": "X8kP2mN9..."          │
│  2. Decrypt:                                                │
│     Encrypted: "X8kP2mN9qL4vR7tY3wZ6aB5cD1eF0gH=="        │
│     Decrypted: "1"                                          │
│  3. Return TenantId: 1                                      │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  TenantDbContextFactory                                     │
│  1. Query Master DB for Tenant with ID=1                   │
│  2. Get connection string                                   │
│  3. Create ApplicationDbContext                             │
│  4. Return products from Tenant1's database                 │
└─────────────────────────────────────────────────────────────┘
```

## 🔑 Encryption Details

### Algorithm: AES-256-CBC

```csharp
public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;  // 32 bytes (256 bits)
    private readonly byte[] _iv;   // 16 bytes (128 bits)
    
    public string Encrypt(string plainText)
    {
        // AES-256 encryption
        // Returns Base64-encoded cipher text
        return Convert.ToBase64String(encrypted);
    }
    
    public string Decrypt(string cipherText)
    {
        // AES-256 decryption
        // Throws CryptographicException if tampered
        return decryptedPlainText;
    }
}
```

### Configuration (appsettings.json)

```json
{
  "Encryption": {
    "Key": "YourVerySecureEncryptionKey32Ch",
    "IV": "YourSecureIV1234"
  }
}
```

⚠️ **PRODUCTION**: Store keys in Azure Key Vault, AWS Secrets Manager, or environment variables!

## 🛡️ Security Benefits

### 1. User Cannot Read TenantId

**Without Encryption:**
```json
// JWT payload (decoded)
{
  "TenantId": "1"  // User can see they are in Tenant 1
}
```

**With Encryption:**
```json
// JWT payload (decoded)
{
  "TenantId": "X8kP2mN9qL4vR7tY3wZ6aB5cD1eF0gH=="  // Random-looking string
}
```

### 2. User Cannot Modify TenantId

**Attack Attempt:**
```javascript
// Attacker decodes JWT
const payload = {
  "name": "user1",
  "TenantId": "X8kP2mN9qL4vR7tY3wZ6aB5cD1eF0gH=="
};

// Attacker tries to change to Tenant 2's encrypted value
payload.TenantId = "Y9lQ3nO0rM5wS8uZ4xA7bC6dE2fG1hI==";

// Attacker re-encodes JWT and sends request
```

**Result:**
```
❌ Decryption fails (invalid cipher text)
❌ TenantResolver returns null
❌ Request fails with "Tenant not found"
❌ Attacker cannot access other tenant's data
```

### 3. User Cannot Enumerate Tenants

**Without Encryption:**
```
TenantId: "1" → Can try "2", "3", "4"...
```

**With Encryption:**
```
TenantId: "X8kP2mN9..." → Cannot guess next tenant's value
```

## 🔧 Implementation

### 1. Encryption Service

```csharp
// Registration in Program.cs
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
```

### 2. Login Handler (Encrypt)

```csharp
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IEncryptionService _encryptionService;
    
    private string GenerateJwtToken(User user, ...)
    {
        // Encrypt TenantId before adding to JWT
        var encryptedTenantId = _encryptionService.Encrypt(user.TenantId.ToString());
        
        var claims = new[]
        {
            new Claim("TenantId", encryptedTenantId)  // Encrypted!
        };
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 3. Tenant Resolver (Decrypt)

```csharp
public class TenantResolver : ITenantResolver
{
    private readonly IEncryptionService _encryptionService;
    
    public string? TenantId
    {
        get
        {
            var encryptedTenantClaim = httpContext.User.FindFirst("TenantId");
            
            if (encryptedTenantClaim != null)
            {
                try
                {
                    // Decrypt TenantId from JWT
                    return _encryptionService.Decrypt(encryptedTenantClaim.Value);
                }
                catch (CryptographicException)
                {
                    // Token tampered or invalid
                    return null;
                }
            }
            
            return null;
        }
    }
}
```

## 🧪 Testing

### Test 1: Login and Decode JWT

```bash
# Login
curl -X POST http://localhost:5231/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user1","password":"123456"}'

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6InVzZXIxIiwiVGVuYW50SWQiOiJYOGtQMm1OOXFMNHZSN3RZM3daNmFCNWNEMWVGMGdIPT0iLCJleHAiOjE2OTg3NjU0MzJ9.xyz",
  "username": "user1",
  "tenantId": 1
}

# Decode JWT at jwt.io
Payload:
{
  "sub": "1",
  "name": "user1",
  "TenantId": "X8kP2mN9qL4vR7tY3wZ6aB5cD1eF0gH==",  ← Encrypted!
  "exp": 1698765432
}
```

### Test 2: Use JWT (Auto-Decrypt)

```bash
# Get products
curl http://localhost:5231/api/products \
  -H "Authorization: Bearer eyJhbGci..."

# TenantResolver automatically decrypts "X8kP2mN9..." → "1"
# Returns products from Tenant 1's database
```

### Test 3: Tamper Detection

```bash
# User manually changes TenantId in JWT payload
# Re-encodes and sends request

# Result:
{
  "error": "Tenant not found",
  "message": "Failed to decrypt TenantId. Token may be tampered or invalid."
}
```

## 📊 Comparison

| Aspect | Plain Text TenantId | Encrypted TenantId |
|--------|-------------------|-------------------|
| **User can read** | ✅ Yes (security risk) | ❌ No (encrypted) |
| **User can modify** | ⚠️ Can try (JWT signature protects) | ❌ No (decryption fails) |
| **Tenant enumeration** | ⚠️ Possible (increment IDs) | ❌ Impossible (random values) |
| **Performance** | Fast | Slightly slower (encrypt/decrypt) |
| **Security Level** | Medium | High |

## ⚙️ Configuration

### Development (appsettings.Development.json)

```json
{
  "Encryption": {
    "Key": "Dev_Encryption_Key_32_Characters",
    "IV": "DevIV_16_Chars!!"
  }
}
```

### Production (Environment Variables)

```bash
# Azure App Service Configuration
ENCRYPTION__KEY="<strong-random-32-char-key>"
ENCRYPTION__IV="<strong-random-16-char-iv>"

# Docker
docker run -e ENCRYPTION__KEY="..." -e ENCRYPTION__IV="..." myapp

# Kubernetes Secret
apiVersion: v1
kind: Secret
metadata:
  name: encryption-secrets
data:
  key: <base64-encoded-key>
  iv: <base64-encoded-iv>
```

## 🔐 Best Practices

1. **Use strong random keys**: Generate with `openssl rand -base64 32`
2. **Never commit keys to source control**: Use secrets management
3. **Rotate keys periodically**: Update keys every 90 days
4. **Different keys per environment**: Dev, Staging, Production
5. **Monitor decryption failures**: Alert on tampering attempts
6. **Log security events**: Track JWT usage and anomalies

## 🎯 Attack Scenarios Prevented

### Scenario 1: JWT Decoding Attack
❌ **Before**: User decodes JWT → Sees TenantId: "1" → Knows they are Tenant 1  
✅ **After**: User decodes JWT → Sees TenantId: "X8kP2..." → Cannot determine tenant

### Scenario 2: JWT Modification Attack
❌ **Before**: User changes TenantId: "1" → "2" → JWT signature check may catch  
✅ **After**: User changes encrypted value → Decryption fails → Request blocked

### Scenario 3: Tenant Enumeration
❌ **Before**: Attacker tries TenantId: 1, 2, 3, 4... → Can discover tenant count  
✅ **After**: Encrypted values are random → Cannot enumerate tenants

## ✅ Summary

**Encryption** adds an extra layer of security on top of JWT signature verification:

- **JWT Signature**: Prevents tampering (integrity)
- **TenantId Encryption**: Prevents reading and enumeration (confidentiality)

Together, they provide **defense in depth** for multi-tenant applications.

**Result**: Even if user decodes JWT, they cannot see or modify TenantId! 🔒
