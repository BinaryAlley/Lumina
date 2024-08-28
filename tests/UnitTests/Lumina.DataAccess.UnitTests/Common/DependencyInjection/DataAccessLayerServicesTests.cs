#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="DataAccessLayerServices"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DataAccessLayerServicesTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="DataAccessLayerServicesTests"/> class.
    /// </summary>
    public DataAccessLayerServicesTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void AddDataAccessLayerServices_WhenBasePathExists_ShouldAddServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var basePath = Path.GetTempPath(); // Use a real existing path

        // Act
        DataAccessLayerServices.AddDataAccessLayerServices(services);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(LuminaDbContext));
        services.Should().Contain(sd => sd.ServiceType == typeof(IUnitOfWork) && sd.ImplementationType == typeof(UnitOfWork));
        services.Should().Contain(sd => sd.ServiceType == typeof(IRepositoryFactory) && sd.ImplementationType == typeof(RepositoryFactory));
        services.Should().Contain(sd => sd.ServiceType == typeof(IBookRepository) && sd.ImplementationType == typeof(BookRepository));
    }

    [Fact]
    public void AddDataAccessLayerServices_ShouldRegisterDbContextWithCorrectConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        DataAccessLayerServices.AddDataAccessLayerServices(services);

        // Assert
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LuminaDbContext>));
        descriptor.Should().NotBeNull();
        var serviceProvider = services.BuildServiceProvider();
        var context = serviceProvider.GetService<LuminaDbContext>();

        context.Should().NotBeNull();
        context!.Database.GetDbConnection().ConnectionString.Should().Contain("Lumina.db");
    }

    [Fact]
    public void AddDataAccessLayerServices_WhenCalled_ShouldRegisterRepositories()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<IBookRepository>();
        repository.Should().NotBeNull();
    }
    #endregion
}