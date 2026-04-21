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
                : Results.StatusCode(500)); // TODO: use Expected

            app.MapGet("/users/{id}", async (int id, IService service, CancellationToken cancellationToken)
                => await service.GetByIdAsync(id, cancellationToken));

            app.MapPatch("/users/{id}",
                async (int id, UpdateRequest request, IService service, CancellationToken cancellationToken)
            => await service.UpdateAsync(id, request, cancellationToken)
                ? Results.Ok()
                : Results.StatusCode(500)); // TODO: use Expected

            return app;
        }
    }
}
