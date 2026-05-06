namespace Beetles.Application.Responses;

public abstract record class BiTemporalResponse
{
    public int Id { get; set; }
    public DateTimeOffset BusinessStart { get; set; }
    public DateTimeOffset? BusinessEnd { get; set; }
    public DateTimeOffset SystemStart { get; set; }
    public DateTimeOffset SystemEnd { get; set; }
}
