using Fanex.Caching;
using Fanex.Data.Repository;
using Fanex.PendingCommand.Broadcast.Services;
using Fanex.PendingCommand.Services;
using Fanex.PendingCommand.Services.SystemName;
using Fanex.RemotingRef;
using Fanex.Settlement.Converter.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Fanex.PendingCommand.Service;

public static class DependenceInjection
{
    public static void InternalConfigServices(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddSingleton<XmlConvertService, XmlConvertServiceImp>();
        service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        service.AddScoped<ICommandProcessor, BroadcastCommandProcessor>();
        service.AddScoped<ICommandProcessor, AutoPayoutCommandProcessor>();
        service.AddTransient<PendingCommandController, PendingCommandControllerImp>();
        service.AddTransient<IDynamicRepository, DynamicRepository>();
        service.AddTransient<IBroadcastService, BroadcastServiceImp>();
        service.AddTransient<CommandService, CommandServiceImp>();

        service.AddTransient<AnnouncementService, AnnouncementServiceImp>();
        service.AddTransient<MatchService, MatchServiceImp>();
        service.AddTransient<MessageAnnouncementService, MessageAnnouncementServiceImp>();
        service.AddTransient<NotifyService, NotifyServiceImp>();
        service.AddTransient<RecipientMessageService, RecipientMessageServiceImp>();
        service.AddTransient<RefDataService, RefDataServiceImp>();
        service.AddTransient<SendMailService, SendMailServiceImp>();
        service.AddTransient<TicketInfoService, TicketInfoServiceImp>();
        service.AddRedis(configuration);
        service.AddRemotingRef(configuration);
    }

    private static void AddRemotingRef(this IServiceCollection services, IConfiguration configuration)
    {
        var remotingRefUrl = configuration.GetValue("Fanex.RemotingRef.Service.Url", string.Empty);
        RefDataTypeContainer
            .ConnectTo(new Uri(remotingRefUrl))
            .FromApplication("Settlement");
        services.AddSingleton<IRemotingRefService, RemotingRefService>();
        services.AddTransient<RefDataService, RefDataServiceImp>();
    }

    private static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Fanex.Caching.Redis:RedisConfig").Value;
        var applicationName = configuration.GetSection("Fanex.Caching.Redis:ChannelPrefix").Value;
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConfig},ChannelPrefix={applicationName}"));
        services.AddSingleton<ICacheService>(new CacheService());
    }
}