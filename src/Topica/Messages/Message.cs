using System;
using System.Collections.Generic;

namespace Topica.Messages
{
    public class Message
    {
        protected Message()
        {
            TimeStampUtc = DateTime.UtcNow;
            Id = Guid.NewGuid().ToString();
        }

        public virtual string UniqueKey() => Id;

        public string Id { get; set; }
        public string Type { get; set; }
        public Guid ConversationId { get; set; }
        public DateTime TimeStampUtc { get; set; }
        public string RaisingComponent { get; set; }
        public string Version { get; private set; }
        public string SourceIp { get; private set; }
        public string Tenant { get; set; }
        public string ReceiptReference { get; set; }
        public IReadOnlyDictionary<string, string> AdditionalProperties { get; set; }
    }
}
