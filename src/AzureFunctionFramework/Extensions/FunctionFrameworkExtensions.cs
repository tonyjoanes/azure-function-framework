using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using AzureFunctionFramework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureFunctionFramework.Extensions;

/// <summary>
/// Extension methods for configuring the Azure Function Framework.
/// </summary>
public static class FunctionFrameworkExtensions
{
    /// <summary>
    /// Adds the Azure Function Framework to the host builder.
    /// This configures App Configuration, Key Vault, environment variable injection,
    /// function discovery, and validation.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configureOptions">Optional configuration action for framework options.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static IHostApplicationBuilder AddFunctionFramework(
        this IHostApplicationBuilder builder,
        Action<FunctionFrameworkOptions>? configureOptions = null
    )
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var options = new FunctionFrameworkOptions();
        configureOptions?.Invoke(options);

        // Configure Azure App Configuration if connection string is provided
        if (!string.IsNullOrEmpty(options.AppConfigConnectionString))
        {
            builder.Configuration.AddAzureAppConfiguration(appConfigOptions =>
            {
                appConfigOptions
                    .Connect(options.AppConfigConnectionString)
                    .Select("*", options.EnvironmentLabel)
                    .Select("*", LabelFilter.Null)
                    .ConfigureKeyVault(kv =>
                    {
                        if (!string.IsNullOrEmpty(options.KeyVaultName))
                        {
                            kv.SetCredential(new DefaultAzureCredential());
                        }
                    });

                // Add prefix filtering if specified
                if (!string.IsNullOrEmpty(options.Prefix))
                {
                    appConfigOptions.Select($"{options.Prefix}*", options.EnvironmentLabel);
                    appConfigOptions.Select($"{options.Prefix}*", LabelFilter.Null);
                }
            });
        }

        // Configure Azure Key Vault if name is provided
        if (!string.IsNullOrEmpty(options.KeyVaultName))
        {
            var keyVaultUri = $"https://{options.KeyVaultName}.vault.azure.net/";
            // Note: KeyVault configuration will be added when the correct method signature is available
            // builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
        }

        // Register framework services
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<FunctionDiscoveryService>();
        builder.Services.AddSingleton<ConfigurationValidator>();

        // Enable environment variable injection if configured
        if (options.EnableEnvironmentVariableInjection)
        {
            InjectEnvironmentVariablesFromConfiguration(builder.Configuration, options);
        }

        // Validate configuration if enabled
        if (options.EnableConfigurationValidation)
        {
            builder.Services.AddHostedService<ConfigurationValidationService>();
        }

        // Enable auto-discovery if configured
        if (options.EnableAutoDiscovery)
        {
            // Discover and register functions immediately during configuration
            DiscoverAndRegisterFunctions(builder.Services);
        }

        // Add custom services if configured
        options.AddCustomService?.Invoke(builder.Services);

        return builder;
    }

    /// <summary>
    /// Discovers and registers function classes in the current assembly.
    /// </summary>
    /// <param name="services">The service collection to register functions with.</param>
    private static void DiscoverAndRegisterFunctions(IServiceCollection services)
    {
        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        if (entryAssembly == null)
            return;

        var functionTypes = entryAssembly.GetTypes().Where(type => IsFunctionType(type)).ToList();

        foreach (var functionType in functionTypes)
        {
            services.AddScoped(functionType);
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
        if (IsSubclassOf(type, typeof(AzureFunctionFramework.BaseClasses.HttpFunctionBase)))
            return true;

        // Check if it inherits from ServiceBusFunctionBase<T>
        if (
            IsSubclassOfGeneric(
                type,
                typeof(AzureFunctionFramework.BaseClasses.ServiceBusFunctionBase<>)
            )
        )
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a type is a subclass of another type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="baseType">The base type.</param>
    /// <returns>True if the type is a subclass of the base type, false otherwise.</returns>
    private static bool IsSubclassOf(Type type, Type baseType)
    {
        if (type == null || baseType == null)
            return false;

        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType == baseType)
                return true;

            currentType = currentType.BaseType;
        }

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

    /// <summary>
    /// Injects configuration values as environment variables for Azure Functions binding resolution.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="options">The framework options.</param>
    private static void InjectEnvironmentVariablesFromConfiguration(
        IConfiguration configuration,
        FunctionFrameworkOptions options
    )
    {
        var environmentVariables = new Dictionary<string, string>();

        // Common Service Bus configuration
        var serviceBusConfig = configuration.GetSection("ServiceBus");
        foreach (var item in serviceBusConfig.GetChildren())
        {
            var value = item.Value;
            if (!string.IsNullOrEmpty(value))
            {
                environmentVariables[$"ServiceBus:{item.Key}"] = value;
            }
        }

        // Function-specific topic and subscription names
        // This allows each function to have its own configuration
        InjectFunctionSpecificServiceBusConfiguration(configuration, environmentVariables);

        // Connection strings
        var serviceBusConnection =
            configuration.GetConnectionString("ServiceBus")
            ?? configuration["ServiceBus:ConnectionString"];
        if (!string.IsNullOrEmpty(serviceBusConnection))
        {
            environmentVariables["ServiceBus"] = serviceBusConnection;
        }

        // Set environment variables
        foreach (var kvp in environmentVariables)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Injects function-specific Service Bus configuration as environment variables.
    /// This allows each function to have its own topic/subscription configuration.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="environmentVariables">The dictionary to add environment variables to.</param>
    private static void InjectFunctionSpecificServiceBusConfiguration(
        IConfiguration configuration,
        Dictionary<string, string> environmentVariables
    )
    {
        // Look for function-specific Service Bus configurations
        var serviceBusFunctions = configuration.GetSection("ServiceBus:Functions");
        foreach (var functionSection in serviceBusFunctions.GetChildren())
        {
            var functionName = functionSection.Key;
            var topicName = functionSection["TopicName"];
            var subscriptionName = functionSection["SubscriptionName"];

            if (!string.IsNullOrEmpty(topicName))
            {
                environmentVariables[$"{functionName}:TopicName"] = topicName;
            }

            if (!string.IsNullOrEmpty(subscriptionName))
            {
                environmentVariables[$"{functionName}:SubscriptionName"] = subscriptionName;
            }
        }

        // Fallback to global configuration if no function-specific config exists
        var globalTopicName = configuration["ServiceBus:TopicName"] ?? configuration["TopicName"];
        var globalSubscriptionName =
            configuration["ServiceBus:SubscriptionName"] ?? configuration["SubscriptionName"];

        if (!string.IsNullOrEmpty(globalTopicName))
        {
            environmentVariables["TopicName"] = globalTopicName;
        }

        if (!string.IsNullOrEmpty(globalSubscriptionName))
        {
            environmentVariables["SubscriptionName"] = globalSubscriptionName;
        }
    }
}

/// <summary>
/// Hosted service for validating configuration at startup.
/// </summary>
internal class ConfigurationValidationService : IHostedService
{
    private readonly ConfigurationValidator _validator;
    private readonly FunctionFrameworkOptions _options;
    private readonly ILogger<ConfigurationValidationService> _logger;

    public ConfigurationValidationService(
        ConfigurationValidator validator,
        FunctionFrameworkOptions options,
        ILogger<ConfigurationValidationService> logger
    )
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _validator.ValidateConfiguration(_options);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Configuration validation failed during startup");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
