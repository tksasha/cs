namespace Beetles.Domain.Entities;

public abstract class BitemporalEntity : Entity, ICloneable
{
    public DateTimeOffset BusinessStart { get; set; }
    public DateTimeOffset? BusinessEnd { get; set; }

    public DateTimeOffset SystemStart { get; set; }
    public DateTimeOffset SystemEnd { get; set; }

    public virtual object Clone()
        => MemberwiseClone();
}
