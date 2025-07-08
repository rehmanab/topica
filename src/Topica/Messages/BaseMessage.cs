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
        /// The type of the message, which is used to find the <see cref="IHandler"/> for this message type.
        /// </summary>
        public string? Type { get; set; }
        
        public long EventId { get; set; }   
        public string? EventName { get; set; }
        
        /// <summary>
        /// Groups related messages or interactions within a specific service or application.
        /// In an e-commerce application, a Conversation ID might be used to group all the messages related to a specific customer's order, from initial product selection to payment and shipping. 
        /// </summary>
        public Guid ConversationId { get; set; }
        
        public DateTime TimeStampUtc { get; set; } = DateTime.UtcNow;
        public string? RaisingComponent { get; set; }
        public string? Version { get; set; }
        public string? SourceIp { get; set; }
        public string? Tenant { get; set; }
        public string? ReceiptReference { get; set; }
        public string? MessageGroupId { get; set; }
        public Dictionary<string, string> MessageAdditionalProperties { get; set; } = new();
        
        public static BaseMessage? Parse<T>(string messageBody) where T: BaseMessage
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