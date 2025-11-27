using Product = Shop.Product;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<Product.IRepository, Product.Repository>()
    .AddSingleton<Product.IService, Product.Service>()
    .AddLogging(builder => builder.AddConsole())
    .AddGraphQLServer()
    .AddQueryType<Product.QueryType>();

var app = builder.Build();

app.MapGraphQL();

app.Run();
