namespace Beetles.Application.Exceptions;

public sealed class NotFoundException : Exception, IApplicationException
{
    public int StatusCode => 404;
    public string Title => "Not Found";

    public NotFoundException() : base("The requested resource was not found")
    { }
}
