namespace Examples;

static class Events
{
    sealed class Button
    {
        public event EventHandler? ClickCallback;

        public int Count;

        public void Click()
        {
            Count++;

            ClickCallback?.Invoke(this, EventArgs.Empty);
        }
    }

    private static void Button_Click(object? sender, EventArgs eventArgs)
    {
        if (sender is not Button button) return;

        WriteLine($"button.Count = {button.Count}");
    }

    public static void Run()
    {
        var button = new Button();

        button.ClickCallback += Button_Click;

        for (int i = 0; i < 10; i++) button.Click();
    }
}
