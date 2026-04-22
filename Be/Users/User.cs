using Be.Data;

namespace Be.Users;

public class User : IEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public DateTime RecordedFrom { get; set; }
    public DateTime RecordedTo { get; set; }
    public int Fact { get; set; }
}
