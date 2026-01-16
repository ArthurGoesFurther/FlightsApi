using Application.Interfaces;
using Infrastructure.Caching;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=FlightsApiDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();
        // Note: registering IHttpContextAccessor and the current user provider is done in the host project (Program.cs)

        // make IConfiguration available for Application handlers via DI
        services.AddSingleton(configuration);

        return services;
    }
}
