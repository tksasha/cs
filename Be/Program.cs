using Scalar.AspNetCore;

using Be.User;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddUserServices();

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

app.MapUserEndpoints();

app.Run();
