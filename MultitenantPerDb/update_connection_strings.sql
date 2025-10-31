-- Update Tenant Connection Strings
UPDATE Tenants 
SET ConnectionString = 'Server=BORA\BRCTN;Database=Tenant1Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;' 
WHERE Id = 1;

UPDATE Tenants 
SET ConnectionString = 'Server=BORA\BRCTN;Database=Tenant2Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;' 
WHERE Id = 2;

-- Verify
SELECT Id, Name, ConnectionString FROM Tenants;
