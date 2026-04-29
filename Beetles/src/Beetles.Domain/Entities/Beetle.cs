namespace Beetles.Domain.Entities;

public class Beetle : IEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
