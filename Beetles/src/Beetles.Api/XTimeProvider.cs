using Microsoft.Extensions.Options;

internal sealed class XTimeOptions
{
    public string? FakeUtcNow { get; set; }
}

internal class XTimeProvider(IOptionsMonitor<XTimeOptions> options) : TimeProvider
{
    public override DateTimeOffset GetUtcNow()
    {
        string? fake = options.CurrentValue.FakeUtcNow;

        return fake is not null
            ? DateTimeOffset.Parse(fake)
            : DateTimeOffset.UtcNow;
    }
}
