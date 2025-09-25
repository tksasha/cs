namespace Cs12DotNet8;

public class Page96
{
    public static void Run()
    {
        int numberOfApples = 12;
        decimal pricePerApple = 0.35M;

        WriteLine(
            format: "{0} apples cost {1:C}",
            arg0: numberOfApples,
            arg1: pricePerApple * numberOfApples);

        string stringFormatter = string.Format(
            format: "{0} apples cost {1:C}",
            arg0: numberOfApples,
            arg1: pricePerApple * numberOfApples);

        WriteLine(stringFormatter);

        WriteLine("{0} apples cost {1:C}", numberOfApples, pricePerApple * numberOfApples);

        // string interpolation allows us to avoid boxing
        WriteLine($"{numberOfApples} apples cost {pricePerApple * numberOfApples:C}");
    }
}
