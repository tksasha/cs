namespace Examples;

class ParamsKeyword
{
    private static void AsArray(int[] numbers)
    {
        foreach (int number in numbers)
        {
            WriteLine($"[{nameof(ParamsKeyword)}.{nameof(AsArray)}] number = {number}");
        }
    }

    private static void AsParams(params int[] numbers)
    {
        foreach (int number in numbers)
        {
            WriteLine($"[{nameof(ParamsKeyword)}.{nameof(AsParams)}] number = {number}");
        }
    }

    public static void Run()
    {
        int[] numbers = { 1, 2, 3 };

        AsArray(numbers);

        AsParams(numbers);
        AsParams(4, 5, 6, 7, 8, 9);
    }
}
