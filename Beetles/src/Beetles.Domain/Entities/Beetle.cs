namespace Beetles.Domain.Entities;

public class Beetle : BitemporalEntity
{
    public required string Name { get; set; }

    public Colony? Colony { get; set; }
}
