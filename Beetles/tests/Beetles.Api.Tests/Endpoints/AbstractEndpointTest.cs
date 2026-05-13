using System.Globalization;

namespace Beetles.Api.Tests.Endpoints;

public abstract class AbstractEndpointTest
{
    protected static DateTimeOffset Date(string date)
        => DateTimeOffset.ParseExact(
            $"{date}, 00:00Z",
            "d MMM yyyy, HH:mm'Z'",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
