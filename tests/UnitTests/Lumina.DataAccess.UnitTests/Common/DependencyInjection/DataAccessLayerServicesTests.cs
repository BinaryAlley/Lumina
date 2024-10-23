#region ========================================================================= USING =====================================================================================
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
using System.IO;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="DataAccessLayerServices"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DataAccessLayerServicesTests
{
    [Fact]
    public void AddDataAccessLayerServices_WhenBasePathExists_ShouldAddServices()
    {
        // Arrange
        ServiceCollection services = new();
        string basePath = Path.GetTempPath(); // use a real existing path

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
        ServiceCollection services = new();

        // Act
        DataAccessLayerServices.AddDataAccessLayerServices(services);

        // Assert
        ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LuminaDbContext>));
        descriptor.Should().NotBeNull();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        LuminaDbContext? context = serviceProvider.GetService<LuminaDbContext>();

        context.Should().NotBeNull();
        context!.Database.GetDbConnection().ConnectionString.Should().Contain("Lumina.db");
    }

    [Fact]
    public void AddDataAccessLayerServices_WhenCalled_ShouldRegisterRepositories()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        IBookRepository? repository = serviceProvider.GetService<IBookRepository>();
        repository.Should().NotBeNull();
    }
}
