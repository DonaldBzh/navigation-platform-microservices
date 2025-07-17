using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NavigationPlatform.Shared.Identity;
using NSubstitute;
using System.Security.Claims;

namespace NavigationPlatform.Shared.UnitTest.Identity;

public class CurrentUserServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly HttpContext _httpContext = Substitute.For<HttpContext>();
    private readonly HttpRequest _httpRequest = Substitute.For<HttpRequest>();
    private readonly IHeaderDictionary _headers = new HeaderDictionary();

    private readonly CurrentUserService _sut;

    public CurrentUserServiceTests()
    {
        _httpContext.Request.Returns(_httpRequest);
        _httpRequest.Headers.Returns(_headers);
        _httpContextAccessor.HttpContext.Returns(_httpContext);
        _sut = new CurrentUserService(_httpContextAccessor);
    }

    [Fact]
    public void GetUserId_WithValidGuidHeader_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        _headers["X-User-Id"] = expectedUserId.ToString();

        // Act
        var result = _sut.GetUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetUserId_WithMissingOrEmptyHeader_ThrowsUnauthorized(string? userIdHeader)
    {
        if (userIdHeader != null)
            _headers["X-User-Id"] = userIdHeader;

        var act = () => _sut.GetUserId();

        act.Should().Throw<UnauthorizedAccessException>()
           .WithMessage("User ID not found.");
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("12345")]
    [InlineData("not-a-guid")]
    public void GetUserId_WithInvalidGuidHeader_ThrowsUnauthorized(string invalidGuid)
    {
        _headers["X-User-Id"] = invalidGuid;

        var act = () => _sut.GetUserId();

        act.Should().Throw<UnauthorizedAccessException>()
           .WithMessage("Invalid User ID format");
    }
}
