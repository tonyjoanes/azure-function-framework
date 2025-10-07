using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AzureFunctionFramework.Attributes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionFramework.BaseClasses;

/// <summary>
/// Abstract base class for Service Bus-triggered Azure Functions.
///
/// Service Bus Topics provide a pub/sub messaging pattern:
/// • PUBLISHERS send messages to a TOPIC (like "orders", "inventory", "payments")
/// • SUBSCRIBERS listen to a TOPIC using a SUBSCRIPTION (like "processing", "updates")
/// • Each SUBSCRIPTION gets its own copy of messages from the TOPIC
///
/// Usage:
/// [ServiceBusTrigger("orders", "processing", "ServiceBus")]
/// public class OrderFunction : ServiceBusFunctionBase&lt;OrderMessage&gt;
/// {
///     protected override async Task HandleMessage(OrderMessage message, FunctionContext context)
///     {
///         // Your business logic here
///     }
/// }
///
/// Provides common functionality for Service Bus message processing including error handling,
/// logging, deserialization, and dead lettering on errors.
/// </summary>
/// <typeparam name="TMessage">The type of message to deserialize from JSON.</typeparam>
public abstract class ServiceBusFunctionBase<TMessage>
{
    /// <summary>
    /// Gets the logger instance for this function.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the ServiceBusFunctionBase.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected ServiceBusFunctionBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the Service Bus message. This method is called by the Azure Functions runtime.
    /// Derived classes should override this method and apply the [ServiceBusTrigger] attribute.
    /// </summary>
    /// <param name="message">The Service Bus message.</param>
    /// <param name="context">The function execution context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Run(string message, FunctionContext context)
    {
        try
        {
            Logger.LogInformation(
                "Service Bus function {FunctionName} started processing message",
                GetType().Name
            );

            // Deserialize the message
            var deserializedMessage = await DeserializeMessageAsync(message);
            if (deserializedMessage == null)
            {
                Logger.LogWarning(
                    "Failed to deserialize message in function {FunctionName}",
                    GetType().Name
                );
                throw new InvalidOperationException("Message deserialization failed");
            }

            // Process the message
            await HandleMessage(deserializedMessage, context);

            Logger.LogInformation(
                "Service Bus function {FunctionName} completed successfully",
                GetType().Name
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Service Bus function {FunctionName} failed with exception",
                GetType().Name
            );

            // The message will be automatically dead-lettered by Azure Functions runtime
            // if an exception is thrown and not caught
            throw;
        }
    }

    /// <summary>
    /// Handles the business logic for the Service Bus message.
    /// Derived classes must implement this method.
    /// </summary>
    /// <param name="message">The deserialized message.</param>
    /// <param name="context">The function execution context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task HandleMessage(TMessage message, FunctionContext context);

    /// <summary>
    /// Deserializes the Service Bus message string to the specified type.
    /// </summary>
    /// <param name="message">The message string.</param>
    /// <returns>The deserialized message object, or null if deserialization fails.</returns>
    protected virtual Task<TMessage?> DeserializeMessageAsync(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Logger.LogWarning("Received empty or null message");
            return Task.FromResult<TMessage?>(default);
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            var deserializedMessage = JsonSerializer.Deserialize<TMessage>(message, options);

            Logger.LogDebug(
                "Successfully deserialized message to type {MessageType}",
                typeof(TMessage).Name
            );
            return Task.FromResult(deserializedMessage);
        }
        catch (JsonException ex)
        {
            Logger.LogError(
                ex,
                "Failed to deserialize message to type {MessageType}. Message content: {Message}",
                typeof(TMessage).Name,
                message
            );
            return Task.FromResult<TMessage?>(default);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Unexpected error during message deserialization to type {MessageType}",
                typeof(TMessage).Name
            );
            return Task.FromResult<TMessage?>(default);
        }
    }
}
