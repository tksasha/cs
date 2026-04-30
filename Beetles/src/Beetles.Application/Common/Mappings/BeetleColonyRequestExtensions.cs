using Beetles.Application.Requests;
using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Mappings;

internal static class BeetleColonyRequestExtensions
{
    extension(BeetleColonyRequest request)
    {
        public BeetleColony ToEntity()
            => new()
            {
                BeetleId = request.BeetleId,
                ColonyId = request.ColonyId,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
            };
    }
}
