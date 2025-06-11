using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Topica.Messages
{
    public class BaseMessage
    {
        public Guid Id { get; set; }
        public string? Type { get; set; }
        
        public long EventId { get; set; }   
        public string? EventName { get; set; }
        
        public Guid ConversationId { get; set; }
        public DateTime TimeStampUtc { get; set; } = DateTime.UtcNow;
        public string? RaisingComponent { get; set; }
        public string? Version { get; set; }
        public string? SourceIp { get; set; }
        public string? Tenant { get; set; }
        public string? ReceiptReference { get; set; }
        public string? MessageGroupId { get; set; }
        public IReadOnlyDictionary<string, string>? AdditionalProperties { get; set; }
        
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