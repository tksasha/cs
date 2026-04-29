
using Beetles.Application.Common.Interfaces;
using Beetles.Application.Requests;
using Beetles.Application.Responses;
using Beetles.Application.Services;
using Beetles.Application.Validators;
using Beetles.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Beetles.Application;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddScoped<IBeetleService, BeetleService>();

            services.AddScoped<IValidator<BeetleRequest>, BeetleValidator>();

            return services;
        }
    }
}
