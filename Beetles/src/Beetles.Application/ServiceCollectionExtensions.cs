
using Beetles.Application.Common.Interfaces;
using Beetles.Application.Services;

using Microsoft.Extensions.DependencyInjection;

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
