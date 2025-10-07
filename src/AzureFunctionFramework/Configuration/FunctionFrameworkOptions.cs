using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctionFramework.Configuration;

/// <summary>
/// Configuration options for the Azure Function Framework.
/// </summary>
public sealed class FunctionFrameworkOptions
{
    /// <summary>
    /// Gets or sets the Azure App Configuration connection string.
    /// If not provided, will attempt to use default configuration sources.
    /// </summary>
    public string? AppConfigConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the Azure Key Vault name (URI will be built internally).
    /// If not provided, Key Vault integration will be skipped.
    /// </summary>
    public string? KeyVaultName { get; set; }

    /// <summary>
    /// Gets or sets the environment label for App Configuration (e.g., "dev", "staging", "prod").
    /// Defaults to "dev".
    /// </summary>
    public string EnvironmentLabel { get; set; } = "dev";

    /// <summary>
    /// Gets or sets the prefix for configuration keys in App Configuration.
    /// This helps isolate your application's configuration from others.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to enable automatic environment variable injection from configuration.
    /// When enabled, configuration values will be injected as environment variables for binding resolution.
    /// Defaults to true.
    /// </summary>
    public bool EnableEnvironmentVariableInjection { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate required configuration at startup.
    /// When enabled, the framework will fail fast if required configuration is missing.
    /// Defaults to true.
    /// </summary>
    public bool EnableConfigurationValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable automatic function discovery via reflection.
    /// When enabled, the framework will automatically discover and register function classes.
    /// Defaults to true.
    /// </summary>
    public bool EnableAutoDiscovery { get; set; } = true;

    /// <summary>
    /// Gets or sets the action to configure additional services in the DI container.
    /// This allows consumers to register their own services alongside the framework's defaults.
    /// </summary>
    public Action<IServiceCollection>? AddCustomService { get; set; }
}
