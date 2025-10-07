using System.Reflection;
using AzureFunctionFramework.BaseClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureFunctionFramework.Configuration;

/// <summary>
/// Service responsible for discovering and registering function classes via reflection.
/// </summary>
public sealed class FunctionDiscoveryService
{
    private readonly ILogger<FunctionDiscoveryService> _logger;

    /// <summary>
    /// Initializes a new instance of the FunctionDiscoveryService.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public FunctionDiscoveryService(ILogger<FunctionDiscoveryService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Discovers function classes in the current assembly and registers them with the DI container.
    /// </summary>
    /// <param name="services">The service collection to register functions with.</param>
    /// <param name="assemblies">Optional assemblies to scan. If not provided, scans the current assembly.</param>
    /// <returns>The number of function classes discovered and registered.</returns>
    public int DiscoverAndRegisterFunctions(
        IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var assembliesToScan =
            assemblies.Length > 0
                ? assemblies
                : new[] { Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly() };

        var functionTypes = new List<Type>();

        foreach (var assembly in assembliesToScan)
        {
            if (assembly == null)
                continue;

            var types = GetFunctionTypes(assembly);
            functionTypes.AddRange(types);
        }

        foreach (var functionType in functionTypes)
        {
            services.AddScoped(functionType);
            _logger.LogInformation(
                "Registered function class: {FunctionType}",
                functionType.FullName
            );
        }

        return functionTypes.Count;
    }

    /// <summary>
    /// Gets all types that inherit from function base classes.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>A collection of function types.</returns>
    private IEnumerable<Type> GetFunctionTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes().Where(type => IsFunctionType(type)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to scan assembly {AssemblyName} for function types",
                assembly.FullName
            );
            return Enumerable.Empty<Type>();
        }
    }

    /// <summary>
    /// Determines if a type is a function class that should be registered.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a function class, false otherwise.</returns>
    private static bool IsFunctionType(Type type)
    {
        if (type.IsAbstract || type.IsInterface || !type.IsClass)
            return false;

        // Check if it inherits from HttpFunctionBase
        if (IsSubclassOfGeneric(type, typeof(HttpFunctionBase)))
            return true;

        // Check if it inherits from ServiceBusFunctionBase<T>
        if (IsSubclassOfGeneric(type, typeof(ServiceBusFunctionBase<>)))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a type is a subclass of a generic type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="genericType">The generic base type.</param>
    /// <returns>True if the type is a subclass of the generic type, false otherwise.</returns>
    private static bool IsSubclassOfGeneric(Type type, Type genericType)
    {
        if (type == null || genericType == null)
            return false;

        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == genericType)
                return true;

            currentType = currentType.BaseType;
        }

        return false;
    }
}
