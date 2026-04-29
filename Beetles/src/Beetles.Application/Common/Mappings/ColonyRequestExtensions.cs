using Beetles.Application.Requests;
using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Mappings;

internal static class ColonyRequestExtensions
{
    extension(ColonyRequest request)
    {
        public Colony ToEntity()
            => new() { Name = request.Name };
    }
}
