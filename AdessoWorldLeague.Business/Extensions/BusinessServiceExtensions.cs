using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AdessoWorldLeague.Business.Interfaces;
using AdessoWorldLeague.Business.Seed;
using AdessoWorldLeague.Business.Services;
using AdessoWorldLeague.Business.Settings;
using AdessoWorldLeague.Repository.Implementations;
using AdessoWorldLeague.Repository.Interfaces;

namespace AdessoWorldLeague.Business.Extensions;

public static class BusinessServiceExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDrawRepository, DrawRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDrawService, DrawService>();

        services.AddTransient<DatabaseSeeder>();

        return services;
    }
}
