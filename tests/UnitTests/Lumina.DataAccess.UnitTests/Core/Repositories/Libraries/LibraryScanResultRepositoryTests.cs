#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.Repositories.Libraries;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Libraries;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanResultRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanResultRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly LibraryScanResultRepository _sut;
    private readonly LibraryScanResultEntityFixture _libraryScanResultEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanResultRepositoryTests"/> class.
    /// </summary>
    public LibraryScanResultRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new LibraryScanResultRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenCalled_ShouldAddScanResultToContextAndReturnCreated()
    {
        // Arrange
        LibraryScanResultEntity libraryScanResult = _libraryScanResultEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(libraryScanResult, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<LibraryScanResultEntity>? addedScanResult = _mockContext.ChangeTracker.Entries<LibraryScanResultEntity>()
            .FirstOrDefault(entityEntry => entityEntry.State == EntityState.Added && entityEntry.Entity.Id == libraryScanResult.Id);
        Assert.NotNull(addedScanResult);
    }
}
