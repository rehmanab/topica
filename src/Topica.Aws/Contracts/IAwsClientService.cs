using Amazon.Runtime;

namespace Topica.Aws.Contracts;

public interface IAwsClientService
{
    TClient GetAwsClient<TClient, TConfig>(string? profileName = null, string? regionEndpoint = null, string? serviceUrl = null)
        where TClient : AmazonServiceClient
        where TConfig : ClientConfig, new();
}