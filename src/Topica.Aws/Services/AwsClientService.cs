using System;
using System.Linq;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Logging;
using Topica.Aws.Configuration;
using Topica.Aws.Contracts;

namespace Topica.Aws.Services;

public class AwsClientService(ILogger<MessagingPlatform> logger) : IAwsClientService
{
    public IAmazonSQS GetSqsClient(AwsTopicaConfiguration awsSettings)
    {
        var sharedFile = new SharedCredentialsFile();
        var config = new AmazonSQSConfig();
        
        // Get from Profile name in credentials file
        if (!string.IsNullOrWhiteSpace(awsSettings.ProfileName))
        {
            var tryGetProfile = sharedFile.TryGetProfile(awsSettings.ProfileName, out var profile);
            if(!tryGetProfile) throw new ApplicationException($"No AWS profile found with name: {awsSettings.ProfileName}");
        
            if (AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials))
            {
                var regionEndpoint = GetRegionEndpoint(awsSettings.ProfileName, awsSettings.RegionEndpoint, profile, config.RegionEndpoint);
                var serviceUrl = string.IsNullOrWhiteSpace(awsSettings.ServiceUrl) ? profile.EndpointUrl ?? null : awsSettings.ServiceUrl;
                logger.LogDebug("**** AWS PROFILE: using {AwsSettingsProfileName} - REGION: {Region}{ServiceUrl} for {ClientName}", awsSettings.ProfileName, regionEndpoint.SystemName, !string.IsNullOrWhiteSpace(serviceUrl) ? $" - ServiceUrl: {serviceUrl}" : "", nameof(AmazonSQSClient));
            
                config.RegionEndpoint = regionEndpoint;
                if(!string.IsNullOrWhiteSpace(serviceUrl)) config.ServiceURL = serviceUrl;
            
                return new AmazonSQSClient(credentials, config);
            }
        }
        
        // Get from Access key & Secret from local appsettings.json -> AwsHostSettings
        if (!string.IsNullOrWhiteSpace(awsSettings.AccessKey) && !string.IsNullOrWhiteSpace(awsSettings.SecretKey))
        {
            var regionEndpoint = GetRegionEndpoint("awsSettings", awsSettings.RegionEndpoint, null, config.RegionEndpoint);
            logger.LogDebug("**** AWS PROFILE: Using AwsAccessKeyId & AwsSecretAccessKey from appsettings.json for Region: {Region} {ServiceUrl} for {GetSqsClientName}", regionEndpoint, !string.IsNullOrWhiteSpace(awsSettings.ServiceUrl) ? $" - ServiceUrl: {awsSettings.ServiceUrl}" : "", nameof(AmazonSQSClient));

            config.RegionEndpoint = regionEndpoint;
            if (!string.IsNullOrEmpty(awsSettings.ServiceUrl)) config.ServiceURL = awsSettings.ServiceUrl;
            
            return new AmazonSQSClient(new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey), config);
        }
            
        // Get from LocalStack
        if (string.IsNullOrEmpty(awsSettings.ServiceUrl))
        {
            throw new Exception($"Please set the ServiceUrl to use localstack for {nameof(AmazonSQSClient)}");
        }
        logger.LogDebug($"**** AWS PROFILE: Using LOCALSTACK for {nameof(AmazonSQSClient)}");
        config.ServiceURL = awsSettings.ServiceUrl;
        return new AmazonSQSClient(new BasicAWSCredentials("", ""), config);
    }

    public IAmazonSimpleNotificationService GetSnsClient(AwsTopicaConfiguration awsSettings)
    {
        var sharedFile = new SharedCredentialsFile();
        var config = new AmazonSimpleNotificationServiceConfig();
        
        // Get from Profile name in credentials file
        if (!string.IsNullOrWhiteSpace(awsSettings.ProfileName))
        {
            var tryGetProfile = sharedFile.TryGetProfile(awsSettings.ProfileName, out var profile);
            if(!tryGetProfile) throw new ApplicationException($"No AWS profile found with name: {awsSettings.ProfileName}");
        
            if (AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials))
            {
                var regionEndpoint = GetRegionEndpoint(awsSettings.ProfileName, awsSettings.RegionEndpoint, profile, config.RegionEndpoint);
                var serviceUrl = string.IsNullOrWhiteSpace(awsSettings.ServiceUrl) ? profile.EndpointUrl ?? null : awsSettings.ServiceUrl;
                logger.LogDebug("**** AWS PROFILE: using {AwsSettingsProfileName} - REGION: {Region}{ServiceUrl} for {ClientName}", awsSettings.ProfileName, regionEndpoint.SystemName, !string.IsNullOrWhiteSpace(serviceUrl) ? $" - ServiceUrl: {serviceUrl}" : "", nameof(AmazonSimpleNotificationServiceClient));
            
                config.RegionEndpoint = regionEndpoint;
                if(!string.IsNullOrWhiteSpace(serviceUrl)) config.ServiceURL = serviceUrl;
            
                return new AmazonSimpleNotificationServiceClient(credentials, config);
            }
        }

        // Get from Access key & Secret from local appsettings.json -> AwsHostSettings
        if (!string.IsNullOrWhiteSpace(awsSettings.AccessKey) && !string.IsNullOrWhiteSpace(awsSettings.SecretKey))
        {
            var regionEndpoint = GetRegionEndpoint("awsSettings", awsSettings.RegionEndpoint, null, config.RegionEndpoint);
            logger.LogDebug("**** AWS PROFILE: Using AwsAccessKeyId & AwsSecretAccessKey from appsettings.json for Region: {Region} {ServiceUrl} for {GetSqsClientName}", regionEndpoint, !string.IsNullOrWhiteSpace(awsSettings.ServiceUrl) ? $" - ServiceUrl: {awsSettings.ServiceUrl}" : "", nameof(AmazonSimpleNotificationServiceClient));

            config.RegionEndpoint = regionEndpoint;
            if (!string.IsNullOrEmpty(awsSettings.ServiceUrl)) config.ServiceURL = awsSettings.ServiceUrl;
            
            return new AmazonSimpleNotificationServiceClient(new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey), config);
        }
            
        // Get from LocalStack
        if (string.IsNullOrEmpty(awsSettings.ServiceUrl))
        {
            throw new Exception($"Please set the ServiceUrl to use localstack for {nameof(AmazonSimpleNotificationServiceClient)}");
        }
        logger.LogDebug($"**** AWS PROFILE: Using LOCALSTACK for {nameof(AmazonSimpleNotificationServiceClient)}");
        config.ServiceURL = awsSettings.ServiceUrl;
        return new AmazonSimpleNotificationServiceClient(new BasicAWSCredentials("", ""), config);
    }

    private RegionEndpoint GetRegionEndpoint(string profileName, string? regionEndpointSystemName, CredentialProfile? profile, RegionEndpoint defaultRegionEndpoint)
    {
        RegionEndpoint regionEndpoint;
        if (!string.IsNullOrWhiteSpace(regionEndpointSystemName))
        {
            var lookupRegionEndpoint = RegionEndpoint.EnumerableAllRegions.FirstOrDefault(x => x.SystemName.Equals(regionEndpointSystemName, StringComparison.OrdinalIgnoreCase));
            regionEndpoint = lookupRegionEndpoint ?? throw new ApplicationException($"RegionEndpoint with SystemName '{regionEndpointSystemName}' not found in AWS SDK. Please check your configuration.");
        }
        else if(profile is { Region: not null })
        {
            var lookupRegionEndpoint = RegionEndpoint.EnumerableAllRegions.FirstOrDefault(x => x.SystemName.Equals(profile.Region.SystemName, StringComparison.OrdinalIgnoreCase) && x.DisplayName != "Unknown");
            regionEndpoint = lookupRegionEndpoint ?? throw new ApplicationException($"RegionEndpoint with SystemName '{profile.Region.SystemName}' not found in AWS SDK. Please check your configuration.");
        }
        else
        {
            regionEndpoint = defaultRegionEndpoint;
            logger.LogWarning("**** NO AWS profile Region found for {AwsSettingsProfileName}. Using Default Region: {DefaultRegion}", profileName, defaultRegionEndpoint.SystemName);
        } 
        
        return regionEndpoint;
    }
}