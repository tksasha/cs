namespace Examples;

#pragma warning disable IDE0066

class SwitchStatement
{
    public static void Run()
    {
        int number = 42;

        string word = default!;

        switch (number)
        {
            case 37:
                word = "Thirty Seven";
                break;
            case 42:
                word = "Fourty Two";
                break;
            default:
                word = "Many";
                break;
        }

        WriteLine($"[{nameof(SwitchStatement)}.{nameof(Run)}] number = {number}, word = {word}");
    }
}

#pragma warning restore IDE0066
