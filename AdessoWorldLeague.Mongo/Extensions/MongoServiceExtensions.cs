using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AdessoWorldLeague.Mongo.Context;
using AdessoWorldLeague.Mongo.Settings;

namespace AdessoWorldLeague.Mongo.Extensions;

public static class MongoServiceExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoSettings>(configuration.GetSection(nameof(MongoSettings)));
        services.AddSingleton<IMongoContext, MongoContext>();
        return services;
    }
}
