using Beetles.Domain.Entities;

namespace Beetles.Application.Requests;

public sealed record class WallRequest : BitemporalRequest
{
    public required string Color { get; set; }

    public Wall ToEntity()
        => new() { Color = Color, BusinessStart = BusinessStart };
}
