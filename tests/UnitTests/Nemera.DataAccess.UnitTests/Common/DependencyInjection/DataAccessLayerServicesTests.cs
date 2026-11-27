#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Time;
using Nemera.DataAccess.Common.DependencyInjection;
using Nemera.DataAccess.Core.UoW;
using Nemera.DataAccess.UnitTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Nemera.DataAccess.UnitTests.Common.DependencyInjection;

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
        string basePath = Path.GetTempPath(); // Use a real existing path.

        // Act
        DataAccessLayerServices.AddDataAccessLayerServices(services);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(NemeraDbContext));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IUnitOfWork) && sd.ImplementationType == typeof(UnitOfWork));
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
        ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<NemeraDbContext>));
        Assert.NotNull(descriptor);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        NemeraDbContext? context = serviceProvider.GetService<NemeraDbContext>();

        Assert.NotNull(context);
        Assert.Contains("Nemera.db", context.Database.GetDbConnection().ConnectionString);
    }

    [Fact]
    public void AddDataAccessLayerServices_WhenCalled_ShouldExposeRepositoriesThroughUnitOfWork()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        DataAccessLayerServices.AddDataAccessLayerServices(services);

        services.AddTransient<ICurrentUserService, TestCurrentUserService>();
        services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        IUnitOfWork unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        Assert.NotNull(unitOfWork.BookRepository);
        Assert.NotNull(unitOfWork.PermissionRepository);
    }
}
