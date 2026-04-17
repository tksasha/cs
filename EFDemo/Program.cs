using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static System.Console;

using EFDemo;
using EFDemo.Models;
using Microsoft.EntityFrameworkCore;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("Development")));

var serviceProvider = services.BuildServiceProvider();

using var context = serviceProvider.GetRequiredService<DatabaseContext>();

context.Database.EnsureCreated();

// Create
{
    var blog = new Blog { Url = "http://blogs.msdn.com/adonet" };
    var post = new Post { Title = "My first post", Content = "Lorem ipsum ...", Blog = blog };

    context.Add(blog);
    context.Add(post);
}
await context.SaveChangesAsync();

foreach (var blog in context.Blogs)
{
    WriteLine($"found: {blog.Id}, {blog.Url}");
}

// Read
// WriteLine("Querying for a blog");
// Blog? blog = await context.Blogs.OrderBy(b => b.Id).FirstOrDefaultAsync();
// if (blog is null)
// {
//     WriteLine($"failed to get first Blog");

//     return;
// }

// WriteLine($"found blog.Id = {blog.Id}");

// BlogRepository blogRepository = new(context.Blogs);

// if (blogRepository.TryGetById(blog.Id, out Blog other))
// {
//     WriteLine($"my repository is working, other.Id = {other.Id}");
// }

// // Update
// WriteLine("Updating the Blog and adding a Post");
// blog.Url = "https://devblogs.microsoft.com/dotnet";
// blog.Posts.Add(new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
// await context.SaveChangesAsync();

// Deleting
foreach (var blog in context.Blogs)
{
    WriteLine($"delete blog #{blog.Id}");

    context.Remove(blog);
}

foreach (var post in context.Posts)
{
    WriteLine($"delete post #{post.Id}");

    context.Remove(post);
}

await context.SaveChangesAsync();


WriteLine("horsing around");

var a = new { Name = "John McClane" };
