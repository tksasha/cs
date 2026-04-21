using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;

using Be.User;
using Be;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddUserServices()
    .AddHttpLogging()
    .AddDbContext<DatabaseContext>((serviceProvider, options) => options
        .UseNpgsql(builder.Configuration.GetConnectionString("Development"))
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .UseSnakeCaseNamingConvention());

builder.Logging.AddSimpleConsole(o => o.IncludeScopes = true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseHttpLogging();
app.MapUserEndpoints();

app.Run();
