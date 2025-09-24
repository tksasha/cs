using Books.GraphQl.Types;
using Books.Data.Repositories;
using Books.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<IAuthorRepository, AuthorRepository>()
    .AddSingleton<IAuthorService, AuthorService>()
    .AddSingleton<IBookRepository, BookRepository>()
    .AddLogging(builder => builder.AddConsole())
    .AddGraphQLServer()
    .ModifyOptions(options =>
        {
            options.DefaultBindingBehavior = BindingBehavior.Explicit;
        })
    .AddQueryType<QueryType>();

var app = builder.Build();

app.MapGraphQL();

app.Run();
