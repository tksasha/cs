using Beetles.Api.Endpoints;
using Beetles.Api.Infrastructure;
using Beetles.Application;
using Beetles.Infrastructure;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();

app.MapBeetleEndpoints();
app.MapColonyEndpoints();
app.MapBeetleColonyEndpoints();

app.Run();
