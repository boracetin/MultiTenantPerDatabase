using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MultitenantPerDb.Core.Infrastructure;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Memory Cache for caching pipeline behavior
builder.Services.AddMemoryCache();

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("AllowSpecific", policy =>
    {
        policy.WithOrigins("http://localhost:5231", "https://localhost:5231")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// HttpContextAccessor - TenantResolver için gerekli
builder.Services.AddHttpContextAccessor();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Multitenant Per Database API", Version = "v1" });
    
    // JWT Authentication için Swagger konfigürasyonu
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===============================================
// MODULAR MONOLITH - Automatic Module Discovery
// ===============================================
builder.Services.AddModules(builder.Configuration);

// ===============================================
// SHARED SERVICES - Domain & Infrastructure
// ===============================================
builder.Services.AddSharedServices(builder.Configuration);

// Shared Kernel Services - Generic UnitOfWork
builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();

// Encryption Service - TenantId encryption in JWT
builder.Services.AddSingleton<MultitenantPerDb.Core.Infrastructure.Security.IEncryptionService, MultitenantPerDb.Core.Infrastructure.Security.AesEncryptionService>();

// Security Services - Authorization & Rate Limiting
builder.Services.AddScoped<MultitenantPerDb.Core.Application.Interfaces.ICurrentUserService, MultitenantPerDb.Core.Infrastructure.Services.CurrentUserService>();
builder.Services.AddSingleton<MultitenantPerDb.Core.Application.Interfaces.IRateLimitService, MultitenantPerDb.Core.Infrastructure.Services.RateLimitService>();

var app = builder.Build();

// ===============================================
// AUTO-MIGRATE MODULES ON STARTUP
// ===============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("[STARTUP] Starting module migrations...");
        
        // Get all registered modules
        var modules = services.GetServices<IModule>();
        
        foreach (var module in modules)
        {
            await module.MigrateAsync(services);
        }
        
        logger.LogInformation("[STARTUP] All modules migrated successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[STARTUP] An error occurred during module migration");
        throw; // Don't start application if migration fails
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS - Authentication'dan ÖNCE olmalı
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ===============================================
// MODULAR MONOLITH - Module Middleware Pipeline
// ===============================================
app.UseModules();

app.MapControllers();

// Health Check Endpoint for Docker
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
}))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
