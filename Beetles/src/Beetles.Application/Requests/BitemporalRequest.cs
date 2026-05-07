namespace Beetles.Application.Requests;

public abstract record class BitemporalRequest
{
    public DateTimeOffset BusinessStart { get; set; }
}
