using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topica.Messages;

namespace Topica.Contracts;

public interface IProducer
{
    Task ProduceAsync(string source, BaseMessage message, Dictionary<string, string>? attributes = null, CancellationToken cancellationToken = default);
    Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken);
    ValueTask DisposeAsync();
}