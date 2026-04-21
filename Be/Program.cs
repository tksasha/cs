using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<Be.User.IRepository, Be.User.Repository>();
builder.Services.AddScoped<Be.User.IService, Be.User.Service>();

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

app.MapGet("/users", (Be.User.IService service, CancellationToken cancellationToken)
    => service.GetUsers(cancellationToken)).WithName("GetPeople");

app.Run();
