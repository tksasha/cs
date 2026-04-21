namespace Be.User;

public static class IEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapUserEndpoints()
        {
            app.MapGet("/users", async (IService service, CancellationToken cancellationToken)
                => await service.GetAllAsync(cancellationToken)).WithName("GetUsers");

            app.MapPost("/users", async (CreateRequest request, IService service, CancellationToken cancellationToken)
                => await service.CreateAsync(request, cancellationToken)
                ? Results.Created()
                : Results.StatusCode(500));

            app.MapGet("/users/{id}", async (int id, IService service, CancellationToken cancellationToken)
                => await service.GetByIdAsync(id, cancellationToken));

            return app;
        }
    }
}
