namespace Cs12DotNet8;

public class Page161
{
    public static void Run()
    {
    begin:
        Write("Enter a number (q for exit): ");

        string? input = ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            WriteLine("Input is empty");

            goto begin;
        }

        if (input == "q") return;

        int number = default;

        try
        {
            if (!int.TryParse(input, out number))
            {
                WriteLine("Input is not a number");

                goto begin;
            }

            Do(number);
        }
        catch (Exception) when (number == 42)
        {
            WriteLine("Fourty Two is not allowed");
        }
        catch (Exception) when (number == 69)
        {
            WriteLine("Sixty Nine is not allowed");
        }

    }

    static void Do(int n)
    {
        if (n == 42) throw new Exception();

        if (n == 69) throw new Exception();

        throw new Exception();
    }
}
