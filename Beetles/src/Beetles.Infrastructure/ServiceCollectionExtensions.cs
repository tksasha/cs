using Beetles.Application.Common.Interfaces;
using Beetles.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Beetles.Infrastructure;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) => options
                .UseNpgsql(configuration.GetConnectionString("Development"))
                .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
                .UseSnakeCaseNamingConvention());

            services.AddScoped<IRepository, Repository>();

            return services;
        }
    }
}
