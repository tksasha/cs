using Beetles.Domain.Entities;

namespace Beetles.Application.Responses;

public sealed record class BeetleColonyResponse(
    Beetle Beetle,
    Colony Colony,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    DateTimeOffset RecordedFrom,
    DateTimeOffset RecordedTo);
