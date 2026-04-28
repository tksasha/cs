using Scalar.AspNetCore;

using Microsoft.EntityFrameworkCore;

using People;
using People.Repositories;
using People.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<DatabaseContext>((serviceProvider, options) => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Development"))
    .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
    .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<ILocationService, LocationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapEndpoints(typeof(Program).Assembly);

app.Run();
