namespace Beetles.Domain.Entities;

public interface IBitemporalEntity : IEntity
{
    DateTimeOffset ValidFrom { get; set; }
    DateTimeOffset? ValidTo { get; set; }

    DateTimeOffset RecordedFrom { get; set; }
    DateTimeOffset RecordedTo { get; set; }
}
