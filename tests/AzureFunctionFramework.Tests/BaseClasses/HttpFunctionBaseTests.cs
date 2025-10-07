using System;
using System.Net.Http;
using System.Threading.Tasks;
using AzureFunctionFramework.BaseClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureFunctionFramework.Tests.BaseClasses;

/// <summary>
/// Unit tests for HttpFunctionBase.
/// </summary>
public class HttpFunctionBaseTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly TestHttpFunction _testFunction;

    public HttpFunctionBaseTests()
    {
        _mockLogger = new Mock<ILogger>();
        _testFunction = new TestHttpFunction(_mockLogger.Object);
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
        Assert.Throws<ArgumentNullException>(() => new TestHttpFunction(null!));
    }

    [Fact]
    public void GetQueryParameter_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _testFunction.GetQueryParameterPublic(null!, "testParam")
        );
    }

    [Fact]
    public void GetQueryParameter_WithNullKey_ShouldThrowArgumentException()
    {
        // Arrange
        var mockRequest = CreateMockHttpRequestData();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _testFunction.GetQueryParameterPublic(mockRequest.Object, null!)
        );
    }

    // Note: DeserializeBodyAsync tests are skipped due to complexity of mocking HttpRequestData
    // In a real implementation, these would be integration tests or use a different testing approach

    private Mock<HttpRequestData> CreateMockHttpRequestData()
    {
        var mockFunctionContext = new Mock<FunctionContext>();
        var mockHttpRequestMessage = new Mock<HttpRequestMessage>();

        var mockRequest = new Mock<HttpRequestData>(
            mockFunctionContext.Object,
            mockHttpRequestMessage.Object
        );

        // Note: In a real test, you'd need to properly mock the HttpRequestData
        // This is a simplified version for demonstration

        return mockRequest;
    }

    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    /// <summary>
    /// Test implementation of HttpFunctionBase for testing purposes.
    /// </summary>
    private class TestHttpFunction : HttpFunctionBase
    {
        public TestHttpFunction(ILogger logger)
            : base(logger) { }

        protected override Task<IActionResult> HandleRequest(HttpRequestData req)
        {
            return Task.FromResult<IActionResult>(new OkResult());
        }

        // Public wrappers for testing protected methods
        public string? GetQueryParameterPublic(HttpRequestData req, string key)
        {
            return GetQueryParameter(req, key);
        }

        public async Task<T?> DeserializeBodyAsyncPublic<T>(HttpRequestData req)
        {
            return await DeserializeBodyAsync<T>(req);
        }
    }
}
