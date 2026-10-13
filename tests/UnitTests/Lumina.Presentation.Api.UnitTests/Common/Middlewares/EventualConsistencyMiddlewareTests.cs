#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Common.Http;
using Lumina.Presentation.Api.Common.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Common.Middlewares;

/// <summary>
/// Contains unit tests for the <see cref="EventualConsistencyMiddleware"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EventualConsistencyMiddlewareTests
{
    private readonly EventualConsistencyMiddleware _sut;
    private bool _nextInvoked;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventualConsistencyMiddlewareTests"/> class.
    /// </summary>
    public EventualConsistencyMiddlewareTests()
    {
        ILogger<EventualConsistencyMiddleware> mockLogger = Substitute.For<ILogger<EventualConsistencyMiddleware>>();
        _sut = new EventualConsistencyMiddleware(
            context =>
            {
                _nextInvoked = true;
                return Task.CompletedTask;
            },
            mockLogger);
    }

    [Fact]
    public async Task InvokeAsync_WhenEndpointOptsOutThroughMetadata_ShouldInvokeNextWithoutTouchingTheDatabase()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.SetEndpoint(new Endpoint(
            requestDelegate: null,
            new EndpointMetadataCollection(new SkipEventualConsistencyMetadata()),
            displayName: "GetTrackStream"));

        // Act
        // The database, the domain event publisher and the domain events queue are deliberately not supplied: an endpoint that opted out through
        // metadata must invoke the next delegate without touching any of them, which is exactly what a long lived stream needs.
        await _sut.InvokeAsync(context, null!, null!, null!);

        // Assert
        Assert.True(_nextInvoked);
    }
}
