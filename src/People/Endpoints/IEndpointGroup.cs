namespace People.Endpoints;

public interface IEndpointGroup
{
    static abstract void Map(RouteGroupBuilder builder);
}
