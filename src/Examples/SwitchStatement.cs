using Microsoft.Extensions.Logging;

namespace Examples;

#pragma warning disable IDE0066

class SwitchStatement(ILogger<SwitchStatement> logger)
{
    public void Run()
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

        using (logger.BeginScope("Run"))
        {
            logger.LogInformation("number = {Number}, word = {Word}", number, word);
        }
    }
}

#pragma warning restore IDE0066
