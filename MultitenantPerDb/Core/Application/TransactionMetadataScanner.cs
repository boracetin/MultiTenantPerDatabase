using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Domain.Constants;
using MultitenantPerDb.Core.Infrastructure.UnitOfWork.Contract;

namespace MultitenantPerDb.Core.Application;

/// <summary>
/// Scans all MediatR handlers at startup and builds transaction metadata cache
/// ZERO RUNTIME REFLECTION - All analysis done once at startup
/// </summary>
public class TransactionMetadataScanner
{
    private readonly Dictionary<Type, TransactionMetadata> _metadataCache = new();
    
    /// <summary>
    /// Scan all assemblies for MediatR handlers and build metadata cache
    /// Called once at startup
    /// </summary>
    public void ScanHandlers(IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var handlerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Where(t => ImplementsIRequestHandler(t))
                    .ToList();

                foreach (var handlerType in handlerTypes)
                {
                    AnalyzeHandler(handlerType);
                }
            }
            catch
            {
                // Skip assemblies that can't be scanned
            }
        }
    }

    /// <summary>
    /// Analyze a handler and extract transaction requirements
    /// </summary>
    private void AnalyzeHandler(Type handlerType)
    {
        // Find IRequestHandler<TRequest, TResponse> interface
        var handlerInterface = handlerType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && 
                               i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        if (handlerInterface == null)
            return;

        var genericArgs = handlerInterface.GetGenericArguments();
        var requestType = genericArgs[0];

        // Check if request is read-only
        var isReadOnly = typeof(IWithoutTransactional).IsAssignableFrom(requestType);

        if (isReadOnly)
        {
            _metadataCache[requestType] = new TransactionMetadata
            {
                RequestType = requestType,
                IsReadOnly = true,
                RequiredDatabases = new HashSet<DatabaseType>(),
                RequiredDbContextTypes = new HashSet<Type>()
            };
            return;
        }

        // Analyze constructor dependencies to find UnitOfWork usage
        var constructor = handlerType.GetConstructors().FirstOrDefault();
        if (constructor == null)
            return;

        var parameters = constructor.GetParameters();
        var dbContextTypes = new HashSet<Type>();
        var databaseTypes = new HashSet<DatabaseType>();

        foreach (var param in parameters)
        {
            var paramType = param.ParameterType;

            // Check if parameter is IUnitOfWork<TDbContext>
            if (paramType.IsGenericType && 
                paramType.GetGenericTypeDefinition() == typeof(IUnitOfWork<>))
            {
                var dbContextType = paramType.GetGenericArguments()[0];
                dbContextTypes.Add(dbContextType);

                // Get DatabaseType from DbContext
                var databaseType = GetDatabaseType(dbContextType);
                if (databaseType != DatabaseType.None)
                    databaseTypes.Add(databaseType);
            }
        }

        _metadataCache[requestType] = new TransactionMetadata
        {
            RequestType = requestType,
            IsReadOnly = false,
            RequiredDatabases = databaseTypes,
            RequiredDbContextTypes = dbContextTypes
        };
    }

    /// <summary>
    /// Get DatabaseType from DbContext using static property
    /// </summary>
    private static DatabaseType GetDatabaseType(Type dbContextType)
    {
        // Check if DbContext implements ITransactionContext
        if (!typeof(ITransactionContext).IsAssignableFrom(dbContextType))
            return DatabaseType.None;

        try
        {
            // Get static DatabaseType property
            var property = dbContextType.GetProperty("DatabaseType", 
                BindingFlags.Public | BindingFlags.Static);
            
            if (property != null && property.PropertyType == typeof(DatabaseType))
            {
                return (DatabaseType)(property.GetValue(null) ?? DatabaseType.None);
            }
        }
        catch
        {
            // Ignore errors
        }

        return DatabaseType.None;
    }

    /// <summary>
    /// Check if type implements IRequestHandler
    /// </summary>
    private static bool ImplementsIRequestHandler(Type type)
    {
        return type.GetInterfaces()
            .Any(i => i.IsGenericType && 
                     i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
    }

    /// <summary>
    /// Get transaction metadata for a request type
    /// O(1) lookup - no reflection at runtime!
    /// </summary>
    public TransactionMetadata? GetMetadata(Type requestType)
    {
        _metadataCache.TryGetValue(requestType, out var metadata);
        return metadata;
    }

    /// <summary>
    /// Get all cached metadata (for diagnostics)
    /// </summary>
    public IReadOnlyDictionary<Type, TransactionMetadata> GetAllMetadata()
    {
        return _metadataCache;
    }
}
