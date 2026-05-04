using Beetles.Application.Common.Interfaces;

namespace Beetles.Application.Requests;

public sealed record class BeetleRequest(string Name, DateTimeOffset ValidFrom, int ColonyId);

public sealed record class CorrectBeetleRequest(string Name, int ColonyId);
