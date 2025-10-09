namespace Cs12DotNet8;

public class Page222
{
    public static void Run()
    {
        Write("Enter a number: ");
        string? input = ReadLine();

        try
        {
            Do(input);
        }
        catch (ArgumentOutOfRangeException e)
        {
            WriteLine($"input is not allowed because: {e.Message}");
        }
        catch (ArgumentException)
        {
            WriteLine("null or white space is not allowed");
        }
        catch (FormatException)
        {
            WriteLine("failed to parse input");
        }
        finally
        {
            WriteLine("Congratulation!");
        }
    }

    static void Do(string? input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        int number = int.Parse(input);

        ArgumentOutOfRangeException.ThrowIfEqual(number, 42);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(number, 37);
    }
}
