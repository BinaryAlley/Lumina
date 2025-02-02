#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Common.Setup;
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
        Assert.Contains(services, sd => sd.ServiceType == typeof(LuminaDbContext));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IUnitOfWork) && sd.ImplementationType == typeof(UnitOfWork));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IRepositoryFactory) && sd.ImplementationType == typeof(RepositoryFactory));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IBookRepository) && sd.ImplementationType == typeof(BookRepository));
    }

    [Fact]
    public void AddDataAccessLayerServices_ShouldRegisterDbContextWithCorrectConnectionString()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        services.AddTransient<ICurrentUserService, TestCurrentUserService>();
        services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();

        // Assert
        ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LuminaDbContext>));
        Assert.NotNull(descriptor);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        LuminaDbContext? context = serviceProvider.GetService<LuminaDbContext>();

        Assert.NotNull(context);
        Assert.Contains("Lumina.db", context.Database.GetDbConnection().ConnectionString);
    }

    [Fact]
    public void AddDataAccessLayerServices_WhenCalled_ShouldRegisterRepositories()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        DataAccessLayerServices.AddDataAccessLayerServices(services);

        services.AddTransient<ICurrentUserService, TestCurrentUserService>();
        services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        IBookRepository? repository = serviceProvider.GetService<IBookRepository>();
        Assert.NotNull(repository);
    }
}
