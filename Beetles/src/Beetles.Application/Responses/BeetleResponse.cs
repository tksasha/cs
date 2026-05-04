using Beetles.Domain.Entities;

namespace Beetles.Application.Responses;

public sealed record class BeetleResponse(
    int Id,
    string Name,
    Colony Colony,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo,
    DateTimeOffset RecordedFrom,
    DateTimeOffset RecordedTo);
