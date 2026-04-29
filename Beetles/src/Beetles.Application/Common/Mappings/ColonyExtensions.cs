using Beetles.Application.Responses;
using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Mappings;

internal static class ColonyExtensions
{
    extension(Colony colony)
    {
        public ColonyResponse ToResponse()
            => new(Id: colony.Id, Name: colony.Name);
    }
}
