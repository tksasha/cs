namespace Examples;

class SwitchExpression
{
    public static void Run()
    {
        int number = 42;

        string word = number switch
        {
            1 => "One",
            37 => "Thirty Seven",
            42 => "Fourty Two",
            69 => "Sixty Nine",
            _ => "Many",
        };

        WriteLine($"[{nameof(SwitchExpression)}.{nameof(Run)}] number = {number}, word = {word}");
    }
}
