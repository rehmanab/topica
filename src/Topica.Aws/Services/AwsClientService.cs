using System;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;

namespace Topica.Aws.Services;

// ReSharper disable once ContextualLoggerProblem - used to accross other classes
public class AwsClientService(ILogger<MessagingPlatform> logger) : IAwsClientService
{
    private const string DefaultEmulatorServiceUrl = "http://localhost:4566";
    private const string DefaultEmulatorRegion = "us-east-1";
    
    public TClient GetAwsClient<TClient, TConfig>(string? profileName = null, string? regionEndpoint = null, string? serviceUrl = null)
        where TClient : AmazonServiceClient
        where TConfig : ClientConfig, new()
    {
        var sharedFile = new SharedCredentialsFile();
        var config = new TConfig();

        if (string.IsNullOrWhiteSpace(profileName))
        {
            var emulatorRegion = string.IsNullOrWhiteSpace(regionEndpoint) ? DefaultEmulatorRegion : regionEndpoint;
		
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(emulatorRegion);
            config.ServiceURL = string.IsNullOrWhiteSpace(serviceUrl) ? DefaultEmulatorServiceUrl : serviceUrl;
            config.AuthenticationRegion = emulatorRegion;

            logger.LogDebug("**** AWS PROFILE: Using Emulator: {ServiceName}; ServiceUrl: {ConfigServiceUrl}; Region: {ConfigAuthenticationRegion}", typeof(TClient).Name, config.ServiceURL, config.AuthenticationRegion);

            var localCredentials = new BasicAWSCredentials("test", "test");

            return (TClient)Activator.CreateInstance(typeof(TClient), localCredentials, config)!;
        }
	
        if(!sharedFile.TryGetProfile(profileName, out var profile))
        {
            throw new Exception($"Profile '{profileName}' not found in AWS credentials/config file.");
        }

        AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);

        if (!string.IsNullOrWhiteSpace(regionEndpoint))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(regionEndpoint);
        }
        else
        {
            config.RegionEndpoint = profile.Region;
        }

        if (!string.IsNullOrWhiteSpace(serviceUrl))
        {
            config.ServiceURL = serviceUrl;
        }
        else if(!string.IsNullOrWhiteSpace(profile.EndpointUrl))
        {
            config.ServiceURL = profile.EndpointUrl;
        }

        var sts = new AmazonSecurityTokenServiceClient(credentials, RegionEndpoint.EUWest1);
        var who = sts.GetCallerIdentityAsync(new GetCallerIdentityRequest()).Result;

        logger.LogDebug("**** AWS PROFILE: Using: {ProfileName} : {ConfigRegionEndpoint} for {ServiceName} as Identity: {WhoArn}", profileName, config.RegionEndpoint, typeof(TClient).Name, who.Arn);

        return (TClient)Activator.CreateInstance(typeof(TClient), credentials, config)!;
    }
}