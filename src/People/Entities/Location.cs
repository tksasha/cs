namespace People.Entities;

public class Location : IBitemporalEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
