using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using EFDemo;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder
    .AddConfiguration(configuration.GetSection("Logging"))
    .AddSimpleConsole(options => options.IncludeScopes = true));

services.AddDbContext<DatabaseContext>(options => options
    .UseNpgsql(configuration.GetConnectionString("Development"))
    .UseLoggerFactory(loggerFactory)
    .UseSnakeCaseNamingConvention());

var serviceProvider = services.BuildServiceProvider();

using var context = serviceProvider.GetRequiredService<DatabaseContext>();
