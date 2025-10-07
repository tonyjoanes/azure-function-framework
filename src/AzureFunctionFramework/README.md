# Azure Function Framework

A lightweight NuGet package to simplify Azure Functions development in .NET. It abstracts away boilerplate for common triggers (HTTP, Service Bus), configuration (App Config + Key Vault), and environment variable setup. Devs inherit from base classes, add their logic, and the framework handles the rest—no more config headaches!

## Features
- **Plug-and-Play Triggers**: Base classes for HTTP and Service Bus (extensible for Timer, Blob, etc.).
- **Seamless Config**: Auto-loads from App Config (with labels for dev/staging/prod) and Key Vault for secrets.
- **Env Var Magic**: Pulls topic/subscription names and routes from config, injects as env vars for binding resolution.
- **Validation**: Fail-fast at startup if required configs are missing.
- **Auto-Discovery**: Reflection scans for your derived classes—no manual registration.
- **Minimal Boilerplate**: Devs focus on business logic; framework wires DI, logging, and error handling.

## Quick Start (For Consumers)
1. **Create a New Azure Functions Project** (Target .NET 8+ Isolated Worker):
   ```
   dotnet new func -n MyFunctionApp --model Isolated
   cd MyFunctionApp
   ```
   - Ensure your `.csproj` targets `<TargetFramework>net8.0</TargetFramework>` (or net9.0+).
   - Install core packages if needed: `dotnet add package Microsoft.Azure.Functions.Worker --version 1.23.0` (or latest 2.x+ for enhanced features).

2. **Install the Package** (once published):
   ```
   dotnet add package AzureFunctionFramework
   ```

3. **Update `Program.cs`** (Using Latest `FunctionsApplication.CreateBuilder`):
   ```csharp
   using AzureFunctionFramework;  // Your NuGet namespace
   using Microsoft.Azure.Functions.Worker;
   using Microsoft.Extensions.Hosting;

   var builder = FunctionsApplication.CreateBuilder(args);

   // Standard Functions defaults (adds middleware, logging, etc.)
   builder.ConfigureFunctionsWorkerDefaults();

   // For ASP.NET Core HTTP integration (if using advanced HTTP features):
   // builder.ConfigureFunctionsWebApplication();

   // Your framework setup
   builder.AddFunctionFramework(options =>
   {
       // Optional: Devs customize here if needed
       options.AppConfigConnectionString = Environment.GetEnvironmentVariable("AppConfigConnection");  // Or pull from secrets
       options.KeyVaultName = "my-keyvault";  // Name of the Key Vault (URI built internally)
       options.EnvironmentLabel = "dev";  // For App Config labels (dev, staging, prod)
       options.Prefix = "MyApp/";  // For key isolation in App Config (e.g., MyApp/TopicName)
       
       // If they need custom services beyond framework defaults
       options.AddCustomService(services =>
       {
           services.AddSingleton<IMyService, MyServiceImpl>();  // Dev plugs in their own stuff
       });
   });

   // Optional: Custom config sources (e.g., extra JSON file)
   builder.Configuration.AddJsonFile("customsettings.json", optional: true);

   // Optional: Application Insights
   builder.Services
       .AddApplicationInsightsTelemetryWorkerService()
       .ConfigureFunctionsApplicationInsights();

   // Optional: JSON serialization tweaks
   builder.Services.Configure<Microsoft.Azure.Functions.Worker.Configuration.WorkerOptions>(workerOptions =>
   {
       // e.g., Switch to Newtonsoft if needed
   });

   var host = builder.Build();

   await host.RunAsync();
   ```

4. **Add a Function** (e.g., HTTP):
   ```csharp
   using Microsoft.AspNetCore.Mvc;
   using Microsoft.Azure.Functions.Worker;
   using Microsoft.Azure.Functions.Worker.Http;
   using System.Threading.Tasks;
   using AzureFunctionFramework;

   namespace MyFunctionApp
   {
       [HttpRoute("api/hello/{name}")]  // Optional attribute for custom route
       public class HelloFunction : HttpFunctionBase
       {
           protected override async Task<IActionResult> HandleRequest(HttpRequestData req)
           {
               string name = req.GetRouteData("name");  // Or from query/body
               return new OkObjectResult($"Hello, {name}!");
           }
       }
   }
   ```

5. **Run Locally**:
   ```
   func start
   ```
   Test at `http://localhost:7071/api/hello/World`.

For Service Bus, inherit from `ServiceBusFunctionBase<TMessage>` and override `HandleMessage`.

## Building the Package
This repo builds the NuGet package. Follow these steps to develop and publish. Targets .NET 8+ isolated worker with `FunctionsApplication.CreateBuilder`.

### Prerequisites
- .NET 8 SDK (or later, e.g., .NET 9).
- Azure CLI (for testing App Config/Key Vault locally).
- Cursor (AI coding assistant) for code generation/refactoring.
- Git.
- Azure Functions Core Tools v4.x: `npm install -g azure-functions-core-tools@4 --unsafe-perm true`.

### Project Structure
```
azure-function-framework/
├── src/                    # Source code
│   └── AzureFunctionFramework/
│       ├── BaseClasses/    # HttpFunctionBase.cs, ServiceBusFunctionBase.cs
│       ├── Extensions/     # FunctionFrameworkExtensions.cs
│       ├── Attributes/     # HttpRouteAttribute.cs, ServiceBusTriggerAttribute.cs
│       └── AzureFunctionFramework.csproj  # <TargetFramework>net8.0</TargetFramework>
├── tests/                  # Unit/integration tests
├── README.md
├── AzureFunctionFramework.nuspec  # NuGet metadata
└── Directory.Build.props   # Shared build settings
```

### Step-by-Step Build Guide
1. **Clone and Setup**:
   ```
   git clone <your-repo-url>
   cd azure-function-framework
   dotnet restore
   ```

2. **Use Cursor to Generate/Refine Code**:
   - Open the repo in Cursor (or VS Code with Cursor extension).
   - **Prompt Cursor for Base Classes**: Select `src/AzureFunctionFramework/BaseClasses/` and prompt: "Generate an abstract base class `HttpFunctionBase` for Azure Functions HTTP triggers in .NET 8 isolated worker. Include [HttpTrigger] attribute with dynamic route from attribute, error handling, and abstract `HandleRequest` method. Use ILogger and return IActionResult. Compatible with FunctionsApplication.CreateBuilder."
   - **For Service Bus**: Prompt: "Create `ServiceBusFunctionBase<TMessage>` with [ServiceBusTrigger] using %TopicName%/%SubscriptionName%, deserialize message, and abstract `HandleMessage`. Add dead-lettering on errors. For .NET 8 isolated."
   - **Extensions**: In `Extensions/FunctionFrameworkExtensions.cs`, prompt: "Write an IHostApplicationBuilder extension `AddFunctionFramework` that configures App Config with labels, Key Vault, sets env vars from config (e.g., TopicName), and auto-discovers/registers subclasses via reflection. Include validation for missing configs. Use FunctionsApplication.CreateBuilder style."
   - **Attributes**: Prompt: "Generate [HttpRouteAttribute] and [ServiceBusTriggerAttribute] for customizing routes/topics in derived classes."
   - **Tests**: In `tests/`, prompt: "Write unit tests for HttpFunctionBase using Moq for ILogger and HttpRequestData. Test HandleRequest delegation and error paths. Target .NET 8."

3. **Build and Pack**:
   ```
   dotnet build --configuration Release
   dotnet pack src/AzureFunctionFramework --configuration Release --output ./nupkgs
   ```
   - This creates `AzureFunctionFramework.<version>.nupkg` in `./nupkgs`.

4. **Test the Package Locally**:
   - Create a sample Function app: `dotnet new func -n TestApp --model Isolated`.
   - Add local NuGet source: `dotnet nuget add source ./nupkgs --name LocalFramework`.
   - Install: `cd TestApp && dotnet add package AzureFunctionFramework --version <version> --source ../nupkgs`.
   - Add a sample function (as in Quick Start), run `func start`, and verify.

5. **Integration Testing with Azure Resources**:
   - Create local App Config and Key Vault via Azure CLI:
     ```
     az group create --name rg-test --location eastus
     az appconfig create --name myappconfig --resource-group rg-test
     az keyvault create --name mykeyvault --resource-group rg-test
     ```
   - Set secrets: `az appconfig kv set --connection-string <conn> --key ServiceBus:TopicName --label dev --value mytopic`.
   - Update `TestApp/local.settings.json` with connections.
   - Prompt Cursor: "Update TestApp Program.cs to use the framework with these resources, using FunctionsApplication.CreateBuilder."
   - Run and send a test message (for Service Bus) or HTTP request.

6. **Publish to NuGet**:
   - Update version in `src/AzureFunctionFramework/AzureFunctionFramework.csproj` (e.g., `<Version>1.0.0</Version>`).
   - `dotnet nuget push ./nupkgs/AzureFunctionFramework.1.0.0.nupkg --api-key <your-key> --source https://api.nuget.org/v3/index.json`.
   - (Get API key from nuget.org.)

### Development Tips with Cursor
- **Iterate Fast**: After generating code, prompt: "Refactor this class to add support for custom DI in options.AddCustomService, compatible with IHostApplicationBuilder."
- **Debug Issues**: Select error-prone code and prompt: "Fix this reflection scan to handle generic base classes like ServiceBusFunctionBase<T> in .NET 8 Functions."
- **Enhance**: Prompt: "Add a TimerFunctionBase with cron from config, using latest isolated worker APIs."
- **Docs**: Use Cursor to generate XML comments: "Add full XML docs to HttpFunctionBase methods."

## Architecture Overview
- **Entry Point**: `AddFunctionFramework` extension configures the `IHostApplicationBuilder` (from `FunctionsApplication.CreateBuilder`), loads config, sets env vars, discovers/registers triggers.
- **Triggers**: Base classes with attributes for customization; runtime invokes via Functions host.
- **Config Flow**: App Config (labeled) → Key Vault secrets → Env vars → Binding resolution.
- **Extensibility**: Add new bases in `BaseClasses/` and update `GetTriggerTypes()` reflection.

## Contributing
1. Fork the repo.
2. Create a feature branch (`git checkout -b feature/new-trigger`).
3. Use Cursor to implement (e.g., "Implement Blob trigger base for .NET 8").
4. Add tests, build, and PR.

## License
MIT - see [LICENSE](LICENSE) file.

---

*Built with ❤️ and Cursor AI for rapid prototyping. Updated for .NET 8+ isolated worker with FunctionsApplication.CreateBuilder (as of October 2025).*