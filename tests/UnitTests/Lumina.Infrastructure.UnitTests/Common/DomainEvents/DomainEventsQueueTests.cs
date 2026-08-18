#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Infrastructure.Common.DomainEvents;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.DomainEvents;

/// <summary>
/// Contains unit tests for the <see cref="DomainEventsQueue"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DomainEventsQueueTests
{
    private readonly DomainEventsQueue _sut = new();

    [Fact]
    public void TryDequeue_WhenQueueIsEmpty_ShouldReturnFalse()
    {
        // Act
        bool result = _sut.TryDequeue(out IDomainEvent domainEvent);

        // Assert
        Assert.False(result);
        Assert.Null(domainEvent);
    }

    [Fact]
    public void Enqueue_WhenCalled_ShouldDequeueTheSameDomainEvent()
    {
        // Arrange
        TestDomainEvent domainEvent = new(Guid.NewGuid(), DateTime.UtcNow);

        // Act
        _sut.Enqueue(domainEvent);
        bool result = _sut.TryDequeue(out IDomainEvent dequeuedEvent);

        // Assert
        Assert.True(result);
        Assert.Same(domainEvent, dequeuedEvent);
    }

    [Fact]
    public void Enqueue_WhenCalledMultipleTimes_ShouldDequeueInFifoOrder()
    {
        // Arrange
        TestDomainEvent firstEvent = new(Guid.NewGuid(), DateTime.UtcNow);
        TestDomainEvent secondEvent = new(Guid.NewGuid(), DateTime.UtcNow);
        TestDomainEvent thirdEvent = new(Guid.NewGuid(), DateTime.UtcNow);

        // Act
        _sut.Enqueue(firstEvent);
        _sut.Enqueue(secondEvent);
        _sut.Enqueue(thirdEvent);
        _sut.TryDequeue(out IDomainEvent firstDequeued);
        _sut.TryDequeue(out IDomainEvent secondDequeued);
        _sut.TryDequeue(out IDomainEvent thirdDequeued);

        // Assert
        Assert.Same(firstEvent, firstDequeued);
        Assert.Same(secondEvent, secondDequeued);
        Assert.Same(thirdEvent, thirdDequeued);
    }

    [Fact]
    public void Enqueue_WhenCalledWithMultipleEventTypes_ShouldDequeueAllTypes()
    {
        // Arrange
        TestDomainEvent testEvent = new(Guid.NewGuid(), DateTime.UtcNow);
        NestedTestDomainEvent nestedEvent = new(Guid.NewGuid(), DateTime.UtcNow);

        // Act
        _sut.Enqueue(testEvent);
        _sut.Enqueue(nestedEvent);
        _sut.TryDequeue(out IDomainEvent firstDequeued);
        _sut.TryDequeue(out IDomainEvent secondDequeued);

        // Assert
        Assert.Same(testEvent, firstDequeued);
        Assert.Same(nestedEvent, secondDequeued);
    }

    [Fact]
    public void TryDequeue_WhenAllEventsWereDequeued_ShouldReturnFalseOnNextCall()
    {
        // Arrange
        TestDomainEvent domainEvent = new(Guid.NewGuid(), DateTime.UtcNow);
        _sut.Enqueue(domainEvent);

        // Act
        _sut.TryDequeue(out _);
        bool result = _sut.TryDequeue(out IDomainEvent dequeuedEvent);

        // Assert
        Assert.False(result);
        Assert.Null(dequeuedEvent);
    }

    /// <summary>
    /// Test domain event used by the queue tests.
    /// </summary>
    private sealed record TestDomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent;

    /// <summary>
    /// A second test domain event used by the queue tests.
    /// </summary>
    private sealed record NestedTestDomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent;
}
