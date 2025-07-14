using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.PendingCommand.Service;
using Fanex.SimpleMailDelivery;
using Fanex.SiteDbConnectionManager.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddOneBookUmForService(builder.Configuration);
services.AddFanexLogSentry(builder.Configuration);
services.AddFanexSql(builder.Configuration);
services.InternalConfigServices(builder.Configuration);
services.AddSiteDbConnectionManagerServices();
services.AddMailDelivery(builder.Configuration);
services.AddHostedService<ProcessingService>();
services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/healthz");
await app.RunAsync();