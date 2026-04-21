namespace Be.User;

// public static class Endpoints
// {
//     public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
//     {
//         app.MapGet("/users", (IService service, CancellationToken cancellationToken)
//             => service.GetUsers(cancellationToken)).WithName("GetUsers");

//         return app;
//     }
// }

public static class IEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapUserEndpoints()
        {
            app.MapGet("/users", (IService service, CancellationToken cancellationToken)
                => service.GetUsers(cancellationToken)).WithName("GetUsers");

            return app;
        }

    }
}
