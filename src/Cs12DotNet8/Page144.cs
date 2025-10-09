namespace Cs12DotNet8;

public class Page144
{
    public static void Run()
    {
        WriteLine(GetMessage([]));
        WriteLine(GetMessage([42, 69]));
        WriteLine(GetMessage([10, 32]));
        WriteLine(GetMessage([37]));
    }

    private static string GetMessage(int[] numbers)
    => numbers switch
    {
        [] => "Empty",
        [42, 69] => "Fourty Two and Sixty Nine",
        [_, _] => "Any two numbers",
        _ => "Unknown",
    };
}
