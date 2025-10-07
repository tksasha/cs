using Books.GraphQl.Types;
using Books.Data.Repositories;
using Books.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<IAuthorRepository, AuthorRepository>()
    .AddSingleton<IAuthorService, AuthorService>()
    .AddSingleton<IBookRepository, BookRepository>()
    .AddSingleton<IBookService, BookService>()
    .AddLogging(builder => builder.AddConsole())
    .AddMemoryCache()
    .AddGraphQLServer()
    .ModifyOptions(options =>
        {
            options.DefaultBindingBehavior = BindingBehavior.Explicit;
        })
    .AddQueryType<QueryType>();

var app = builder.Build();

app.MapGraphQL();

app.Run();
