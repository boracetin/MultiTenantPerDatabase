namespace MultitenantPerDb.Core.Application.Abstractions;

/// <summary>
/// Resolves MediatR handler types for requests
/// Pre-scans assemblies at startup for optimal performance
/// Eliminates runtime reflection overhead in TransactionBehavior
/// </summary>
public interface IHandlerTypeResolver
{
    /// <summary>
    /// Gets the handler type for a given request type and response type
    /// Uses pre-built cache from startup assembly scan
    /// </summary>
    /// <param name="requestType">The request type (e.g., CreateProductCommand)</param>
    /// <param name="responseType">The response type (e.g., Product)</param>
    /// <returns>Handler type if found, null otherwise</returns>
    Type? GetHandlerType(Type requestType, Type responseType);

    /// <summary>
    /// Gets constructor parameters for a given handler type
    /// Uses cached reflection data for performance
    /// </summary>
    /// <param name="handlerType">The handler type</param>
    /// <returns>Array of constructor parameters</returns>
    System.Reflection.ParameterInfo[] GetConstructorParameters(Type handlerType);
}
