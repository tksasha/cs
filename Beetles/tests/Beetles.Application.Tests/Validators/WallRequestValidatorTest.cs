using Beetles.Application.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Beetles.Application.Tests.Validators;

public sealed class WallRequestValidatorTest : BaseTest
{
    private readonly IValidator<WallRequest> _validator;

    public WallRequestValidatorTest()
    {
        _validator = _serviceProvider.GetRequiredService<IValidator<WallRequest>>();
    }

    [Fact]
    public async Task Color_ShouldNotBeEmpty()
    {
        var request = new WallRequest { Color = "" };

        var result = await _validator.ValidateAsync(request, CancellationToken.None);

        var errors = result.Errors.Select(e => e.ErrorMessage);

        Assert.Multiple(
            () => Assert.False(result.IsValid),
            () => Assert.Contains("'Color' must not be empty.", errors)
        );
    }

    [Fact]
    public async Task BusinessStart_ShouldBeUtc()
    {
        var request = new WallRequest
        {
            Color = "yellow",
            BusinessStart = DateTimeOffset.Parse("2026-05-07")
        };

        var response = await _validator.ValidateAsync(request, CancellationToken.None);

        var errors = response.Errors.Select(e => e.ErrorMessage);

        Assert.Multiple(
            () => Assert.False(response.IsValid),
            () => Assert.Contains("Must be UTC.", errors)
        );
    }

    [Fact]
    public async Task ShouldBeValid()
    {
        var request = new WallRequest
        {
            Color = "red",
            BusinessStart = DateTimeOffset.Parse("2026-05-07T12:34:56Z"),
        };

        var response = await _validator.ValidateAsync(request, CancellationToken.None);

        Assert.Multiple(
            () => Assert.True(response.IsValid),
            () => Assert.Empty(response.Errors)
        );
    }
}
