namespace EFDemo.Models;

public class Blog
{
    public int Id { get; set; }
    public required string Url { get; set; }

    public IList<Post> Posts { get; } = [];
}
