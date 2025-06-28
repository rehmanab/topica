using System.Reflection;
using Topica.Web.Extensions;
using Topica.Web.HealthChecks;
using Topica.Web.Settings;
using static Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;

var builder = WebApplication.CreateBuilder(args);

////////////////////////////////////
// Configure Services

builder.Logging.AddSimpleConsole().AddFilter(level => level >= LogLevel.Information);
// var logger = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddSimpleConsole().AddFilter(level => level >= LogLevel.Information)).CreateLogger("Program");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Health Checks and Health Checks UI (https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
builder.Services.AddHealthChecks()
    .AddCheck("Topica Platform Running", () => Healthy(), tags: ["health", "local"])
    .AddCheck<AwsQueueHealthCheck>("Aws Queue (Publish)", timeout: TimeSpan.FromSeconds(10), tags: new List<string> { "readiness", "external", "aws", "queue" })
    .AddCheck<AwsTopicHealthCheck>("Aws Topic (Publish)", timeout: TimeSpan.FromSeconds(10000), tags: new List<string> { "readiness", "external", "aws", "topic" })
    .AddUrlGroup(new Uri("https://httpbin.org/status/200"), name: "https connection check", tags: new List<string> { "readiness", "external", "https", "internet" });
        
builder.Services.AddHealthChecksUI().AddInMemoryStorage();

// Add MessagingPlatform Components
var awsHostSettings = builder.Configuration.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>() ?? throw new Exception("Could not bind the AWS Host Settings, please check configuration");;
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

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Custom Middleware for Health Checks
app.MapCustomHealthCheck();
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health";
    options.ApiPath = "/healthapi";
    options.WebhookPath = "/healthwebhook";
    options.AddCustomStylesheet("HealthChecks/css/healthchecksui.css");
});

app.Run();