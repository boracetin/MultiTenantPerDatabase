# MultitenantPerDb.Analyzers

Custom Roslyn Analyzers for enforcing architectural rules in multi-tenant applications.

## 🎯 Purpose

Provides **compile-time** enforcement of database access patterns to ensure:
- ✅ DbContext is never accessed directly (only through Repository)
- ✅ Controllers use Services, not UnitOfWork
- ✅ Services inherit from BaseService for consistency

## 📦 Analyzers

| ID | Rule | Severity | Description |
|----|------|----------|-------------|
| MTDB001 | Unauthorized DbContext Access | Error | Only `ICanAccessDbContext` types can inject DbContext |
| MTDB002 | Unauthorized UnitOfWork in Controller | Error | Controllers must use Services, not UnitOfWork |
| MTDB003 | Service must inherit from BaseService | Warning | Service classes should extend BaseService<TDbContext> |

## 🚀 Installation

Add to your project's `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\MultitenantPerDb.Analyzers\MultitenantPerDb.Analyzers.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## 📖 Documentation

See [ROSLYN_ANALYZER.md](../MultitenantPerDb/Docs/ROSLYN_ANALYZER.md) for detailed documentation.

## 🏗️ Build

```bash
dotnet build MultitenantPerDb.Analyzers.csproj
```

## ✅ Usage

Simply build your project. Violations will appear as compile errors:

```bash
dotnet build

error MTDB001: Type 'ProductController' cannot access DbContext 'ApplicationDbContext'. 
Only types implementing ICanAccessDbContext are allowed to inject DbContext.
```

## 🛠️ Development

Based on:
- Microsoft.CodeAnalysis.CSharp (Roslyn)
- .NET Standard 2.0 (for IDE compatibility)

## 📄 License

Same as parent project.
