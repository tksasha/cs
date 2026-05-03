namespace Beetles.Domain.Entities;

public class Beetle : IEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }

    public DateTimeOffset RecordedFrom { get; set; }
    public DateTimeOffset RecordedTo { get; set; }
}
