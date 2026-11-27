#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.Models.Core;

/// <summary>
/// Contains unit tests for the <see cref="AggregateRoot{TId}"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AggregateRootTests
{
    [Fact]
    public void GetDomainEvents_WhenNoEventsWereRaised_ShouldReturnEmptyList()
    {
        // Arrange
        TestAggregateRoot aggregateRoot = new(Guid.NewGuid());

        // Act
        List<IDomainEvent> domainEvents = aggregateRoot.GetDomainEvents();

        // Assert
        Assert.Empty(domainEvents);
    }

    [Fact]
    public void GetDomainEvents_WhenEventsWereRaised_ShouldReturnAllEvents()
    {
        // Arrange
        TestAggregateRoot aggregateRoot = new(Guid.NewGuid());
        aggregateRoot.RaiseTestEvent();
        aggregateRoot.RaiseTestEvent();

        // Act
        List<IDomainEvent> domainEvents = aggregateRoot.GetDomainEvents();

        // Assert
        Assert.Equal(2, domainEvents.Count);
        Assert.All(domainEvents, domainEvent => Assert.IsType<TestDomainEvent>(domainEvent));
    }

    [Fact]
    public void GetDomainEvents_WhenCalled_ShouldClearTheStoredEvents()
    {
        // Arrange
        TestAggregateRoot aggregateRoot = new(Guid.NewGuid());
        aggregateRoot.RaiseTestEvent();

        // Act
        aggregateRoot.GetDomainEvents();
        List<IDomainEvent> domainEvents = aggregateRoot.GetDomainEvents();

        // Assert
        Assert.Empty(domainEvents);
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    private sealed class TestAggregateRoot : AggregateRoot<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAggregateRoot"/> class.
        /// </summary>
        /// <param name="id">The id of the aggregate root.</param>
        public TestAggregateRoot(Guid id) : base(id)
        {
        }

        /// <summary>
        /// Raises a <see cref="TestDomainEvent"/> on this aggregate root.
        /// </summary>
        public void RaiseTestEvent()
        {
            _domainEvents.Add(new TestDomainEvent(Guid.NewGuid(), DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Test implementation of the <see cref="IDomainEvent"/> interface.
    /// </summary>
    private sealed record TestDomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent;
}
