# Topica

[![Build](https://github.com/rehmanab/topica/actions/workflows/ci_main.yml/badge.svg)](https://github.com/rehmanab/topica/actions/workflows/ci_main.yml)&nbsp;&nbsp;&nbsp;[![Publish Nuget Packages](https://github.com/rehmanab/topica/actions/workflows/ci_main_tag.yml/badge.svg)](https://github.com/rehmanab/topica/actions/workflows/ci_main_tag.yml)

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
- **Topica.SharedMessageHandlers**: Shared messages classes and handlers for all the Host producers & consumers
- **Topica.Web**: An ASP.NET website that hosts Health Checks with a UI

## Description
These are a set of libraries that handle topic, queue messages processing, where you can consume messages from a queue that is subscribed to a topic. 

Consumers can run multiple instances in parallel to split the workload. When using the nuget packages, you only need create the message and handler classes and correct handler class HandleAsync() method will be called where the generic type argument of the handler matches the `Type` property of the `BaseMessage`

After creating a message class and a handler for that class, the subscriber will look for a message handler that implements that message type and execute its Validate then Handle methods.

## Installation

You can view/run the docker compose files in the [docker-scripts](https://github.com/rehmanab/docker-scripts) GitHub repository folder for setting up local instances of the various messaging platforms.


Add the relevant NuGet packages to your project:

```shell
dotnet add package Topica (Required)
dotnet add package Topica.Aws
dotnet add package Topica.Azure.ServiceBus
dotnet add package Topica.Kafka
dotnet add package Topica.Pulsar
dotnet add package Topica.RabbitMq
```

## Quick Start

### 1. Define a Message & Handler
Create a message class that implements `BaseMessage`.

```csharp
public class ButtonClickedMessageV1 : BaseMessage
{
    public string? ButtonText { get; set; }
    public string? ButtonId { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
}
```

Create a handler class that implements `IHandler<ButtonClickedMessageV1>`.

```csharp
public class ButtonClickedMessageHandlerV1(ILogger<ButtonClickedMessageHandlerV1> logger) : IHandler<ButtonClickedMessageV1>
{
    public async Task<bool> HandleAsync(ButtonClickedMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(ButtonClickedMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(ButtonClickedMessageV1 message)
    {
        // Do some validation on the incoming message
        return true;
    }
}
```

### 2. Setup the startup dependencies and configuration using the extension method
Example with AWS SNS Topics

Configure the messaging options in your `appsettings.json` or through code.

```csharp
services.AddAwsTopica(c =>
{
    c.ProfileName = hostSettings.ProfileName;
    c.AccessKey = hostSettings.AccessKey;
    c.SecretKey = hostSettings.SecretKey;
    c.ServiceUrl = hostSettings.ServiceUrl;
    c.RegionEndpoint = hostSettings.RegionEndpoint;
}, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
```
`ClassToReferenceAssembly` is a holding class in the project where your messages & handlers are located, if they are in the same project, you can just use `Program`

Inject an instance of `IAwsTopicCreationBuilder` and use its builder methods to configure the topic and queue properties, this builder will also create the topic and subscribed queues if they dont exist on the source messaging system (AWS in this case). The setup for producer and consumer are similar because they will both independently create the topic and queues if they dont exist. 

To use a builder that does NOT create the Topic and queue, please ensure that they already exist (else producing and consuming will throw an exception) use an instance of `IAwsTopicBuilder`

#### Building a Producer and Consumer (not all are required, only the ones you need)
```csharp
var producer = await builder
    .WithWorkerName(nameof(ButtonClickedMessageV1)) // Just the name of the worker so you can identify it when you log
    .WithTopicName(topicName)
    .WithSubscribedQueues(["topica_web_analytics_queue_sales_v1", "topica_web_analytics_queue_reporting_v1"]) // Topic will publish to these queues
    .WithQueueToSubscribeTo("topica_web_analytics_queue_sales_v1") // Will consume from this queue
    .WithFifoSettings(true, true) // First in First out, do duplication based on message content
    .WithTemporalSettings(30, 0, 345600, 5) // (MessageVisibilityTimeoutSeconds, QueueMessageDelaySeconds, QueueMessageRetentionPeriodSeconds, QueueReceiveMessageWaitTimeSeconds)
    .WithErrorQueueSettings(true, 5) // Create and error queue that is published to after 5 handling errors
    .WithQueueSettings(262144) // QueueMaximumMessageSize KB
    .BuildProducerAsync(cancellationToken);
```

```csharp
var consumer = await builder
    .WithWorkerName(nameof(ButtonClickedMessageV1)) // Just the name of the worker so you can identify it when you log
    .WithTopicName("topica_web_analytics_topic_v1")
    .WithSubscribedQueues(["topica_web_analytics_queue_sales_v1", "topica_web_analytics_queue_reporting_v1"]) // Topic will publish to these queues
    .WithQueueToSubscribeTo("topica_web_analytics_queue_sales_v1") // Will consume from this queue
    .WithFifoSettings(true, true) // First in First out, do duplication based on message content
    .WithTemporalSettings(30, 0, 345600, 5) // (MessageVisibilityTimeoutSeconds, QueueMessageDelaySeconds, QueueMessageRetentionPeriodSeconds, QueueReceiveMessageWaitTimeSeconds)
    .WithErrorQueueSettings(true, 5) // Create and error queue that is published to after 5 handling errors
    .WithQueueSettings(262144) // QueueMaximumMessageSize KB
    .WithConsumeSettings(1, 10) // Number of parallel instances, QueueReceiveMaximumNumberOfMessages
    .BuildConsumerAsync(cancellationToken);
```

### 3. Publishing a Message
Publish messages using `IProducer` that was created above.

```csharp
var message = new ButtonClickedMessageV1
{
    ConversationId = Guid.NewGuid(), 
    EventId = count, 
    EventName = "button.clicked.web.v1", 
    Type = nameof(ButtonClickedMessageV1), // This string value must match the name of the generic type parameter of the Handler when consuming messages. i.e. `IHandler<ButtonClickedMessageV1>`
    MessageGroupId = Guid.NewGuid().ToString()
};

// Add some AWS topic message attributes
var attributes = new Dictionary<string, string>
{
    {"traceparent", "AWS topic" },
    {"tracestate", "AWS topic" },
};

await producer.ProduceAsync(message, attributes, cancellationToken);

```

### 4. Consuming from a Topic or Queue.
Consume messages using `IConsumer` that was created above. Depending on your configuration of .WithConsumerSettings() instance count, those many instances of the worker will be spun up running in parallel allowing multiple different messages to be processed in parallel. The actual platform message system is responsible of passing a single unique message to any one consumer. i.e. if you run an instance count of 5 workers and there are 10 messages on the AWS queue, each worker will process 2 messages each...depending on each message handling time that is.

```csharp
await consumer.ConsumeAsync(cancellationToken);
```

### 5. Running a Host
Each *.Host project demonstrates a producer or consumer for a specific platform. For example, to run a RabbitMQ topic consumer:

```code
cd src/RabbitMq.Topic.Consumer.Host
dotnet run
```

Configure connection settings in appsettings.json as needed.
Each Messaging platform supports its own configuration options, typically set in appsettings.json or via code. See the respective Host project for details.

### 6. Running Topic.Web - Health Checks UI
The Topica.Web project is an ASP.NET Core web application that provides a health check UI for monitoring the status of your messaging services. It uses the AspNetCore.HealthChecks.UI package to display health check results in a user-friendly interface.

The Topica.Web project uses "UserSecrets" to manage sensitive configuration data during local development (i.e. running the project in VS or Rider). When running locally, then you can add your Messaging platform keys in your secret file, using the json from the appsettings.json

To enable a health check for a specific messaging service, change the relevant section to the `HealthCheckSettings` in your `appsettings.json` file. For example, to enable a health check for an AWS SNS Topic:
```json
{
  "HealthCheckSettings": {
    ...
    "AwsTopic": {
      "Name": "AWS SNS Topic",
      "Tag": "Aws",
      "Enabled": true, // Set to true to enable this health check
      "TimeOut": "00:00:15"
    }
    ...
  }
}
```

To run in Docker, then run the following command from the src/ directory:
```bash
docker build -f Topica.Web\Dockerfile -t topica.web:latest .
```
```bash
docker run -d -p 7055:7055 -p 7022:7022 --env ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_URLS=http://*:7022 --name topica.web topica.web:latest
```

If you want to run the https endpoint, then you must create a self-signed certificate and place it in the %USERPROFILE%\\.aspnet\https\ folder with the name aspnetapp.pfx and password "password". You can create a self-signed certificate. Then you can add these environment variables to your Docker run command:
```
-e ASPNETCORE_URLS=https://*:7055;http://*:7022 -e ASPNETCORE_Kestrel__Certificates__Default__Password="password" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v %USERPROFILE%\.aspnet\https:/https/
```

You can then access the health check UI at:

http://localhost:7022/health
or
https://localhost:7055/health

## License

MIT
<hr/>

## Documentation
For more details, see the individual example host applications, and it's appsettings.json files for configuration options.

### Example Consumer Hosts
- [AWS SQS (Queue) Host](src/Aws.Queue.Consumer.Host)
- [AWS SNS (Topic) Host](src/Aws.Topic.Consumer.Host)
- [Azure Service Bus Host](src/Azure.ServiceBus.Topic.Consumer.Host)
- [Kafka Host](src/Kafka.Topic.Consumer.Host)
- [Pulsar Host](src/Pulsar.Topic.Consumer.Host)
- [RabbitMQ Queue Host](src/RabbitMq.Queue.Consumer.Host)
- [RabbitMQ Topic Host](src/RabbitMq.Topic.Consumer.Host)

### Example Producer Hosts
- [AWS SQS (Queue) Host](src/Aws.Queue.Producer.Host)
- [AWS SNS (Topic) Host](src/Aws.Topic.Producer.Host)
- [Azure Service Bus Host](src/Azure.ServiceBus.Topic.Producer.Host)
- [Kafka Host](src/Kafka.Topic.Producer.Host)
- [Pulsar Host](src/Pulsar.Topic.Producer.Host)
- [RabbitMQ Queue Host](src/RabbitMq.Queue.Producer.Host)
- [RabbitMQ Topic Host](src/RabbitMq.Topic.Producer.Host)

### Libraries - Implementations
- [Topica (Core)](src/Topica)
- [Topica AWS](src/Topica.Aws)
- [Topica Azure Service Bus](src/Topica.Azure.ServiceBus)
- [Topica Kafka](src/Topica.Kafka)
- [Topica Pulsar](src/Topica.Pulsar)
- [Topica RabbitMQ](src/Topica.RabbitMq)

### Web (Health Checks UI)
- [Topica Web (ASP.NET Health Check UI)](src/Topica.Web)

### Docker Compose Files
- You can view/run the docker compose files in the [docker-scripts](https://github.com/rehmanab/docker-scripts) GitHub repository folder for setting up local instances of the various messaging platforms.