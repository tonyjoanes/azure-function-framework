using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureFunctionFramework.Configuration;

/// <summary>
/// Service responsible for validating required configuration at startup.
/// </summary>
public sealed class ConfigurationValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the ConfigurationValidator.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="logger">The logger instance.</param>
    public ConfigurationValidator(
        IConfiguration configuration,
        ILogger<ConfigurationValidator> logger
    )
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates that all required configuration values are present.
    /// </summary>
    /// <param name="options">The framework options containing validation settings.</param>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing.</exception>
    public void ValidateConfiguration(FunctionFrameworkOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (!options.EnableConfigurationValidation)
        {
            _logger.LogInformation("Configuration validation is disabled");
            return;
        }

        var missingConfigurations = new List<string>();

        // Validate App Configuration connection if specified
        if (!string.IsNullOrEmpty(options.AppConfigConnectionString))
        {
            if (!IsValidConnectionString(options.AppConfigConnectionString))
            {
                missingConfigurations.Add(
                    "AppConfigConnectionString is specified but appears to be invalid"
                );
            }
        }

        // Validate Key Vault configuration if specified
        if (!string.IsNullOrEmpty(options.KeyVaultName))
        {
            if (!IsValidKeyVaultName(options.KeyVaultName))
            {
                missingConfigurations.Add("KeyVaultName is specified but appears to be invalid");
            }
        }

        // Validate common Service Bus configuration
        ValidateServiceBusConfiguration(missingConfigurations);

        if (missingConfigurations.Any())
        {
            var errorMessage =
                $"Configuration validation failed. Missing or invalid configurations:\n{string.Join("\n", missingConfigurations)}";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        _logger.LogInformation("Configuration validation completed successfully");
    }

    /// <summary>
    /// Validates Service Bus related configuration.
    /// </summary>
    /// <param name="missingConfigurations">List to add missing configurations to.</param>
    private void ValidateServiceBusConfiguration(List<string> missingConfigurations)
    {
        var serviceBusConnection = _configuration.GetConnectionString("ServiceBus");
        if (string.IsNullOrEmpty(serviceBusConnection))
        {
            // Check if it's in a different format
            var serviceBusConnectionValue = _configuration["ServiceBus:ConnectionString"];
            if (string.IsNullOrEmpty(serviceBusConnectionValue))
            {
                _logger.LogWarning(
                    "ServiceBus connection string not found. Service Bus functions may not work properly."
                );
            }
        }
    }

    /// <summary>
    /// Checks if a connection string appears to be valid.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns>True if the connection string appears valid, false otherwise.</returns>
    private static bool IsValidConnectionString(string connectionString)
    {
        return !string.IsNullOrWhiteSpace(connectionString)
            && connectionString.Length > 10
            && // Minimum reasonable length
            !connectionString.Contains("your-connection-string"); // Common placeholder
    }

    /// <summary>
    /// Checks if a Key Vault name appears to be valid.
    /// </summary>
    /// <param name="keyVaultName">The Key Vault name to validate.</param>
    /// <returns>True if the Key Vault name appears valid, false otherwise.</returns>
    private static bool IsValidKeyVaultName(string keyVaultName)
    {
        return !string.IsNullOrWhiteSpace(keyVaultName)
            && keyVaultName.Length > 3
            && // Minimum reasonable length
            !keyVaultName.Contains("your-keyvault")
            && // Common placeholder
            keyVaultName.All(c => char.IsLetterOrDigit(c) || c == '-'); // Valid Key Vault name characters
    }
}
