namespace Beetles.Domain.Entities;

public class BeetleColony : IBitemporalEntity
{
    public int BeetleId { get; set; }
    public Beetle? Beetle { get; set; }

    public int ColonyId { get; set; }
    public Colony? Colony { get; set; }

    public required DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }

    public DateTimeOffset RecordedFrom { get; set; }
    public DateTimeOffset RecordedTo { get; set; }
}
