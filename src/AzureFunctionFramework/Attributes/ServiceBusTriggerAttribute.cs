using System;

namespace AzureFunctionFramework.Attributes;

/// <summary>
/// Attribute to specify custom Service Bus topic and subscription names for function triggers.
///
/// Azure Service Bus Topics provide a pub/sub messaging pattern where:
/// • TOPIC = A named message destination (like a radio station)
/// • SUBSCRIPTION = A named listener within a topic (like tuning to that radio station)
/// • CONNECTION = The connection string to your Azure Service Bus namespace
///
/// Example: [ServiceBusTrigger("orders", "processing", "ServiceBus")]
/// • "orders" = Topic name (where messages are published)
/// • "processing" = Subscription name (your function's specific listener)
/// • "ServiceBus" = Connection string name in configuration
///
/// When applied to a class that inherits from ServiceBusFunctionBase&lt;TMessage&gt;, this attribute
/// allows customization of the Service Bus trigger configuration beyond the default configuration values.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ServiceBusTriggerAttribute : Attribute
{
    /// <summary>
    /// Gets the custom topic name for the Service Bus function.
    /// A topic is like a "channel" or "radio station" where messages are published.
    /// Multiple functions can listen to the same topic using different subscriptions.
    /// </summary>
    public string? TopicName { get; }

    /// <summary>
    /// Gets the custom subscription name for the Service Bus function.
    /// A subscription is like "tuning in" to a specific topic. Each subscription
    /// receives its own copy of messages published to the topic.
    /// </summary>
    public string? SubscriptionName { get; }

    /// <summary>
    /// Gets the connection string name for the Service Bus.
    /// This references a connection string in your configuration that contains
    /// the Service Bus namespace URL and authentication credentials.
    /// </summary>
    public string Connection { get; }

    /// <summary>
    /// Initializes a new instance of the ServiceBusTriggerAttribute with the specified topic and subscription.
    /// </summary>
    /// <param name="topicName">The topic name - where messages are published (optional, will use configuration if not specified)</param>
    /// <param name="subscriptionName">The subscription name - your function's specific listener (optional, will use configuration if not specified)</param>
    /// <param name="connection">The connection string name in configuration (defaults to "ServiceBus")</param>
    public ServiceBusTriggerAttribute(
        string? topicName = null,
        string? subscriptionName = null,
        string connection = "ServiceBus"
    )
    {
        TopicName = topicName;
        SubscriptionName = subscriptionName;
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }
}
