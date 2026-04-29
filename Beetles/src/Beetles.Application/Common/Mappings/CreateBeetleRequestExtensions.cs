using Beetles.Application.Requests;
using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Mappings;

internal static class CreateBeetleRequestExtensions
{
    extension(CreateBeetleRequest request)
    {
        public Beetle ToEntity()
            => new() { Name = request.Name };
    }
}
