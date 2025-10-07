using AzureFunctionFramework.Attributes;
using AzureFunctionFramework.BaseClasses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ExampleFunctionApp.ServiceBusFunctions;

[ServiceBusTrigger("orders", "processing", "ServiceBus")]
public class OrderProcessingFunction : ServiceBusFunctionBase<OrderMessage>
{
    public OrderProcessingFunction(ILogger<OrderProcessingFunction> logger)
        : base(logger) { }

    protected override async Task HandleMessage(OrderMessage message, FunctionContext context)
    {
        Logger.LogInformation(
            "Processing order {OrderId} for customer {CustomerId}",
            message.OrderId,
            message.CustomerId
        );

        // Process the order here
        await Task.Delay(100); // Simulate processing

        Logger.LogInformation("Order {OrderId} processed successfully", message.OrderId);
    }
}

public class OrderMessage
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime OrderDate { get; set; }
}
