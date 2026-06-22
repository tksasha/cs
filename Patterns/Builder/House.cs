namespace Patterns.Builder;

sealed class House
{
    public required Floor Floor { get; init; }
    public required Wall Wall { get; init; }
    public required Roof Roof { get; init; }
}
