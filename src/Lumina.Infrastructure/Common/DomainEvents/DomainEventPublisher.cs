#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Common.DomainEvents;

/// <summary>
/// Publishes domain events to all handlers registered for the event type, by resolving them from the dependency injection container.
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
    private static readonly ConcurrentDictionary<Type, Func<IDomainEvent, IServiceProvider, CancellationToken, Task>> s_dispatchers = new();

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventPublisher"/> class.
    /// </summary>
    /// <param name="serviceProvider">Injected provider used to resolve the domain event handlers.</param>
    public DomainEventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Publishes the specified <paramref name="domainEvent"/> to all its registered handlers, in registration order.
    /// </summary>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public ValueTask PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        Func<IDomainEvent, IServiceProvider, CancellationToken, Task> dispatcher = s_dispatchers.GetOrAdd(domainEvent.GetType(), static eventType =>
        {
            MethodInfo dispatchMethod = typeof(DomainEventPublisher).GetMethod(nameof(DispatchAsync), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(eventType);
            return dispatchMethod.CreateDelegate<Func<IDomainEvent, IServiceProvider, CancellationToken, Task>>();
        });

        return new ValueTask(dispatcher(domainEvent, _serviceProvider, cancellationToken));
    }

    /// <summary>
    /// Dispatches the specified <paramref name="domainEvent"/> to all handlers registered for <typeparamref name="TDomainEvent"/>.
    /// </summary>
    /// <typeparam name="TDomainEvent">The type of the domain event being dispatched.</typeparam>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <param name="serviceProvider">The provider used to resolve the domain event handlers.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task DispatchAsync<TDomainEvent>(IDomainEvent domainEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        where TDomainEvent : IDomainEvent
    {
        foreach (IDomainEventHandler<TDomainEvent> handler in serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>())
            await handler.HandleAsync((TDomainEvent)domainEvent, cancellationToken).ConfigureAwait(false);
    }
}
