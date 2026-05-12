namespace Beetles.Application.Requests;

public abstract record class BitemporalRequest
{
    public DateTimeOffset DateTime { get; set; }
}
