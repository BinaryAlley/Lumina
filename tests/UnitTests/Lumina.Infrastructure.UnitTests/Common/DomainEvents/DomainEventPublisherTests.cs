#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Infrastructure.Common.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.DomainEvents;

/// <summary>
/// Contains unit tests for the <see cref="DomainEventPublisher"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DomainEventPublisherTests
{
    public sealed record TestDomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent;

    public sealed record NestedTestDomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent;

    private sealed class RecordingDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        private readonly List<string> _callLog;
        private readonly string _handlerName;

        public RecordingDomainEventHandler(List<string> callLog, string handlerName)
        {
            _callLog = callLog;
            _handlerName = handlerName;
        }

        public ValueTask HandleAsync(TestDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            _callLog.Add(_handlerName);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Creates a publisher wired to a fresh service provider, optionally registering additional services.
    /// </summary>
    /// <param name="configureServices">Optional action used to register additional services into the provider.</param>
    /// <returns>A domain event publisher resolved from the created provider.</returns>
    private static IDomainEventPublisher CreatePublisher(Action<IServiceCollection>? configureServices = null)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        configureServices?.Invoke(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IDomainEventPublisher>();
    }

    /// <summary>
    /// Creates a valid instance of <see cref="TestDomainEvent"/>.
    /// </summary>
    /// <returns>The created domain event.</returns>
    private static TestDomainEvent CreateTestDomainEvent()
    {
        return new TestDomainEvent(Guid.NewGuid(), DateTime.UtcNow);
    }

    /// <summary>
    /// Creates a valid instance of <see cref="NestedTestDomainEvent"/>.
    /// </summary>
    /// <returns>The created domain event.</returns>
    private static NestedTestDomainEvent CreateNestedTestDomainEvent()
    {
        return new NestedTestDomainEvent(Guid.NewGuid(), DateTime.UtcNow);
    }

    [Fact]
    public async Task Publish_WhenHandlersAreRegistered_ShouldInvokeAllHandlersInOrder()
    {
        // Arrange
        TestDomainEvent domainEvent = CreateTestDomainEvent();
        List<string> callLog = [];
        IDomainEventPublisher publisher = CreatePublisher(services =>
        {
            services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => new RecordingDomainEventHandler(callLog, "first"));
            services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => new RecordingDomainEventHandler(callLog, "second"));
        });

        // Act
        await publisher.PublishAsync(domainEvent, CancellationToken.None);

        // Assert
        Assert.Equal(["first", "second"], callLog);
    }

    [Fact]
    public async Task Publish_WhenNoHandlersAreRegisteredForTheEventType_ShouldNotThrow()
    {
        // Arrange
        TestDomainEvent domainEvent = CreateTestDomainEvent();
        IDomainEventPublisher publisher = CreatePublisher();

        // Act
        await publisher.PublishAsync(domainEvent, CancellationToken.None);

        // Assert
        // reaching this point without an exception means the publish was a no-op, as expected
    }

    [Fact]
    public async Task Publish_WhenOnlyAnotherEventTypeHasHandlers_ShouldNotInvokeThem()
    {
        // Arrange
        TestDomainEvent domainEvent = CreateTestDomainEvent();
        IDomainEventHandler<NestedTestDomainEvent> nestedHandler = Substitute.For<IDomainEventHandler<NestedTestDomainEvent>>();
        IDomainEventPublisher publisher = CreatePublisher(services => services.AddScoped(_ => nestedHandler));

        // Act
        await publisher.PublishAsync(domainEvent, CancellationToken.None);

        // Assert
        await nestedHandler.DidNotReceive().HandleAsync(Arg.Any<NestedTestDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publish_WhenHandlerThrows_ShouldPropagateTheException()
    {
        // Arrange
        TestDomainEvent domainEvent = CreateTestDomainEvent();
        InvalidOperationException expectedException = new("Test exception");
        IDomainEventHandler<TestDomainEvent> handler = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        handler.HandleAsync(Arg.Any<TestDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => throw expectedException);
        IDomainEventPublisher publisher = CreatePublisher(services => services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => handler));

        // Act
        InvalidOperationException result = await Assert.ThrowsAsync<InvalidOperationException>(
            () => publisher.PublishAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Same(expectedException, result);
    }

    [Fact]
    public async Task Publish_WhenHandlerPublishesAnotherEvent_ShouldInvokeNestedHandlers()
    {
        // Arrange
        TestDomainEvent domainEvent = CreateTestDomainEvent();
        NestedTestDomainEvent nestedDomainEvent = CreateNestedTestDomainEvent();
        IDomainEventHandler<TestDomainEvent> handler = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        IDomainEventHandler<NestedTestDomainEvent> nestedHandler = Substitute.For<IDomainEventHandler<NestedTestDomainEvent>>();
        IDomainEventPublisher publisher = CreatePublisher(services =>
        {
            services.AddScoped(_ => handler);
            services.AddScoped(_ => nestedHandler);
        });
        handler.HandleAsync(domainEvent, Arg.Any<CancellationToken>())
            .Returns(callInfo => publisher.PublishAsync(nestedDomainEvent, callInfo.Arg<CancellationToken>()));

        // Act
        await publisher.PublishAsync(domainEvent, CancellationToken.None);

        // Assert
        await nestedHandler.Received(1).HandleAsync(nestedDomainEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publish_WhenCancellationRequested_ShouldPassTokenToHandlers()
    {
        // Arrange
        TestDomainEvent domainEvent = CreateTestDomainEvent();
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        IDomainEventHandler<TestDomainEvent> handler = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        IDomainEventPublisher publisher = CreatePublisher(services => services.AddScoped(_ => handler));

        // Act
        await publisher.PublishAsync(domainEvent, cancellationToken);

        // Assert
        await handler.Received(1).HandleAsync(domainEvent, Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task Publish_WhenDomainEventIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDomainEventPublisher publisher = CreatePublisher();

        // Act
        ArgumentNullException result = await Assert.ThrowsAsync<ArgumentNullException>(() => publisher.PublishAsync(null!, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal("domainEvent", result.ParamName);
    }
}
