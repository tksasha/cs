namespace Beetles.Domain.Entities;

public class Colony : IEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
