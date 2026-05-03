using Beetles.Application.Common.Interfaces;

namespace Beetles.Application.Requests;

public sealed record class BeetleRequest(
    string Name,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo);
