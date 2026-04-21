namespace Be.User;

public static class IEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapUserEndpoints()
        {
            app.MapGet("/users", (IService service, CancellationToken cancellationToken)
                => service.GetUsersAsync(cancellationToken)).WithName("GetUsers");

            return app;
        }

    }
}
