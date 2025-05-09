using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.PendingCommand.Service;
using Fanex.SimpleMailDelivery;
using Fanex.SiteDbConnectionManager.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOneBookUmForService(builder.Configuration);
builder.Services.AddFanexLogSentry(builder.Configuration);
builder.Services.AddFanexSql(builder.Configuration);
builder.Services.InternalConfigServices(builder.Configuration);
builder.Services.AddSiteDbConnectionManagerServices();
builder.Services.AddMailDelivery(builder.Configuration);

builder.Services.AddHostedService<ProcessingService>();

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

await app.RunAsync();