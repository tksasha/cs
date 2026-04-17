namespace EFDemo.Models;

public class Student
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public int GradeId { get; set; }
    public Grade? Grade { get; set; }
}
