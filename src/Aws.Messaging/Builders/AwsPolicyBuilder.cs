using System;
using Amazon.Auth.AccessControlPolicy;
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using Aws.Messaging.Contracts;

namespace Aws.Messaging.Builders
{
    //TODO - support multiple account? arn:aws:sns:aws-region:account-id:topic-name, can use wildcards?
    public class AwsPolicyBuilder : IAwsPolicyBuilder
    {
        public string BuildQueueAllowPolicyForTopicToSendMessage(string queueUrl, string queueArn, string topicArn)
        {
            const Statement.StatementEffect statementEffect = Statement.StatementEffect.Allow;
            var sourceArnWildcard = CreateResourceArnWildcard(topicArn);
            var principals = new[] { Principal.AllUsers };
            var actionIdentifiers = new[] { SQSActionIdentifiers.SendMessage };

            return BuildAwsPolicy(statementEffect, queueArn, sourceArnWildcard, principals, actionIdentifiers).ToJson();
        }

        public Policy BuildAwsPolicy(Statement.StatementEffect statementEffect, string resourceArn, string sourceArn, Principal[] principals, ActionIdentifier[] actionIdentifiers)
        {
            return new Policy()
                .WithStatements(new Statement(statementEffect)
                    .WithResources(new Resource(resourceArn))
                    .WithConditions(ConditionFactory.NewSourceArnCondition(sourceArn))
                    .WithPrincipals(principals)
                    .WithActionIdentifiers(actionIdentifiers));
        }

        public string CreateResourceArnWildcard(string resourceArn)
        {
            if (string.IsNullOrWhiteSpace(resourceArn) ||
                !resourceArn.StartsWith("arn", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ApplicationException($"AwsPolicy: Seems not to be a valid ARN: {resourceArn}");
            }
           
            var index = resourceArn.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);

            if (index > 0)
                resourceArn = resourceArn.Substring(0, index + 1);
            else
                throw new ApplicationException($"AwsPolicy: Seems not to be a valid ARN: {resourceArn}");

            return resourceArn + "*";
        }
    }
}