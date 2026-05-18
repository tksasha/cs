using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Beetles.Application.Tests.Validators;

/// <summary>
/// Direct unit tests for the <c>DateTimeOffsetValidator</c> used by the DELETE endpoint
/// (and indirectly by <c>WallRequestValidator</c>). The validator's single rule is:
/// the offset must be UTC (TimeSpan.Zero).
/// </summary>
public sealed class DateTimeOffsetValidatorTest : AbstractTest
{
    private readonly IValidator<DateTimeOffset> _validator;

    public DateTimeOffsetValidatorTest()
    {
        _validator = _serviceProvider.GetRequiredService<IValidator<DateTimeOffset>>();
    }

    [Fact]
    public async Task Utc_IsValid()
    {
        var value = new DateTimeOffset(2025, 5, 1, 12, 0, 0, TimeSpan.Zero);

        var result = await _validator.ValidateAsync(value, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task PositiveOffset_IsInvalid()
    {
        var value = new DateTimeOffset(2025, 5, 1, 12, 0, 0, TimeSpan.FromHours(5));

        var result = await _validator.ValidateAsync(value, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Must be UTC.");
    }

    [Fact]
    public async Task NegativeOffset_IsInvalid()
    {
        var value = new DateTimeOffset(2025, 5, 1, 12, 0, 0, TimeSpan.FromHours(-5));

        var result = await _validator.ValidateAsync(value, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Must be UTC.");
    }

    [Fact]
    public async Task FractionalOffset_IsInvalid()
    {
        // India Standard Time +05:30
        var value = new DateTimeOffset(2025, 5, 1, 12, 0, 0, new TimeSpan(5, 30, 0));

        var result = await _validator.ValidateAsync(value, CancellationToken.None);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task DefaultDateTimeOffset_IsValid()
    {
        // default(DateTimeOffset) has Offset == Zero and is therefore "UTC".
        // This documents the current behaviour — callers must be aware that omitting
        // the value passes validation.
        var result = await _validator.ValidateAsync(default, CancellationToken.None);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task MinValue_IsValid()
    {
        // DateTimeOffset.MinValue has Offset.Zero — should be accepted by the UTC rule.
        var result = await _validator.ValidateAsync(DateTimeOffset.MinValue, CancellationToken.None);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task MaxValue_IsValid()
    {
        var result = await _validator.ValidateAsync(DateTimeOffset.MaxValue, CancellationToken.None);

        Assert.True(result.IsValid);
    }
}
