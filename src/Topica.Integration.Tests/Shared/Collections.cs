using Topica.Integration.Tests.Aws.AwsQueue;
using Topica.Integration.Tests.Aws.AwsTopic;
using Topica.Integration.Tests.Azure.ServiceBusQueue;
using Topica.Integration.Tests.Azure.ServiceBusTopic;
using Topica.Integration.Tests.Kafka;
using Topica.Integration.Tests.Pulsar;
using Topica.Integration.Tests.RabbitMq;
using Xunit;

namespace Topica.Integration.Tests.Shared;

// This class has no code, and is never created. Its purpose is simply to be the place
// to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.

// Ensure this test runs in the same collection (group of tests across the project)
// injecting type T in ICollectionFixture injected once as singleton for all tests in the same collection
// as opposed to a new instance created for each test class, including base classes in the tests.

// Each collection runs in parallel with other collections, but not within the same collection.

// Having multiple collection, SharedFixture is created once for each different collections.
// Each SharedFixture is responsible for setting up resources for all tests in that collection

[CollectionDefinition(nameof(AwsQueueCollection))]
public class AwsQueueCollection : ICollectionFixture<AwsQueueSharedFixture>;

[CollectionDefinition(nameof(AwsTopicCollection))]
public class AwsTopicCollection : ICollectionFixture<AwsTopicSharedFixture>;

[CollectionDefinition(nameof(AzureServiceBusQueueCollection))]
public class AzureServiceBusQueueCollection : ICollectionFixture<AzureServiceBusQueueSharedFixture>;

[CollectionDefinition(nameof(AzureServiceBusTopicCollection))]
public class AzureServiceBusTopicCollection : ICollectionFixture<AzureServiceBusTopicSharedFixture>;

[CollectionDefinition(nameof(KafkaTopicCollection))]
public class KafkaTopicCollection : ICollectionFixture<KafkaTopicSharedFixture>;

[CollectionDefinition(nameof(PulsarTopicCollection))]
public class PulsarTopicCollection : ICollectionFixture<PulsarTopicSharedFixture>;

[CollectionDefinition(nameof(RabbitMqCollection))]
public class RabbitMqCollection : ICollectionFixture<RabbitMqSharedFixture>;