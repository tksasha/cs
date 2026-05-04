namespace Beetles.Application.Requests;

public abstract record class BitemporalRequest
{
    public DateTimeOffset ValidFrom { get; set; }
}
