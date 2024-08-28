#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Common.Factory;

/// <summary>
/// Contains unit tests for the <see cref="RepositoryFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RepositoryFactoryTests
{
    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void CreateRepository_WhenCalled_ShouldReturnCorrectRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        var serviceProvider = services.BuildServiceProvider();
        var repositoryFactory = new RepositoryFactory(serviceProvider);

        // Act
        var result = repositoryFactory.CreateRepository<IBookRepository>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IBookRepository>();
    }

    [Fact]
    public void CreateRepository_WhenUnregisteredTypeRequested_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        var serviceProvider = services.BuildServiceProvider();
        var repositoryFactory = new RepositoryFactory(serviceProvider);

        // Act & Assert
        Action act = () => repositoryFactory.CreateRepository<IUnregisteredRepository>();
        act.Should().Throw<InvalidOperationException>().WithMessage("No service for type 'Lumina.DataAccess.UnitTests.Core.Repositories.Common.Factory.IUnregisteredRepository' has been registered.");
    }
    #endregion
}

public interface IUnregisteredRepository { }