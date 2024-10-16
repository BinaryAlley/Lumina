﻿
#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.UoW;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.UoW;

/// <summary>
/// Contains integration tests for the <see cref="UnitOfWork"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnitOfWorkTests
{
    [Fact]
    public void UnitOfWorkImplementsUnitOfWorkInterface_InterfaceIsImplemented_ReturnsTrue()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        UnitOfWork unitOfWork = (UnitOfWork)serviceProvider.GetRequiredService<IUnitOfWork>();

        // Assert
        Assert.True(unitOfWork is IUnitOfWork);
    }
}