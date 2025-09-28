# Topica

Topica is a lightweight, modular library for managing messages and topics in .NET applications. It provides a unified API for creating, publishing, and subscribing to messages across multiple messaging platforms, including AWS SQS/SNS, Azure Service Bus, Kafka, Pulsar, and RabbitMQ.

## Features

- Unified API for message publishing and subscription
- Support for multiple brokers: AWS, Azure, Kafka, Pulsar, RabbitMQ
- Extensible and modular architecture
- Built-in dependency injection and logging support
- Resilience with Polly integration

## Project Structure

- **Topica**: Core abstractions and utilities (`netstandard2.1`)
- **Topica.Aws**: AWS SQS/SNS implementation
- **Topica.Azure.ServiceBus**: Azure Service Bus implementation
- **Topica.Kafka**: Kafka implementation
- **Topica.Pulsar**: Pulsar implementation
- **Topica.RabbitMq**: RabbitMQ implementation
- **Topica.SharedMessageHandlers**: Shared message handler logic
- **\*.Host**: Example producer/consumer host applications for each platform

## Installation

Add the relevant NuGet packages to your project:

```shell
dotnet add package Topica
dotnet add package Topica.Aws
dotnet add package Topica.Azure.ServiceBus
dotnet add package Topica.Kafka
dotnet add package Topica.Pulsar
dotnet add package Topica.RabbitMq
```

## Quick Start

### 1. Define a Message
Create a message class that implements `ITopicMessage`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Topica;
using Topica.Aws; // or Topica.Azure.ServiceBus, etc.

var services = new ServiceCollection();

services.AddLogging();
services.AddTopica(); // Registers core services

// Register the desired transport
services.AddTopicaAws(options => {
    options.Region = "us-east-1";
    // ...other AWS options
});
```

### 2. Publishing a Message
Publish messages using `ITopicPublisher<T>`.

```csharp
public class MyMessage
{
    public string Content { get; set; }
}

// Inject ITopicPublisher<MyMessage>
public class MyService
{
    private readonly ITopicPublisher<MyMessage> _publisher;

    public MyService(ITopicPublisher<MyMessage> publisher)
    {
        _publisher = publisher;
    }

    public async Task SendAsync()
    {
        var message = new MyMessage { Content = "Hello, World!" };
        await _publisher.PublishAsync(message, topicName: "my-topic");
    }
}
```

### 3. Subscribing to a Topic
Subscribe to a topic by implementing `ITopicMessageHandler<T>`.

```csharp
public class MyMessageHandler : ITopicMessageHandler<MyMessage>
{
    public Task HandleAsync(MyMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Received: {message.Content}");
        return Task.CompletedTask;
    }
}

// Register handler in DI
services.AddScoped<ITopicMessageHandler<MyMessage>, MyMessageHandler>();
```

### 4. Running a Host
Each *.Host project demonstrates a producer or consumer for a specific platform. For example, to run a RabbitMQ topic consumer:

```code
cd src/RabbitMq.Topic.Consumer.Host
dotnet run
```

## Configuration
Configure the messaging options in your `appsettings.json` or through code.

```json
{
  "Topica": {
    "Aws": {
      "Region": "us-east-1",
      "AccessKey": "your-access-key"
    }
  }
}
```

Configure connection settings in appsettings.json as needed.
Configuration
Each transport supports its own configuration options, typically set in appsettings.json or via code. See the respective project for details.
Extending
To add support for a new broker, implement the relevant interfaces from Topica and register your implementation via DI.
License
MIT
<hr/>

## Documentation
For more details, see the individual example host applications and is appsettings.json.