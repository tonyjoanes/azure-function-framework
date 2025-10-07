using AzureFunctionFramework;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Standard Functions defaults (adds middleware, logging, etc.)
builder.ConfigureFunctionsWorkerDefaults();

// Your framework setup
builder.AddFunctionFramework(options =>
{
    // Optional: Devs customize here if needed
    options.AppConfigConnectionString = Environment.GetEnvironmentVariable("AppConfigConnection"); // Or pull from secrets
    options.KeyVaultName = "my-keyvault"; // Name of the Key Vault (URI built internally)
    options.EnvironmentLabel = "dev"; // For App Config labels (dev, staging, prod)
    options.Prefix = "MyApp/"; // For key isolation in App Config (e.g., MyApp/TopicName)

    // If they need custom services beyond framework defaults
    options.AddCustomService = services =>
    {
        services.AddSingleton<IMyService, MyServiceImpl>(); // Dev plugs in their own stuff
    };
});

// Optional: Custom config sources (e.g., extra JSON file)
builder.Configuration.AddJsonFile("customsettings.json", optional: true);

// Optional: Application Insights
builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var host = builder.Build();

await host.RunAsync();

// Example services
public interface IMyService
{
    string GetValue();
}

public class MyServiceImpl : IMyService
{
    public string GetValue() => "Hello from custom service!";
}
