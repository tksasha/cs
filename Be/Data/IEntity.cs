namespace Be.Data;

public interface IEntity
{
    Guid Id { get; set; }
    DateTimeOffset ValidTo { get; set; }
    DateTimeOffset RecordedTo { get; set; }
    DateTimeOffset RecordedFrom { get; set; }
}
