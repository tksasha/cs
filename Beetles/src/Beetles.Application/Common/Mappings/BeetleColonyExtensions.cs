using Beetles.Application.Responses;
using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Mappings;

internal static class BeetleColonyExtensions
{
    extension(BeetleColony beetleColony)
    {
        public BeetleColonyResponse ToResponse()
            => new(
                Beetle: beetleColony.Beetle!,
                Colony: beetleColony.Colony!,
                ValidFrom: beetleColony.ValidFrom,
                ValidTo: beetleColony.ValidTo ?? DateTimeOffset.MaxValue,
                RecordedFrom: beetleColony.RecordedFrom,
                RecordedTo: beetleColony.RecordedTo);
    }
}
