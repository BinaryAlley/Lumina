#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.DataAccess.Common.DependencyInjection;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Provides the repository properties exposed by the unit of work.
    /// </summary>
    /// <returns>An enumerable of tuples containing the repository property name and its accessor.</returns>
    public static IEnumerable<object[]> GetRepositoryProperties()
    {
        yield return new object[] { nameof(IUnitOfWork.PermissionRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.PermissionRepository) };
        yield return new object[] { nameof(IUnitOfWork.RolePermissionRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.RolePermissionRepository) };
        yield return new object[] { nameof(IUnitOfWork.RoleRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.RoleRepository) };
        yield return new object[] { nameof(IUnitOfWork.UserRoleRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.UserRoleRepository) };
        yield return new object[] { nameof(IUnitOfWork.BookRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.BookRepository) };
        yield return new object[] { nameof(IUnitOfWork.DirectoryScanFingerprintRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.DirectoryScanFingerprintRepository) };
        yield return new object[] { nameof(IUnitOfWork.LibraryRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.LibraryRepository) };
        yield return new object[] { nameof(IUnitOfWork.LibraryScanRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.LibraryScanRepository) };
        yield return new object[] { nameof(IUnitOfWork.LibraryScanSnapshotRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.LibraryScanSnapshotRepository) };
        yield return new object[] { nameof(IUnitOfWork.LibraryScanStagingResultsRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.LibraryScanStagingResultsRepository) };
        yield return new object[] { nameof(IUnitOfWork.LibraryMetadataProviderConfigurationRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.LibraryMetadataProviderConfigurationRepository) };
        yield return new object[] { nameof(IUnitOfWork.PluginRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.PluginRepository) };
        yield return new object[] { nameof(IUnitOfWork.UserRepository), (Func<UnitOfWork, object>)(unitOfWork => unitOfWork.UserRepository) };
    }

    [Theory]
    [MemberData(nameof(GetRepositoryProperties))]
    public void RepositoryProperty_WhenAccessed_ShouldReturnRepositoryAndCacheInstance(string propertyName, Func<UnitOfWork, object> repositoryAccessor)
    {
        // Arrange
        UnitOfWork unitOfWork = CreateUnitOfWork();

        // Act
        object firstAccess = repositoryAccessor(unitOfWork);
        object secondAccess = repositoryAccessor(unitOfWork);

        // Assert
        Assert.NotNull(firstAccess);
        Assert.Same(firstAccess, secondAccess);
        Assert.EndsWith(propertyName, firstAccess.GetType().Name);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCalled_ShouldCallDbContextSaveChangesAsync()
    {
        // Arrange
        LuminaDbContext dbContext = _fixture.Create<LuminaDbContext>();
        UnitOfWork unitOfWork = new(dbContext);
        CancellationToken cancellationToken = new();

        // Act
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Assert
        await dbContext.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public void UnitOfWork_WhenResolvedFromServiceProvider_ShouldExposeRepositories()
    {
        // Arrange
        ServiceCollection services = new();
        DataAccessLayerServices.AddDataAccessLayerServices(services);
        services.AddTransient<ICurrentUserService, TestCurrentUserService>();
        services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        IUnitOfWork unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

        // Assert
        Assert.NotNull(unitOfWork.PermissionRepository);
        Assert.NotNull(unitOfWork.RoleRepository);
        Assert.NotNull(unitOfWork.UserRepository);
    }

    private UnitOfWork CreateUnitOfWork()
    {
        return new(_fixture.Create<LuminaDbContext>());
    }
}
