-- Add migration history for existing TenantMasterDb
-- This tells EF Core that the initial migration has already been applied

USE TenantMasterDb;
GO

-- Create __EFMigrationsHistory table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Get the latest migration name from your Migrations folder
-- Replace 'YYYYMMDDHHMMSS_InitialCreate' with your actual migration name
-- You can find it in: MultitenantPerDb/Modules/Tenancy/Infrastructure/Persistence/Migrations/

-- Check if migration is already recorded
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251108224141_FixConnectionStrings')
BEGIN
    -- Add the initial migration record
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (
        '20251108224141_FixConnectionStrings',
        '8.0.0'
    );
    
    PRINT 'Migration history added successfully';
END
ELSE
BEGIN
    PRINT 'Migration history already exists';
END
GO
