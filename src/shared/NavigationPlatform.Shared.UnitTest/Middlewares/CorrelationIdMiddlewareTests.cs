using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NavigationPlatform.Shared.Middlewares;

namespace NavigationPlatform.Shared.UnitTest.Middlewares
{
    public class CorrelationIdMiddlewareTests
    {
        private const string CorrelationIdHeader = "X-Correlation-Id";

        [Fact]
        public async Task InvokeAsync_WhenHeaderIsMissing_ShouldGenerateCorrelationId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var called = false;
            var middleware = new CorrelationIdMiddleware(ctx =>
            {
                called = true;
                return Task.CompletedTask;
            });

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            called.Should().BeTrue();
            context.Response.Headers.Should().ContainKey(CorrelationIdHeader);
            context.Response.Headers[CorrelationIdHeader].ToString().Should().NotBeNullOrEmpty();
            Guid.TryParse(context.Response.Headers[CorrelationIdHeader], out var guid).Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_WhenHeaderIsPresent_ShouldUseExistingCorrelationId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var existingId = Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationIdHeader] = existingId;

            var middleware = new CorrelationIdMiddleware(ctx => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Headers[CorrelationIdHeader].ToString().Should().Be(existingId);
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNextDelegate()
        {
            // Arrange
            var wasCalled = false;
            var context = new DefaultHttpContext();

            var middleware = new CorrelationIdMiddleware(ctx =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            wasCalled.Should().BeTrue();
        }

    }
}
