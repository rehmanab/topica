using Amazon.Auth.AccessControlPolicy;

namespace Topica.Aws.Contracts
{
    public interface IAwsPolicyBuilder
    {
        string BuildQueueAllowPolicyForTopicToSendMessage(string queueUrl, string queueArn, string topicArn);
        Policy BuildAwsPolicy(Statement.StatementEffect statementEffect, string resourceArn, string sourceArn, Principal[] principals, ActionIdentifier[] actionIdentifiers);
        string CreateResourceArnWildcard(string resourceArn);
    }
}