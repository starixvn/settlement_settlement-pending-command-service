using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Fanex.PendingCommand.Services;
using Fanex.PendingCommand.Services.SystemName;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Fanex.PendingCommand.Service;

public static class DependenceInjection
{
    public static void InternalConfigServices(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        service.AddTransient<IDynamicRepository, DynamicRepository>();
        service.AddSingleton<ICacheService, CacheService>();
        service.AddTransient<IBroadcastService, BroadcastServiceImp>();
        service.AddTransient<CommandService, CommandServiceImp>();
        service.AddScoped<ICommandProcessor, BroadcastCommandProcessor>();
        service.AddScoped<ICommandProcessor, AutoPayoutCommandProcessor>();
        service.AddTransient<PendingCommandController, PendingCommandControllerImp>();

        service.AddRedis(configuration);
    }

    private static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Fanex.Caching.Redis:RedisConfig").Value;
        var applicationName = configuration.GetSection("Fanex.Caching.Redis:ChannelPrefix").Value;
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConfig},ChannelPrefix={applicationName}"));
        services.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = redisConfig,
            ApplicationName = applicationName
        }));
    }
}