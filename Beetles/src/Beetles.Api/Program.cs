using Scalar.AspNetCore;

using Beetles.Infrastructure;
using Beetles.Application;
using Beetles.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapBeetlesEndpoints();

app.Run();
