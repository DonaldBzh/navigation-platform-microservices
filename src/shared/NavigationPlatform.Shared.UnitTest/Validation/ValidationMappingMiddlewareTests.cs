using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Validation;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NavigationPlatform.Shared.UnitTest.Validation;

public class ValidationMappingMiddlewareTests
{
    private readonly DefaultHttpContext _httpContext;
    private readonly RequestDelegate _next;
    private readonly ValidationMappingMiddleware _middleware;

    public ValidationMappingMiddlewareTests()
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
        _next = Substitute.For<RequestDelegate>();
        _middleware = new ValidationMappingMiddleware(_next);
    }

    [Fact]
    public async Task InvokeAsync_WithValidationException_ShouldReturn400AndValidationErrors()
    {
        // Arrange
        var errors = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Property1", "Error1"),
            new("Property2", "Error2")
        };

        _next.Invoke(Arg.Any<HttpContext>()).Throws(new ValidationException(errors));

        // Act
        await _middleware.InvokeAsync(_httpContext);
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

        // Assert
        _httpContext.Response.StatusCode.Should().Be(400);
        response.Should().Contain("Property1").And.Contain("Error1");
    }

    [Fact]
    public async Task InvokeAsync_WithNotFoundException_ShouldReturn404()
    {
        _next.Invoke(Arg.Any<HttpContext>()).Throws(new NotFoundException("not found"));

        await _middleware.InvokeAsync(_httpContext);
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

        _httpContext.Response.StatusCode.Should().Be(404);
        response.Should().Contain("not found");
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthorizedAccessException_ShouldReturn401()
    {
        _next.Invoke(Arg.Any<HttpContext>()).Throws(new UnauthorizedAccessException("unauthorized"));

        await _middleware.InvokeAsync(_httpContext);
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

        _httpContext.Response.StatusCode.Should().Be(401);
        response.Should().Contain("unauthorized");
    }

    [Fact]
    public async Task InvokeAsync_WithForbiddenAccessException_ShouldReturn403()
    {
        _next.Invoke(Arg.Any<HttpContext>()).Throws(new ForbiddenAccessException("forbidden"));

        await _middleware.InvokeAsync(_httpContext);
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

        _httpContext.Response.StatusCode.Should().Be(403);
        response.Should().Contain("forbidden");
    }

    [Fact]
    public async Task InvokeAsync_WithConflictException_ShouldReturn409()
    {
        _next.Invoke(Arg.Any<HttpContext>()).Throws(new ConflictException("conflict"));

        await _middleware.InvokeAsync(_httpContext);
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

        _httpContext.Response.StatusCode.Should().Be(409);
        response.Should().Contain("conflict");
    }

    [Fact]
    public async Task InvokeAsync_WithGoneException_ShouldReturn410()
    {
        _next.Invoke(Arg.Any<HttpContext>()).Throws(new GoneException("gone"));

        await _middleware.InvokeAsync(_httpContext);
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

        _httpContext.Response.StatusCode.Should().Be(410);
        response.Should().Contain("gone");
    }

    [Fact]
    public async Task InvokeAsync_WithUnknownException_ShouldReturn500()
    {
        _next.Invoke(Arg.Any<HttpContext>()).Throws(new Exception("unexpected"));

        await _middleware.InvokeAsync(_httpContext);
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

        _httpContext.Response.StatusCode.Should().Be(500);
        response.Should().Contain("unexpected error");
    }

    //[Fact]
    //public async Task InvokeAsync_WhenNoExceptionOccurs_ShouldCallNextAndNotModifyResponse()
    //{
    //    // Arrange
    //    var wasCalled = false;
    //    var next = new RequestDelegate(ctx =>
    //    {
    //        wasCalled = true;
    //        return Task.CompletedTask;
    //    });

    //    var middleware = new ValidationMappingMiddleware(next);

    //    // Act
    //    await middleware.InvokeAsync(_httpContext);

    //    // Assert
    //    wasCalled.Should().BeTrue();
    //    _httpContext.Response.StatusCode.Should().Be(200); 
    //}

    //[Fact]
    //public async Task InvokeAsync_WithUnhandledException_ShouldReturn500WithGenericMessage()
    //{
    //    _next.Invoke(Arg.Any<HttpContext>()).Throws(new InvalidOperationException("something went wrong"));

    //    await _middleware.InvokeAsync(_httpContext);
    //    _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
    //    var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

    //    _httpContext.Response.StatusCode.Should().Be(500);
    //    response.Should().Contain("unexpected error");
    //}

    //[Fact]
    //public async Task InvokeAsync_WithValidationExceptionWithoutErrors_ShouldReturn400WithEmptyArray()
    //{
    //    _next.Invoke(Arg.Any<HttpContext>()).Throws(new ValidationException(new List<FluentValidation.Results.ValidationFailure>()));

    //    await _middleware.InvokeAsync(_httpContext);
    //    _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
    //    var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

    //    _httpContext.Response.StatusCode.Should().Be(400);
    //    response.Should().Contain("\"errors\":[]");
    //}

    //[Fact]
    //public async Task InvokeAsync_CalledTwiceWithSuccess_ShouldWorkBothTimes()
    //{
    //    var called = 0;
    //    var next = new RequestDelegate(ctx =>
    //    {
    //        called++;
    //        return Task.CompletedTask;
    //    });

    //    var middleware = new ValidationMappingMiddleware(next);

    //    await middleware.InvokeAsync(_httpContext);
    //    await middleware.InvokeAsync(_httpContext);

    //    called.Should().Be(2);
    //}
}