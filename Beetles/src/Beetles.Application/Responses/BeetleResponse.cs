namespace Beetles.Application.Responses;

public sealed record class BeetleResponse(
    int Id,
    string Name,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo,
    DateTimeOffset RecordedFrom,
    DateTimeOffset RecordedTo);
