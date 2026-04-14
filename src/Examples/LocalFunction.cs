namespace Examples;

class LocalFunction
{
    public static void Run()
    {
        int x = 69;

        void callMe(int number)
        {
            WriteLine($"x = {x}"); // w/o access to x should be static

            string word = number switch
            {
                1 => "One",
                2 => "Two",
                _ => "Many"
            };

            WriteLine($"[{nameof(LocalFunction)}.{nameof(Run)}] word = {word}");

        }
        ;

        callMe(2);
    }
}
