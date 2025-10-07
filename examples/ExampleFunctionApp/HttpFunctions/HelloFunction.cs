using AzureFunctionFramework.Attributes;
using AzureFunctionFramework.BaseClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ExampleFunctionApp.HttpFunctions;

[HttpRoute("api/hello/{name}")] // Optional attribute for custom route
public class HelloFunction : HttpFunctionBase
{
    public HelloFunction(ILogger<HelloFunction> logger)
        : base(logger) { }

    protected override async Task<IActionResult> HandleRequest(HttpRequestData req)
    {
        string name = GetQueryParameter(req, "name") ?? "World";
        return new OkObjectResult($"Hello, {name}!");
    }
}
