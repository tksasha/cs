namespace Beetles.Application.Exceptions;

public sealed class ConflictException : Exception, IApplicationException
{
    public int StatusCode => 409;
    public string Title => "Conflict";

    public ConflictException() : base("The resource already exists or conflicts with existing data")
    { }
}
