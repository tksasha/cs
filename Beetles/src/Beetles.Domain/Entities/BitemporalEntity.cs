namespace Beetles.Domain.Entities;

public abstract class BitemporalEntity : Entity, ICloneable
{
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }

    public DateTimeOffset RecordedFrom { get; set; }
    public DateTimeOffset RecordedTo { get; set; }

    public virtual object Clone()
        => MemberwiseClone();
}
