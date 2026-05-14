using System.Globalization;

namespace Beetles.Infrastructure.Tests.Repositories;

public abstract class AbstractRepositoryTest
{
    protected static DateTimeOffset Date(string date)
        => DateTimeOffset.ParseExact(
            $"{date}, 00:00Z",
            "d MMM yyyy, HH:mm'Z'",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
