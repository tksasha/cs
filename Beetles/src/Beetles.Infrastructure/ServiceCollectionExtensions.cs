using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Beetles.Application.Common.Interfaces;
using Beetles.Infrastructure.Repositories;

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
