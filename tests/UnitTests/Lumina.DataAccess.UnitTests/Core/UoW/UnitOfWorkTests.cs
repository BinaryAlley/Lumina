#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.UoW;

/// <summary>
/// Contains unit tests for the <see cref="UnitOfWork"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnitOfWorkTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWorkTests"/> class.
    /// </summary>
    public UnitOfWorkTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _fixture.Customize<LuminaDbContext>(composer => composer.FromFactory(() =>
        {
            DbContextOptions<LuminaDbContext> options = new DbContextOptionsBuilder<LuminaDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return Substitute.ForPartsOf<LuminaDbContext>(options);
        }));
    }

    [Fact]
    public void ResetRepositories_WhenCalled_ShouldClearAllRepositories()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        services.AddTransient<ICurrentUserService, TestCurrentUserService>();
        services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        UnitOfWork unitOfWork = (UnitOfWork)serviceProvider.GetRequiredService<IUnitOfWork>();

        // Act
        unitOfWork.ResetRepositories();

        // Assert
        Assert.Equal(0, unitOfWork.Repositories.Count);
    }

    [Fact]
    public void AddRepositories_WhenCalled_ShouldAddRepositories()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        services.AddTransient<ICurrentUserService, TestCurrentUserService>();
        services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        UnitOfWork unitOfWork = (UnitOfWork)serviceProvider.GetRequiredService<IUnitOfWork>();
        int initialCount = unitOfWork.Repositories.Count;

        // Act
        unitOfWork.ResetRepositories();
        unitOfWork.AddRepositories();

        // Assert
        Assert.True(unitOfWork.Repositories.Count > 0);
        Assert.Equal(initialCount, unitOfWork.Repositories.Count);
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldAddRepositories()
    {
        // Arrange
        IRepositoryFactory repositoryFactory = _fixture.Create<IRepositoryFactory>();
        LuminaDbContext dbContext = _fixture.Create<LuminaDbContext>();

        // Act
        UnitOfWork unitOfWork = new(repositoryFactory, dbContext);

        // Assert
        Assert.True(unitOfWork.Repositories.Count > 0);
    }

    [Fact]
    public void GetRepository_WhenRepositoryNotAdded_ShouldReturnNull()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        services.AddTransient<ICurrentUserService, TestCurrentUserService>();
        services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        UnitOfWork unitOfWork = (UnitOfWork)serviceProvider.GetRequiredService<IUnitOfWork>();
        unitOfWork.ResetRepositories();

        // Act
        IBookRepository result = unitOfWork.GetRepository<IBookRepository>();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetRepository_WhenRepositoriesAdded_ShouldReturnRepositories()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        services.AddTransient<ICurrentUserService, TestCurrentUserService>();
        services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        UnitOfWork unitOfWork = (UnitOfWork)serviceProvider.GetRequiredService<IUnitOfWork>();

        // Act & Assert
        Assert.NotNull(unitOfWork.GetRepository<IBookRepository>());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCalled_ShouldCallDbContextSaveChangesAsync()
    {
        // Arrange
        IRepositoryFactory repositoryFactory = _fixture.Create<IRepositoryFactory>();
        LuminaDbContext dbContext = _fixture.Create<LuminaDbContext>();
        UnitOfWork unitOfWork = new(repositoryFactory, dbContext);
        CancellationToken cancellationToken = new();

        // Act
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Assert
        await dbContext.Received(1).SaveChangesAsync(cancellationToken);
    }
}
