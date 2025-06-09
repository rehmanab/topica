using Xunit.Abstractions;

namespace Topica.Integration.Tests.AwsQueue;

public class BaseTest(SharedFixture sharedFixture, ITestOutputHelper testOutputHelper)
{
    protected SharedFixture SharedFixture { get; } = sharedFixture;
    protected ITestOutputHelper Logger { get; } = testOutputHelper;
}