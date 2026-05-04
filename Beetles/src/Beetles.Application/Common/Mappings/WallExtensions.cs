using Beetles.Application.Responses;
using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Mappings;

public static class WallExtensions
{
    extension(Wall wall)
    {
        public WallResponse ToResponse() => new()
        {
            Id = wall.Id,
            Color = wall.Color,
            ValidFrom = wall.ValidFrom,
            ValidTo = wall.ValidTo,
            RecordedFrom = wall.RecordedFrom,
            RecordedTo = wall.RecordedTo,
        };
    }
}
