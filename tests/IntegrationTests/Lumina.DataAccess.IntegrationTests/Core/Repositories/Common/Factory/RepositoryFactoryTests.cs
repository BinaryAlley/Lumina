﻿#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Common.Factory;

/// <summary>
/// Contains integration tests for the <see cref="RepositoryFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RepositoryFactoryTests
{
    [Fact]
    public void RepositoryFactoryImplementsRepositoryFactoryInterface_InterfaceIsImplemented_ReturnsTrue()
    {
        // Arrange
        RepositoryFactory repositoryFactory = new(null!);

        // Assert
        Assert.True(repositoryFactory is IRepositoryFactory);
    }
}
