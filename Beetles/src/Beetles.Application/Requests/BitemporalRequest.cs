namespace Beetles.Application.Requests;

public abstract record class BitemporalRequest
{
    public DateTimeOffset BusinessStart { get; set; }
    public DateTimeOffset? BusinessEnd { get; set; }
}
