using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AzureFunctionFramework.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionFramework.BaseClasses;

/// <summary>
/// Abstract base class for HTTP-triggered Azure Functions.
/// Provides common functionality for HTTP requests including error handling,
/// logging, and route customization via attributes.
/// </summary>
public abstract class HttpFunctionBase
{
    /// <summary>
    /// Gets the logger instance for this function.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the HttpFunctionBase.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected HttpFunctionBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the HTTP request. This method is called by the Azure Functions runtime.
    /// Derived classes should override this method and apply the [HttpTrigger] attribute.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <returns>The HTTP response.</returns>
    public async Task<HttpResponseData> Run(HttpRequestData req)
    {
        try
        {
            Logger.LogInformation(
                "HTTP function {FunctionName} started processing request",
                GetType().Name
            );

            var result = await HandleRequest(req);

            Logger.LogInformation(
                "HTTP function {FunctionName} completed successfully",
                GetType().Name
            );

            return await CreateResponse(req, result);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "HTTP function {FunctionName} failed with exception",
                GetType().Name
            );
            return await CreateErrorResponse(req, ex);
        }
    }

    /// <summary>
    /// Handles the business logic for the HTTP request.
    /// Derived classes must implement this method.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <returns>An action result representing the response.</returns>
    protected abstract Task<IActionResult> HandleRequest(HttpRequestData req);

    /// <summary>
    /// Gets route data from the HTTP request.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="key">The route parameter key.</param>
    /// <returns>The route parameter value, or null if not found.</returns>
    protected string? GetRouteData(HttpRequestData req, string key)
    {
        if (req == null)
            throw new ArgumentNullException(nameof(req));
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Route key cannot be null or empty", nameof(key));

        return req.Query[key];
    }

    /// <summary>
    /// Gets query parameter from the HTTP request.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="key">The query parameter key.</param>
    /// <returns>The query parameter value, or null if not found.</returns>
    protected string? GetQueryParameter(HttpRequestData req, string key)
    {
        if (req == null)
            throw new ArgumentNullException(nameof(req));
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Query parameter key cannot be null or empty", nameof(key));

        return req.Query[key];
    }

    /// <summary>
    /// Deserializes the request body to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="req">The HTTP request data.</param>
    /// <returns>The deserialized object, or default(T) if the body is empty.</returns>
    protected async Task<T?> DeserializeBodyAsync<T>(HttpRequestData req)
    {
        if (req == null)
            throw new ArgumentNullException(nameof(req));

        try
        {
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
                return default;

            return JsonSerializer.Deserialize<T>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (JsonException ex)
        {
            Logger.LogWarning(
                ex,
                "Failed to deserialize request body to type {Type}",
                typeof(T).Name
            );
            return default;
        }
    }

    /// <summary>
    /// Creates an HTTP response from an action result.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="result">The action result.</param>
    /// <returns>The HTTP response data.</returns>
    private async Task<HttpResponseData> CreateResponse(HttpRequestData req, IActionResult result)
    {
        var response = req.CreateResponse();

        switch (result)
        {
            case OkObjectResult okResult:
                response.StatusCode = System.Net.HttpStatusCode.OK;
                if (okResult.Value != null)
                {
                    var json = JsonSerializer.Serialize(okResult.Value);
                    await response.WriteStringAsync(json);
                }
                break;

            case OkResult:
                response.StatusCode = System.Net.HttpStatusCode.OK;
                break;

            case BadRequestObjectResult badRequestResult:
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                if (badRequestResult.Value != null)
                {
                    var json = JsonSerializer.Serialize(badRequestResult.Value);
                    await response.WriteStringAsync(json);
                }
                break;

            case NotFoundResult:
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                break;

            case UnauthorizedResult:
                response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                break;

            case ForbidResult:
                response.StatusCode = System.Net.HttpStatusCode.Forbidden;
                break;

            case ObjectResult objectResult:
                response.StatusCode = (System.Net.HttpStatusCode)(objectResult.StatusCode ?? 200);
                if (objectResult.Value != null)
                {
                    var json = JsonSerializer.Serialize(objectResult.Value);
                    await response.WriteStringAsync(json);
                }
                break;

            default:
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                break;
        }

        return response;
    }

    /// <summary>
    /// Creates an error response for exceptions.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>The HTTP response data.</returns>
    private async Task<HttpResponseData> CreateErrorResponse(
        HttpRequestData req,
        Exception exception
    )
    {
        var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);

        var errorResponse = new
        {
            error = "An internal server error occurred",
            message = exception.Message,
            timestamp = DateTime.UtcNow,
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await response.WriteStringAsync(json);

        return response;
    }
}
