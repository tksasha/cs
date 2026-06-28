using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Options;

namespace Patterns.OptionsPattern;

sealed class ApplicationSettings
{
    [Required]
    public required Uri Uri { get; init; }

    [Required]
    public required string Key { get; init; }
}

sealed class AnotherSettings
{
    public required decimal Discount { get; init; }
}

sealed class Application(
    IOptions<ApplicationSettings> applicationSettings,
    IOptions<AnotherSettings> anotherSettings)
{
    public void Run()
    {
        WriteLine($"Uri = {applicationSettings.Value.Uri}");
        WriteLine($"Key = {applicationSettings.Value.Key}");

        WriteLine($"discount = {anotherSettings.Value.Discount:C}");
    }
}

static class Program
{
    public static void Run()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var services = new ServiceCollection();

        services.AddOptions<ApplicationSettings>()
            .Bind(configuration.GetSection(nameof(ApplicationSettings)))
            .ValidateDataAnnotations();

        var anotherSettings = new AnotherSettings
        {
            Discount = 28.12M,
        };

        services.AddSingleton(Options.Create(anotherSettings));

        services.AddSingleton<Application>();

        var serviceProvider = services.BuildServiceProvider();

        var application = serviceProvider.GetRequiredService<Application>();

        application.Run();
    }
}
