namespace EFDemo.Models;

public class Grade
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public IList<Student> Students { get; set; } = [];
}
