using Beetles.Application.Responses;
using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Mappings;

internal static class BeetleExtensions
{
    extension(Beetle beetle)
    {
        public BeetleResponse ToResponse()
            => new(Id: beetle.Id, Name: beetle.Name);
    }
}
