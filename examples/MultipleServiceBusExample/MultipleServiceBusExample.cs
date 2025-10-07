using AzureFunctionFramework.Attributes;
using AzureFunctionFramework.BaseClasses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MultipleServiceBusExample;

/// <summary>
/// Example showing multiple Service Bus functions with different configurations.
///
/// Service Bus Topics & Subscriptions Explained:
/// • TOPIC = The "channel" where messages are published (like "orders", "inventory", "payments")
/// • SUBSCRIPTION = Your function's "tuner" for the topic (like "processing", "updates")
/// • Each subscription gets its own copy of messages from the topic
///
/// The framework will automatically inject the correct environment variables
/// for each function based on the configuration.
///
/// See docs/SERVICE_BUS_GUIDE.md for complete explanation.
/// </summary>
public class MultipleServiceBusFunctions
{
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

    [ServiceBusTrigger("inventory", "updates", "ServiceBus")]
    public class InventoryUpdateFunction : ServiceBusFunctionBase<InventoryMessage>
    {
        public InventoryUpdateFunction(ILogger<InventoryUpdateFunction> logger)
            : base(logger) { }

        protected override async Task HandleMessage(
            InventoryMessage message,
            FunctionContext context
        )
        {
            Logger.LogInformation(
                "Updating inventory for product {ProductId} with quantity {Quantity}",
                message.ProductId,
                message.Quantity
            );

            // Update inventory here
            await Task.Delay(50); // Simulate processing

            Logger.LogInformation("Inventory updated for product {ProductId}", message.ProductId);
        }
    }

    [ServiceBusTrigger("payments", "processing", "ServiceBus")]
    public class PaymentProcessingFunction : ServiceBusFunctionBase<PaymentMessage>
    {
        public PaymentProcessingFunction(ILogger<PaymentProcessingFunction> logger)
            : base(logger) { }

        protected override async Task HandleMessage(PaymentMessage message, FunctionContext context)
        {
            Logger.LogInformation(
                "Processing payment {PaymentId} for amount {Amount}",
                message.PaymentId,
                message.Amount
            );

            // Process payment here
            await Task.Delay(200); // Simulate processing

            Logger.LogInformation("Payment {PaymentId} processed successfully", message.PaymentId);
        }
    }
}

// Message types
public class OrderMessage
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime OrderDate { get; set; }
}

public class InventoryMessage
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string WarehouseId { get; set; } = string.Empty;
    public DateTime UpdateDate { get; set; }
}

public class PaymentMessage
{
    public string PaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
}
