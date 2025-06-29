using System.Reflection;
using Topica.Web.Extensions;
using Topica.Web.HealthChecks;
using Topica.Web.Models;
using Topica.Web.Settings;
using static Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;

var builder = WebApplication.CreateBuilder(args);

////////////////////////////////////
// Configure Services

builder.Logging.AddSimpleConsole().AddFilter(level => level >= LogLevel.Information);
// var logger = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddSimpleConsole().AddFilter(level => level >= LogLevel.Information)).CreateLogger("Program");

// Add services to the container.
builder.Services.AddControllersWithViews();

// The tags correspond to the health check groups, which can be used to filter health checks in the UI or when querying the health status. (EndpointRouteBuilderExtensions.cs)
builder.Services.AddHealthCheckServices(config =>
{
    config.HealthCheckTimeoutSeconds = 5;
});

builder.Services.AddHealthChecksUI().AddInMemoryStorage();

// Add MessagingPlatform Components
var awsHostSettings = builder.Configuration.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>() ?? throw new Exception("Could not bind the AWS Host Settings, please check configuration");
if (awsHostSettings == null) throw new InvalidOperationException($"{nameof(AwsHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
builder.Services.AddSingleton(awsHostSettings);
builder.Services.AddAwsTopica(c =>
{
    c.ProfileName = awsHostSettings.ProfileName;
    c.AccessKey = awsHostSettings.AccessKey;
    c.SecretKey = awsHostSettings.SecretKey;
    c.ServiceUrl = awsHostSettings.ServiceUrl;
    c.RegionEndpoint = awsHostSettings.RegionEndpoint;
}, Assembly.GetExecutingAssembly());

var azureServiceBusHostSettings = builder.Configuration.GetSection(AzureServiceBusHostSettings.SectionName).Get<AzureServiceBusHostSettings>() ?? throw new Exception("Could not bind the AzureServiceBusHostSettings Host Settings, please check configuration");
if (azureServiceBusHostSettings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
builder.Services.AddSingleton(azureServiceBusHostSettings);
builder.Services.AddAzureServiceBusTopica(c => { c.ConnectionString = azureServiceBusHostSettings.ConnectionString; }, Assembly.GetExecutingAssembly());

var kafkaHostSettings = builder.Configuration.GetSection(KafkaHostSettings.SectionName).Get<KafkaHostSettings>() ?? throw new Exception("Could not bind the KafkaHostSettings Host Settings, please check configuration");
if (kafkaHostSettings == null) throw new InvalidOperationException($"{nameof(KafkaHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
builder.Services.AddSingleton(kafkaHostSettings);
builder.Services.AddKafkaTopica(c => { c.BootstrapServers = kafkaHostSettings.BootstrapServers; }, Assembly.GetExecutingAssembly());

var pulsarHostSettings = builder.Configuration.GetSection(PulsarHostSettings.SectionName).Get<PulsarHostSettings>() ?? throw new Exception("Could not bind the PulsarHostSettings Host Settings, please check configuration");
if (pulsarHostSettings == null) throw new InvalidOperationException($"{nameof(PulsarHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
builder.Services.AddSingleton(pulsarHostSettings);
builder.Services.AddPulsarTopica(c =>
{
    c.ServiceUrl = pulsarHostSettings.ServiceUrl;
    c.PulsarManagerBaseUrl = pulsarHostSettings.PulsarManagerBaseUrl;
    c.PulsarAdminBaseUrl = pulsarHostSettings.PulsarAdminBaseUrl;
}, Assembly.GetExecutingAssembly());

var rabbitMqHostSettings = builder.Configuration.GetSection(RabbitMqHostSettings.SectionName).Get<RabbitMqHostSettings>() ?? throw new Exception("Could not bind the RabbitMqHostSettings Host Settings, please check configuration");
if (rabbitMqHostSettings == null) throw new InvalidOperationException($"{nameof(RabbitMqHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
builder.Services.AddSingleton(rabbitMqHostSettings);
builder.Services.AddRabbitMqTopica(c =>
{
    c.Hostname = rabbitMqHostSettings.Hostname;
    c.UserName = rabbitMqHostSettings.UserName;
    c.Password = rabbitMqHostSettings.Password;
    c.Scheme = rabbitMqHostSettings.Scheme;
    c.Port = rabbitMqHostSettings.Port;
    c.ManagementPort = rabbitMqHostSettings.ManagementPort;
    c.ManagementScheme = rabbitMqHostSettings.ManagementScheme;
    c.VHost = rabbitMqHostSettings.VHost;
}, Assembly.GetExecutingAssembly());


////////////////////////////////////
// Setup middleware
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Custom Middleware for Health Checks
app.MapCustomHealthCheck(Enum.GetValues<HealthCheckTags>().Select(x => x.ToString()).ToArray());
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health";
    options.ApiPath = "/healthapi";
    options.WebhookPath = "/healthwebhook";
    options.AddCustomStylesheet("HealthChecks/css/healthchecksui.css");
});

app.Run();