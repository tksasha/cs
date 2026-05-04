namespace Beetles.Application.Responses;

public sealed record class WallResponse : BiTemporalResponse
{
    public required string Color { get; set; }
}
