# ============================================
# Quick Start Script - Windows PowerShell
# Starts Multi-Tenant application with Docker
# ============================================

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Multi-Tenant Per Database - Quick Start" -ForegroundColor Cyan
Write-Host "  (Application only - SQL Server on host)" -ForegroundColor Gray
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check Docker is running
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    docker version | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Check SQL Server on localhost
Write-Host "Checking SQL Server on localhost..." -ForegroundColor Yellow
try {
    $sqlCheck = Invoke-Expression "sqlcmd -S localhost -U sa -P YourStrong@Passw0rd123 -Q `"SELECT 1`" -h -1" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ SQL Server is accessible" -ForegroundColor Green
    } else {
        Write-Host "⚠ SQL Server not accessible. Make sure it's running on localhost:1433" -ForegroundColor Yellow
        Write-Host "  Connection String: Server=localhost;User Id=sa;Password=YourStrong@Passw0rd123" -ForegroundColor Gray
    }
} catch {
    Write-Host "⚠ SQL Server check skipped (sqlcmd not found)" -ForegroundColor Yellow
    Write-Host "  Make sure SQL Server is running on localhost:1433" -ForegroundColor Gray
}

# Create .env file if not exists
if (-not (Test-Path ".env")) {
    Write-Host "Creating .env file from template..." -ForegroundColor Yellow
    Copy-Item ".env.example" ".env"
    Write-Host "✓ .env file created" -ForegroundColor Green
} else {
    Write-Host "✓ .env file already exists" -ForegroundColor Green
}

# Stop and remove existing containers
Write-Host ""
Write-Host "Cleaning up existing containers..." -ForegroundColor Yellow
docker-compose down -v 2>$null
Write-Host "✓ Cleanup complete" -ForegroundColor Green

# Build and start services
Write-Host ""
Write-Host "Building and starting services..." -ForegroundColor Yellow
Write-Host "This may take a few minutes on first run..." -ForegroundColor Gray
docker-compose up -d --build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Services started successfully" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Services are starting up..." -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Waiting for services to be healthy..." -ForegroundColor Yellow
    
    # Wait for health checks
    Start-Sleep -Seconds 30
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "  🚀 Application is ready!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "📍 Service URLs:" -ForegroundColor Cyan
    Write-Host "   • API:        http://localhost:5231" -ForegroundColor White
    Write-Host "   • Swagger:    http://localhost:5231/swagger" -ForegroundColor White
    Write-Host "   • Health:     http://localhost:5231/health" -ForegroundColor White
    Write-Host ""
    Write-Host "🗄️ Database:" -ForegroundColor Cyan
    Write-Host "   • SQL Server: localhost:1433 (Host machine)" -ForegroundColor White
    Write-Host "   • Credentials: sa / YourStrong@Passw0rd123" -ForegroundColor White
    Write-Host ""
    Write-Host "� Test User:" -ForegroundColor Cyan
    Write-Host "   • Username:   testuser" -ForegroundColor White
    Write-Host "   • Password:   Test123" -ForegroundColor White
    Write-Host ""
    Write-Host "📚 Quick Commands:" -ForegroundColor Cyan
    Write-Host "   • View logs:     docker-compose logs -f" -ForegroundColor Gray
    Write-Host "   • Stop all:      docker-compose down" -ForegroundColor Gray
    Write-Host "   • Restart:       docker-compose restart" -ForegroundColor Gray
    Write-Host ""
    Write-Host "📖 Documentation: MultitenantPerDb/Docs/DOCKER_SETUP.md" -ForegroundColor Yellow
    Write-Host ""
    
    # Open browser
    $openBrowser = Read-Host "Open Swagger in browser? (Y/n)"
    if ($openBrowser -ne "n" -and $openBrowser -ne "N") {
        Start-Process "http://localhost:5231/swagger"
    }
    
} else {
    Write-Host "✗ Failed to start services" -ForegroundColor Red
    Write-Host "Check logs with: docker-compose logs" -ForegroundColor Yellow
    exit 1
}
