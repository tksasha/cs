namespace Be.Users;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddUserServices()
        {
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IService, Service>();

            return services;
        }
    }
}
