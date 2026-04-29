namespace Beetles.Api.Endpoints;

public static class IEnpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder MapBeetlesEndpoints()
        {
            return builder;
        }
    }
}
