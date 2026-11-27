#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Telemetry;
using Nemera.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Nemera.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Domain.Common.Events;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Telemetry;

/// <summary>
/// Contains unit tests for the <see cref="TelemetryDecoratorFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TelemetryDecoratorFactoryTests
{
    [Fact]
    public void GetDecoratorType_WhenContractIsQueryHandler_ShouldReturnQueryDecoratorType()
    {
        // Arrange
        Type contract = typeof(IQueryHandler<GetRolePermissionsQuery, Result<RolePermissionsResponse>>);

        // Act
        Type result = TelemetryDecoratorFactory.GetDecoratorType(contract);

        // Assert
        Assert.Equal(typeof(TelemetryQueryHandlerDecorator<GetRolePermissionsQuery, Result<RolePermissionsResponse>>), result);
    }

    [Fact]
    public void GetDecoratorType_WhenContractIsCommandHandler_ShouldReturnCommandDecoratorType()
    {
        // Arrange
        Type contract = typeof(ICommandHandler<AddRoleCommand, Result<RolePermissionsResponse>>);

        // Act
        Type result = TelemetryDecoratorFactory.GetDecoratorType(contract);

        // Assert
        Assert.Equal(typeof(TelemetryCommandHandlerDecorator<AddRoleCommand, Result<RolePermissionsResponse>>), result);
    }

    [Fact]
    public void GetDecoratorType_WhenContractIsDomainEventHandler_ShouldReturnDomainEventHandlerDecoratorType()
    {
        // Arrange
        Type contract = typeof(IDomainEventHandler<LibrarySavedDomainEvent>);

        // Act
        Type result = TelemetryDecoratorFactory.GetDecoratorType(contract);

        // Assert
        Assert.Equal(typeof(TelemetryDomainEventHandlerDecorator<LibrarySavedDomainEvent>), result);
    }

    [Fact]
    public void GetDecoratorType_WhenContractIsNotSupported_ShouldThrowArgumentException()
    {
        // Arrange
        Type contract = typeof(IEnumerable<int>);

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => TelemetryDecoratorFactory.GetDecoratorType(contract));
        Assert.Contains("No telemetry decorator is defined", exception.Message);
    }
}
