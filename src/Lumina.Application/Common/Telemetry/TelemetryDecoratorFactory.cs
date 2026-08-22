#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Domain.Common.Events;
using System;
#endregion

namespace Lumina.Application.Common.Telemetry;

/// <summary>
/// Resolves the telemetry decorator type that wraps a given application handler contract.
/// </summary>
internal static class TelemetryDecoratorFactory
{
    /// <summary>
    /// Gets the closed telemetry decorator type for the specified handler contract.
    /// </summary>
    /// <param name="contract">The handler contract to wrap.</param>
    /// <returns>The closed generic telemetry decorator type for the contract.</returns>
    /// <exception cref="ArgumentException">Thrown when the contract has no matching telemetry decorator.</exception>
    public static Type GetDecoratorType(Type contract)
    {
        Type contractDefinition = contract.GetGenericTypeDefinition();
        Type[] typeArguments = contract.GetGenericArguments();

        if (contractDefinition == typeof(IQueryHandler<,>))
            return typeof(TelemetryQueryHandlerDecorator<,>).MakeGenericType(typeArguments);
        if (contractDefinition == typeof(ICommandHandler<,>))
            return typeof(TelemetryCommandHandlerDecorator<,>).MakeGenericType(typeArguments);
        if (contractDefinition == typeof(IDomainEventHandler<>))
            return typeof(TelemetryDomainEventHandlerDecorator<>).MakeGenericType(typeArguments);

        throw new ArgumentException($"No telemetry decorator is defined for the contract type '{contract}'.", nameof(contract));
    }
}
