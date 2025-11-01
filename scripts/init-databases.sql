-- ============================================
-- Initial Database Setup Script
-- Creates master database and sample tenants
-- ============================================

USE master;
GO

-- Create Master Database (Tenant Registry)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MultiTenantMaster')
BEGIN
    CREATE DATABASE MultiTenantMaster;
    PRINT 'Master database created successfully.';
END
GO

USE MultiTenantMaster;
GO

-- Create Tenants Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tenants')
BEGIN
    CREATE TABLE Tenants (
        Id INT PRIMARY KEY IDENTITY(1,1),
        TenantName NVARCHAR(100) NOT NULL,
        DatabaseName NVARCHAR(100) NOT NULL,
        ConnectionString NVARCHAR(500) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Tenants table created successfully.';
END
GO

-- Create Sample Tenant Databases
USE master;
GO

-- Tenant 1: Acme Corp
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TenantDb_AcmeCorp')
BEGIN
    CREATE DATABASE TenantDb_AcmeCorp;
    PRINT 'TenantDb_AcmeCorp created successfully.';
END
GO

-- Tenant 2: GlobalTech
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TenantDb_GlobalTech')
BEGIN
    CREATE DATABASE TenantDb_GlobalTech;
    PRINT 'TenantDb_GlobalTech created successfully.';
END
GO

-- Tenant 3: StartupHub
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TenantDb_StartupHub')
BEGIN
    CREATE DATABASE TenantDb_StartupHub;
    PRINT 'TenantDb_StartupHub created successfully.';
END
GO

-- Insert Tenant Records
USE MultiTenantMaster;
GO

IF NOT EXISTS (SELECT * FROM Tenants WHERE Id = 1)
BEGIN
    INSERT INTO Tenants (TenantName, DatabaseName, ConnectionString, IsActive, CreatedAt)
    VALUES 
        ('Acme Corp', 'TenantDb_AcmeCorp', 'Server=sqlserver;Database=TenantDb_AcmeCorp;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;', 1, GETUTCDATE()),
        ('GlobalTech', 'TenantDb_GlobalTech', 'Server=sqlserver;Database=TenantDb_GlobalTech;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;', 1, GETUTCDATE()),
        ('StartupHub', 'TenantDb_StartupHub', 'Server=sqlserver;Database=TenantDb_StartupHub;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;', 1, GETUTCDATE());
    
    PRINT 'Sample tenants inserted successfully.';
END
GO

PRINT '============================================';
PRINT 'Database initialization completed!';
PRINT '============================================';
GO
