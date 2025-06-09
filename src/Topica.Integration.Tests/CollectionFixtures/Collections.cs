using Topica.Integration.Tests.AwsQueue;
using Xunit;

namespace Topica.Integration.Tests.CollectionFixtures;

// This class has no code, and is never created. Its purpose is simply to be the place
// to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.

[CollectionDefinition(nameof(AwsQueueCollection))]
public class AwsQueueCollection : ICollectionFixture<SharedFixture> { }

[CollectionDefinition(nameof(AwsTopicCollection))]
public class AwsTopicCollection : ICollectionFixture<SharedFixture> { }