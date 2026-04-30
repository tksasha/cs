using Beetles.Api.Endpoints;
using Beetles.Application;
using Beetles.Infrastructure;

using Scalar.AspNetCore;

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

app.MapBeetleEndpoints();
app.MapColonyEndpoints();
app.MapBeetleColonyEndpoints();

app.Run();
