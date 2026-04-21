using Microsoft.AspNetCore.Http.HttpResults;

namespace Be.Users;

public static class IEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapUserEndpoints()
        {
            var users = app
                .MapGroup("/users")
                .WithTags("Users")
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            users.MapGet("/", Endpoints.GetAllAsync).WithName("GetAllUsers");

            users.MapGet("/{id:int}", Endpoints.GetByIdAsync).WithName("GetUserById");

            users.MapPost("/", Endpoints.CreateAsync).WithName("CreateUser");

            users.MapPatch("/{id:int}", Endpoints.UpdateAsync).WithName("UpdateUser");

            return app;
        }
    }
}

static class Endpoints
{
    public static async Task<Ok<IEnumerable<User>>> GetAllAsync(IService service, CancellationToken cancellationToken)
    {
        var users = await service.GetAllAsync(cancellationToken);

        return TypedResults.Ok(users);
    }

    public static async Task<Results<Ok<User>, NotFound>> GetByIdAsync(
        int id,
        IService service,
        CancellationToken cancellationToken)
    {
        var user = await service.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(user);
    }

    public static async Task<Results<Created, InternalServerError>> CreateAsync(
        CreateRequest request,
        IService service,
        CancellationToken cancellationToken)
    {
        return await service.CreateAsync(request, cancellationToken)
            ? TypedResults.Created()
            : TypedResults.InternalServerError(); // TODO: use Expected
    }

    public static async Task<Results<Ok, InternalServerError>> UpdateAsync(
        int id,
        UpdateRequest request,
        IService service,
        CancellationToken cancellationToken)
    {
        return await service.UpdateAsync(id, request, cancellationToken)
            ? TypedResults.Ok()
            : TypedResults.InternalServerError(); // TODO: use Expected
    }
}
