# 🐳 Docker Setup Guide (Application Only)

Complete guide to run Multi-Tenant Per Database application in Docker container while using SQL Server on host machine.

## 📋 Prerequisites

- **Docker Desktop** 4.20+ (Windows/Mac) or **Docker Engine** 20.10+ (Linux)
- **SQL Server** 2019+ running on **localhost:1433**
- **At least 2GB RAM** allocated to Docker
- **5GB disk space** for Docker images

## 🗄️ SQL Server Setup

SQL Server must be running on your host machine at **localhost:1433**

**Connection details:**
- Server: localhost
- Port: 1433
- User: sa
- Password: YourStrong@Passw0rd123

## 🚀 Quick Start

```powershell
# Windows
cd C:\Users\borac\Desktop\DemoCodes\MultiTenantPerDatabase
.\start.ps1
```

## 🌐 Access Points

- **API**: http://localhost:5231
- **Swagger**: http://localhost:5231/swagger
- **Health**: http://localhost:5231/health

Perfect! Docker configuration tamam. Şimdi test edelim! 🚀
