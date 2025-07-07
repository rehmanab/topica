using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topica.Messages;

namespace Topica.Contracts;

public interface IProducer
{
    string Source { get; }
    Task ProduceAsync(BaseMessage message, Dictionary<string, string>? attributes, CancellationToken cancellationToken);
    Task ProduceBatchAsync(IEnumerable<BaseMessage> messages, Dictionary<string, string>? attributes, CancellationToken cancellationToken);
    Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken);
    ValueTask DisposeAsync();
}