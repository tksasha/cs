namespace Beetles.Application.Requests;

public sealed record class BeetleColonyRequest(
    int BeetleId,
    int ColonyId,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo);
