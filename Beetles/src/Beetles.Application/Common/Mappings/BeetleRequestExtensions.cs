using Beetles.Application.Requests;
using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Mappings;

internal static class BeetleRequestExtensions
{
    extension(BeetleRequest request)
    {
        public Beetle ToEntity()
            => new() { Name = request.Name, ValidFrom = request.ValidFrom, ValidTo = request.ValidTo };
    }
}
