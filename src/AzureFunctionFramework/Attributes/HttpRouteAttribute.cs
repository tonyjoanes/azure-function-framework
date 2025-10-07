using System;

namespace AzureFunctionFramework.Attributes;

/// <summary>
/// Attribute to specify a custom HTTP route for function endpoints.
/// When applied to a class that inherits from HttpFunctionBase, this attribute
/// allows customization of the HTTP trigger route beyond the default class name.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class HttpRouteAttribute : Attribute
{
    /// <summary>
    /// Gets the custom route template for the HTTP function.
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Initializes a new instance of the HttpRouteAttribute with the specified route template.
    /// </summary>
    /// <param name="route">The route template (e.g., "api/hello/{name}")</param>
    public HttpRouteAttribute(string route)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
    }
}
