namespace Beetles.Application.Tests.Database;

public sealed class TestTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow = DateTimeOffset.MinValue;

    public void SetUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public void Reset()
    {
        _utcNow = DateTimeOffset.MinValue;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
