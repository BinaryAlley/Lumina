#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.Repositories.Common.Factory;
using Lumina.DataAccess.Core.UoW;
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
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
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
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void ResetRepositories_WhenCalled_ShouldClearAllRepositories()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        UnitOfWork unitOfWork = (UnitOfWork)serviceProvider.GetRequiredService<IUnitOfWork>();

        // Act
        unitOfWork.ResetRepositories();

        // Assert
        unitOfWork.Repositories.Count.Should().Be(0);
    }

    [Fact]
    public void AddRepositories_WhenCalled_ShouldAddRepositories()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        UnitOfWork unitOfWork = (UnitOfWork)serviceProvider.GetRequiredService<IUnitOfWork>();
        int initialCount = unitOfWork.Repositories.Count;

        // Act
        unitOfWork.ResetRepositories();
        unitOfWork.AddRepositories();

        // Assert
        unitOfWork.Repositories.Count.Should().BeGreaterThan(0);
        unitOfWork.Repositories.Count.Should().Be(initialCount);
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
        unitOfWork.Repositories.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetRepository_WhenRepositoryNotAdded_ShouldReturnNull()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        UnitOfWork unitOfWork = (UnitOfWork)serviceProvider.GetRequiredService<IUnitOfWork>();
        unitOfWork.ResetRepositories();

        // Act
        IBookRepository result = unitOfWork.GetRepository<IBookRepository>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRepository_WhenRepositoriesAdded_ShouldReturnRepositories()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        UnitOfWork unitOfWork = (UnitOfWork)serviceProvider.GetRequiredService<IUnitOfWork>();

        // Act & Assert
        unitOfWork.GetRepository<IBookRepository>().Should().NotBeNull();
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
    #endregion
}