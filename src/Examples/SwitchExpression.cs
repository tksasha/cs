using Microsoft.Extensions.Logging;

namespace Examples;

class SwitchExpression(ILogger<SwitchExpression> logger)
{
    public void Run()
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

        using (logger.BeginScope("Run"))
        {
            logger.LogInformation("number = {Number}, word = {Word}", number, word);
        }
    }
}
