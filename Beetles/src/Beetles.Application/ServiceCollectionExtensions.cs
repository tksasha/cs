using Beetles.Application.Common.Interfaces;
using Beetles.Application.Requests;
using Beetles.Application.Services;
using Beetles.Application.Validators;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Beetles.Application;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddScoped<IWallService, WallService>();

            services.AddScoped<IValidator<WallRequest>, WallRequestValidator>();

            return services;
        }
    }
}
