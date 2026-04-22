using Be.Data;

namespace Be.Users;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddUserServices()
        {
            services.AddScoped<IRepository<User>, Repository<User>>();
            services.AddScoped<IService, Service>();

            return services;
        }
    }
}
