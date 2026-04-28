using Scalar.AspNetCore;

using Microsoft.EntityFrameworkCore;

using People;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<DatabaseContext>((serviceProvider, options) => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Development"))
    .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
    .UseSnakeCaseNamingConvention());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapEndpoints(typeof(Program).Assembly);

app.Run();
