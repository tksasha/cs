namespace Cs12DotNet8;

public class Page162
{
    public static void Run()
    {
        int x = int.MaxValue;

        WriteLine($"x = {x}");

        WriteLine($"x++ = {x++}");

        checked
        {
            int y = int.MaxValue;

            WriteLine($"y = {y}");

            try
            {
                WriteLine($"y++ = {y++}");
            }
            catch (OverflowException)
            {
                WriteLine("Cannot increment y because it has a maximum value");
            }
        }
    }
}
