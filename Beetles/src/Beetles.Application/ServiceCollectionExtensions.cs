
using Microsoft.Extensions.DependencyInjection;

using Beetles.Application.Common.Interfaces;
using Beetles.Application.Services;

namespace Beetles.Application;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddScoped<IBeetleService, BeetleService>();

            return services;
        }
    }
}
