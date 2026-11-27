#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DomainEvents;
using Lumina.Domain.Common.Events;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Domain events queue that exposes its enqueuing and dequeuing to a <see cref="DomainEventsQueueProbe"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ProbingDomainEventsQueue : IDomainEventsQueue
{
    private readonly ConcurrentQueue<IDomainEvent> _queue = new();
    private readonly DomainEventsQueueProbe _probe;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProbingDomainEventsQueue"/> class.
    /// </summary>
    /// <param name="probe">The probe that controls and observes the queue.</param>
    /// <param name="httpContextAccessor">Injected accessor used to identify the request that dequeues the queue.</param>
    public ProbingDomainEventsQueue(DomainEventsQueueProbe probe, IHttpContextAccessor httpContextAccessor)
    {
        _probe = probe;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Adds a domain event to the queue, before reporting it to the probe.
    /// </summary>
    /// <param name="domainEvent">The domain event to enqueue.</param>
    public void Enqueue(IDomainEvent domainEvent)
    {
        _queue.Enqueue(domainEvent);
        _probe.RecordEnqueue();
    }

    /// <summary>
    /// Attempts to remove and return the domain event at the beginning of the queue, reporting the attempt to the probe.
    /// </summary>
    /// <param name="domainEvent">When this method returns, contains the domain event removed from the queue, or the default value of <see cref="IDomainEvent"/> if the queue is empty.
    /// </param>
    /// <returns><see langword="true"/> if a domain event was successfully dequeued, <see langword="false"/> otherwise.</returns>
    public bool TryDequeue(out IDomainEvent domainEvent)
    {
        bool wasDequeued = _queue.TryDequeue(out domainEvent!);
        string path = _httpContextAccessor.HttpContext?.Request.Path.Value ?? string.Empty;
        _probe.RecordDequeue(path, wasDequeued);
        return wasDequeued;
    }
}
