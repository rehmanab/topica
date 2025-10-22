using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Topica.Messages
{
    public class BaseMessage
    {
        /// <summary>
        /// The message identifier, which is a unique identifier for the message.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The type of the message, which is used to find the <see cref="Topica.Contracts.IHandler"/> for this message type.
        /// E.g. if your Message is called "ButtonClickedMessageV1", your handler implementation will inherit <!-- IHandler<ButtonClickedMessageV1> -->
        /// So this property will be set to "ButtonClickedMessageV1" to allow the message processing system to route the message to the correct handler
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// An optional event identifier that represents a specific event or action
        /// within the system. This can be useful for tracking and logging purposes,
        /// especially in event-driven architectures where multiple events may be
        /// processed. It helps in correlating logs and traces related to a particular event
        /// across different components or services
        /// </summary>
        public long EventId { get; set; }

        /// <summary>
        /// An optional event name that provides a human-readable identifier
        /// for the event associated with the message. This can enhance clarity
        /// and understanding when analyzing logs or traces, as it gives context
        /// to what the event represents within the system
        /// </summary>
        public string? EventName { get; set; }

        /// <summary>
        /// ConversationId would be the choice when the interaction spans multiple
        /// requests and responses, and there is a necessity to track the entire conversation
        /// or session as a whole. This is more common in complex workflows where
        /// long-running processes are broken down into multiple messages
        /// across different systems
        /// </summary>
        public Guid ConversationId { get; set; }

        /// <summary>
        /// CorrelationId is particularly useful when the need is to associate a response
        /// directly to a singular request. For example, in a simple query-response model,
        /// where a client needs to know which response corresponds to which request,
        /// using a CorrelationId is appropriate
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// The timestamp indicating when the message was created or sent, represented in UTC
        /// to ensure consistency across different time zones
        /// </summary>
        public DateTime TimeStampUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The component or service that generated or raised the message
        /// </summary>
        public string? RaisingComponent { get; set; }

        /// <summary>
        /// The version of the message schema or format
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// The source IP address from which the message originated
        /// </summary>
        public string? SourceIp { get; set; }

        /// <summary>
        /// The tenant identifier associated with the message, useful in multi-tenant systems
        /// </summary>
        public string? Tenant { get; set; }

        /// <summary>
        /// Optional: A reference identifier for the receipt of the message, which can be used for tracking or acknowledgment purposes,
        /// Some messaging systems provide a receipt or acknowledgment ID when a message is successfully processed.
        /// E.g. AWS SQS & SNS provide and handle their own ReceiptHandle for message deletion and tracking
        /// </summary>
        public string? ReceiptReference { get; set; }

        /// <summary>
        /// An optional identifier used to group related messages together,
        /// ensuring that they are processed in order within the same group. Some Messaging systems,
        /// like AWS SQS FIFO queues, Kafka, utilize MessageGroupId to maintain the order of messages
        /// that belong to the same group
        /// </summary>
        public string? MessageGroupId { get; set; }

        /// <summary>
        /// A dictionary to hold any additional properties or metadata associated with the message.
        /// This allows for flexibility in including extra information that may not be part of the core message
        /// structure but is still relevant for processing or handling the message
        /// </summary>
        public Dictionary<string, string> MessageAdditionalProperties { get; set; } = new();

        /// <summary>
        /// Parses the message body into a specific type of BaseMessage
        /// </summary>
        /// <param name="messageBody">The string representation of any derived BaseMessage</param>
        /// <typeparam name="T">Any BaseMessage derived message type</typeparam>
        /// <returns></returns>
        public static BaseMessage? Parse<T>(string messageBody) where T : BaseMessage
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(messageBody);
            }
            catch
            {
                return null;
            }
        }
    }
}