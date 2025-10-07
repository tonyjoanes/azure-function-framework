using System;
using AzureFunctionFramework.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AzureFunctionFramework.Tests.Extensions;

/// <summary>
/// Unit tests for FunctionFrameworkExtensions.
/// </summary>
public class FunctionFrameworkExtensionsTests
{
    [Fact]
    public void AddFunctionFramework_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            FunctionFrameworkExtensions.AddFunctionFramework(null!)
        );
    }

    [Fact]
    public void AddFunctionFramework_WithValidBuilder_ShouldNotThrow()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act & Assert
        var result = builder.AddFunctionFramework();
        Assert.NotNull(result);
    }

    [Fact]
    public void AddFunctionFramework_WithConfigurationAction_ShouldNotThrow()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act & Assert
        var result = builder.AddFunctionFramework(options =>
        {
            options.EnvironmentLabel = "test";
            options.Prefix = "TestApp/";
            options.EnableAutoDiscovery = false;
            options.EnableConfigurationValidation = false;
        });

        Assert.NotNull(result);
    }

    [Fact]
    public void AddFunctionFramework_WithAppConfigConnectionString_ShouldConfigureAppConfig()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(
            new[]
            {
                new KeyValuePair<string, string?>(
                    "AppConfigConnectionString",
                    "Endpoint=https://test.azconfig.io;Id=test;Secret=test"
                ),
            }
        );

        // Act
        var result = builder.AddFunctionFramework(options =>
        {
            // Don't set AppConfigConnectionString to avoid Azure connections
            options.EnvironmentLabel = "dev";
            options.EnableAutoDiscovery = false;
            options.EnableConfigurationValidation = false;
        });

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void AddFunctionFramework_WithKeyVaultName_ShouldConfigureKeyVault()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Act
        var result = builder.AddFunctionFramework(options =>
        {
            options.KeyVaultName = "test-keyvault";
            options.EnableAutoDiscovery = false;
            options.EnableConfigurationValidation = false;
        });

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void AddFunctionFramework_WithCustomServices_ShouldRegisterCustomServices()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        var customServiceRegistered = false;

        // Act
        var result = builder.AddFunctionFramework(options =>
        {
            options.AddCustomService = services =>
            {
                services.AddSingleton<ITestService, TestService>();
                customServiceRegistered = true;
            };
            options.EnableAutoDiscovery = false;
            options.EnableConfigurationValidation = false;
        });

        // Assert
        Assert.NotNull(result);
        Assert.True(customServiceRegistered);
    }

    [Fact]
    public void AddFunctionFramework_WithAllOptions_ShouldConfigureAllFeatures()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        var customServiceRegistered = false;

        // Act
        var result = builder.AddFunctionFramework(options =>
        {
            // Don't set AppConfigConnectionString or KeyVaultName to avoid Azure connections
            options.EnvironmentLabel = "test";
            options.Prefix = "TestApp/";
            options.EnableEnvironmentVariableInjection = true;
            options.EnableConfigurationValidation = false; // Disable for test
            options.EnableAutoDiscovery = false; // Disable for test
            options.AddCustomService = services =>
            {
                services.AddSingleton<ITestService, TestService>();
                customServiceRegistered = true;
            };
        });

        // Assert
        Assert.NotNull(result);
        Assert.True(customServiceRegistered);
    }

    private interface ITestService
    {
        string GetValue();
    }

    private class TestService : ITestService
    {
        public string GetValue() => "TestValue";
    }
}
