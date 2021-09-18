using Amazon.Auth.AccessControlPolicy;

namespace Aws.Messaging.Contracts
{
    public interface IAwsPolicyBuilder
    {
        string BuildQueueAllowPolicyForTopicToSendMessage(string queueUrl, string queueArn, string topicArn);
        Policy BuildAwsPolicy(Statement.StatementEffect statementEffect, string resourceArn, string sourceArn, Principal[] principals, ActionIdentifier[] actionIdentifiers);
        string CreateResourceArnWildcard(string resourceArn);
    }
}