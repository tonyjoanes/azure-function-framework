using System;
using System.Threading.Tasks;
using AzureFunctionFramework.BaseClasses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureFunctionFramework.Tests.BaseClasses;

/// <summary>
/// Unit tests for ServiceBusFunctionBase.
/// </summary>
public class ServiceBusFunctionBaseTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly TestServiceBusFunction _testFunction;

    public ServiceBusFunctionBaseTests()
    {
        _mockLogger = new Mock<ILogger>();
        _testFunction = new TestServiceBusFunction(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldNotThrow()
    {
        // Act & Assert
        Assert.NotNull(_testFunction);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestServiceBusFunction(null!));
    }

    [Fact]
    public async Task DeserializeMessageAsync_WithValidJson_ShouldReturnDeserializedObject()
    {
        // Arrange
        var testMessage = new TestMessage
        {
            Id = 1,
            Name = "Test",
            Value = 42.5,
        };
        var json = System.Text.Json.JsonSerializer.Serialize(testMessage);

        // Act
        var result = await _testFunction.DeserializeMessageAsyncPublic(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal(42.5, result.Value);
    }

    [Fact]
    public async Task DeserializeMessageAsync_WithEmptyMessage_ShouldReturnDefault()
    {
        // Act
        var result = await _testFunction.DeserializeMessageAsyncPublic(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeserializeMessageAsync_WithNullMessage_ShouldReturnDefault()
    {
        // Act
        var result = await _testFunction.DeserializeMessageAsyncPublic(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeserializeMessageAsync_WithInvalidJson_ShouldReturnDefault()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var result = await _testFunction.DeserializeMessageAsyncPublic(invalidJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeserializeMessageAsync_WithCaseInsensitiveProperties_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = "{\"id\": 1, \"NAME\": \"Test\", \"value\": 42.5}";

        // Act
        var result = await _testFunction.DeserializeMessageAsyncPublic(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal(42.5, result.Value);
    }

    private class TestMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    /// <summary>
    /// Test implementation of ServiceBusFunctionBase for testing purposes.
    /// </summary>
    private class TestServiceBusFunction : ServiceBusFunctionBase<TestMessage>
    {
        public TestServiceBusFunction(ILogger logger)
            : base(logger) { }

        protected override Task HandleMessage(TestMessage message, FunctionContext context)
        {
            return Task.CompletedTask;
        }

        // Public wrapper for testing protected method
        public async Task<TestMessage?> DeserializeMessageAsyncPublic(string message)
        {
            return await DeserializeMessageAsync(message);
        }
    }
}
