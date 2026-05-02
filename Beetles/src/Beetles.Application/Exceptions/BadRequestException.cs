namespace Beetles.Application.Exceptions;

public sealed class BadRequestException : Exception, IApplicationException
{
    public int StatusCode => 400;
    public string Title => "Bad Request";

    public BadRequestException() : base("The request is invalid or malformed")
    { }
}
