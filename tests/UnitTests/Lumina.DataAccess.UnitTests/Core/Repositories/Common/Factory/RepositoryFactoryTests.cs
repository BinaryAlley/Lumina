#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using Lumina.DataAccess.UnitTests.Common.Setup;
using Lumina.DataAccess.UnitTests.Core.Repositories.Common.Factory.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Common.Factory;

/// <summary>
/// Contains unit tests for the <see cref="RepositoryFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RepositoryFactoryTests
{
    [Fact]
    public void CreateRepository_WhenCalled_ShouldReturnCorrectRepository()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);

        services.AddTransient<ICurrentUserService, TestCurrentUserService>();
        services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        RepositoryFactory repositoryFactory = new(serviceProvider);

        // Act
        IBookRepository result = repositoryFactory.CreateRepository<IBookRepository>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IBookRepository>();
    }

    [Fact]
    public void CreateRepository_WhenUnregisteredTypeRequested_ShouldThrowException()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        RepositoryFactory repositoryFactory = new(serviceProvider);

        // Act & Assert
        Action act = () => repositoryFactory.CreateRepository<IUnregisteredRepository>();
        act.Should().Throw<InvalidOperationException>().WithMessage("No service for type 'Lumina.DataAccess.UnitTests.Core.Repositories.Common.Factory.Fixtures.IUnregisteredRepository' has been registered.");
    }
}
