namespace Cs12DotNet8;

public class Page78
{
    public static void Run()
    {
        string fullNameWithTabSeparator = "Bob\tSmith";

        WriteLine($"fullNameWithTabSeparator = {fullNameWithTabSeparator}");

        // won't work !!!
        // string filePath = "C:\televisions\sony\bravia.txt";

        // WriteLine(filePath);

        string filePath = @"C:\televisions\sony\bravia.txt";

        WriteLine(filePath);
    }
}
