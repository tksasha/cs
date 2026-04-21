using Scalar.AspNetCore;

using Be.User;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddUserServices();

builder.Services.AddHttpLogging();

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
