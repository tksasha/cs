namespace Be.User;

public static class IEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapUserEndpoints()
        {
            app.MapGet("/users", (IService service, CancellationToken cancellationToken)
                => service.GetUsersAsync(cancellationToken)).WithName("GetUsers");

            app.MapPost("/users", async (CreateRequest request, IService service, CancellationToken cancellationToken)
                => await service.CreateUserAsync(request, cancellationToken)
                ? Results.Created()
                : Results.StatusCode(500));

            return app;
        }

    }
}
