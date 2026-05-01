namespace Beetles.Application.Exceptions;

public interface IApplicationException
{
    int StatusCode { get; }
    string Title { get; }
    string Message { get; }
}
